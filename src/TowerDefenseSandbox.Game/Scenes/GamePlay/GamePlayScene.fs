namespace TowerDefenseSandbox.Game.Scenes

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open Fame
open Fame.Input
open Fame.Messaging
open Fame.Scene
open Fame.Graphics

open TowerDefenseSandbox.Game
open TowerDefenseSandbox.Game.Entities

type GamePlayInteractionMessage (x: int, y: int) =
    member _.X = x
    member _.Y = y

type GamePlayInteractionMessageHandler(interact: int -> int -> unit) =
    interface IMessageHandler<GamePlayInteractionMessage> with
        member _.Handle(message: GamePlayInteractionMessage) =
            interact message.X message.Y

type EnemyCreatedMessageHandler (entityProvier: IEntityProvider) =
    
    interface IMessageHandler<EnemyCreatedMessage> with

        member _.Handle (message: EnemyCreatedMessage) =
            entityProvier.RegisterEntity message.Enemy

type EnemyKilledMessageHandler (entityProvier: IEntityProvider, updatePixels: int -> unit) =
    
    interface IMessageHandler<EnemyKilledMessage> with

        member _.Handle (message: EnemyKilledMessage) =
            entityProvier.RemoveEntity message.Enemy

            updatePixels message.Pixels

type TurretCreatedMessageHandler(updatePixels: int -> unit) =
    interface IMessageHandler<TurretCreatedMessage> with

       member _.Handle (message: TurretCreatedMessage) =
           updatePixels message.Pixels

type GameExitMessage() = class end

type GameOverMessage() = class end

type GameVictoryMessage() = class end

type CameraZoomMessage(scale: float32) = 
    
    member _.Scale = scale

type CameraMoveMessage(position: Vector<pixel>) =

    member _.Position = position

type CameraMoveMessageHandler(camera: Camera) =

    interface IMessageHandler<CameraMoveMessage> with 
        member _.Handle(message: CameraMoveMessage) =
            camera.Position <- camera.Position + message.Position

type CameraZoomMessageHandler(camera: Camera) =

    interface IMessageHandler<CameraZoomMessage> with 
        member _.Handle(message: CameraZoomMessage) =
            camera.Zoom <- camera.Zoom + message.Scale
type GameDifficult =
    | Easy
    | Normal
    | Hard

type MapInfo = 
    { ScreenWidth:  int
      ScreenHeight: int
      Maze:         (int * int) list
      Waves:        int
      Lifes:        int
      Difficult:    GameDifficult }

type GamePlayScene (
                    camera: Camera,
                    input: IInputController,
                    entityProvider: IEntityProvider,
                    register: IMessageHandlerRegister, 
                    queue: IMessageQueue, 
                    draw: CameraMatrix option -> Shape -> unit, 
                    content: ContentManager, 
                    mapInfo: MapInfo,
                    playButtonScale: float32) =

    // Content
    let font = content.Load<SpriteFont>("Fonts\HUD")

    // Game Play
    let mutable gameSpeedCoefficient = 1.0f
    let mutable lifes = 0
    let mutable pixels = 130
    let mutable playButtons : GamePlaySceneHud.PlayButtonsInfo = { Button = GamePlaySceneHud.PlayButton.Play; Position = (Vector(25.0f<pixel>, 25.0f<pixel>)); Scale = playButtonScale }

    // Map
    let cellWidth = 50.0f<pixel>
    let cellHeight = 50.0f<pixel>
    let mutable columns = 0
    let mutable raws = 0
    let mutable grid: IEntity option [,] = Array2D.init columns raws (fun _ _ -> None)    
    let mutable spawner = Unchecked.defaultof<Spawner>

    // AI
    let mutable waveNumber = 0
    let mutable wave = []
    let mutable actionDelay = 0.0f<second>
    let mutable enemyPixelGrowthCoefficient = 0.0f
    let mutable turretRisePriceCoefficient = 0.0f
    let mutable regularTurretBasePrice = GameBalance.regularTurretPrice
    let mutable slowTurretBasePrice = GameBalance.slowTurretPrice
    let mutable splashTurretBasePrice = GameBalance.splashTurretPrice

    let createPath (grid: IEntity option [,]) =
        let rec findSpawner (grid: IEntity option [,]) x y raws columns =
            match grid.[x, y] with
            | Some t when t.GetType() = typeof<Spawner> -> Some (x, y)
            | _ when x = columns - 1 -> findSpawner grid 0 (y + 1) raws columns
            | _ when x = columns - 1 && y = raws - 1 -> None
            | _ -> findSpawner grid (x + 1) y raws columns

        let isPath (grid: IEntity option [,]) (x, y) =
            match grid.[x, y] with 
             | Some x when x.GetType() = typeof<Road> || x.GetType() = typeof<Receiver> -> true
             | _ -> false

        let rec findPath grid (x, y) (raws: int) (columns: int) path = 
            if 0 <= y - 1 && y - 1 < raws && List.contains (x, y - 1) path |> not && isPath grid (x, y - 1) then findPath grid (x, y - 1) raws columns ((x, y - 1)::path)
            else if 0 <= x + 1 && x + 1 < columns && List.contains (x + 1, y) path |> not && isPath grid (x + 1, y) then findPath grid (x + 1, y) raws columns ((x + 1, y)::path)
            else if 0 <= y + 1 && y + 1 < raws && List.contains (x, y + 1) path |> not && isPath grid (x, y + 1) then findPath grid (x, y + 1) raws columns ((x, y + 1)::path)
            else if 0 <= x - 1 && x - 1 < columns && List.contains (x - 1, y) path |> not && isPath grid (x - 1, y) then findPath grid (x - 1, y) raws columns ((x - 1, y)::path)
            else path

        match findSpawner grid 0 0 raws columns with 
        | Some (x, y) -> findPath grid (x, y) raws columns ((x, y)::[])
        | _ -> raise (System.Exception("Spawner not found!"))

    let center (column: int) (raw: int) = Vector.init ((float32 column) * cellWidth + cellWidth / 2.0f) ((float32 raw) * cellHeight + cellHeight / 2.0f)
    let createEntity (t: int) column raw (entityProvider: IEntityProvider) (factory: EnemyFactory) =
        match t with 
        | 0 -> Spawner (center column raw, factory) :> IEntity
        | 1 -> Road (Vector.init (float32 column * cellWidth) (float32 raw * cellHeight), cellWidth, cellHeight) :> IEntity
        | 2 -> Receiver (center column raw, entityProvider, mapInfo.Lifes) :> IEntity

    do

        let addPixels p =
            pixels <- pixels + p

        let subPixels p =
            pixels <- pixels - p

        let gameInteract (x: int) (y: int) =

            let pos = Vector.init ((float32 x) * 1.0f<pixel>) ((float32 y) * 1.0f<pixel>)
            if GamePlaySceneHud.isInPlayButtons playButtons pos
                then
                    playButtons <- GamePlaySceneHud.update pos playButtons
                else
                    let (Vector(x, y)) = (Vector.init (float32 x) (float32 y)) * camera.Inverse
                    let column = x / cellWidth |> int
                    let raw = y / cellHeight |> int

                    match grid.[column, raw] with
                    | None -> 
                        let picker = TurretPicker(Vector.init (float32 column * cellWidth) (float32 raw * cellHeight), cellWidth, cellHeight, raw) :> IEntity
                        grid.[column, raw] <- Some picker
                        entityProvider.RegisterEntity picker
                    | Some cell ->
                        match cell with 
                        | :? TurretPicker as picker -> 
                          
                            match picker.Click(Vector(x * 1.0f<pixel>, y * 1.0f<pixel>)) with
                            | Regular when pixels >= regularTurretBasePrice -> 
                                let turret = Turret.CreateRegular(center column raw, entityProvider) :> IEntity
                                grid.[column, raw] <- Some turret
                                entityProvider.RegisterEntity turret
                                entityProvider.RemoveEntity picker
                                pixels <- pixels - regularTurretBasePrice
                                regularTurretBasePrice <- regularTurretBasePrice |> float32 |> (*) turretRisePriceCoefficient |> int
                            | Slow when pixels >= slowTurretBasePrice -> 
                                let turret = Turret.CreateSlow(center column raw, entityProvider) :> IEntity
                                grid.[column, raw] <- Some turret
                                entityProvider.RegisterEntity turret
                                entityProvider.RemoveEntity picker
                                pixels <- pixels - slowTurretBasePrice
                                slowTurretBasePrice <- slowTurretBasePrice |> float32 |> (*) turretRisePriceCoefficient |> int
                            | Splash when pixels >= splashTurretBasePrice -> 
                                let turret = Turret.CreateSplash(center column raw, entityProvider) :> IEntity
                                grid.[column, raw] <- Some turret
                                entityProvider.RegisterEntity turret
                                entityProvider.RemoveEntity picker
                                pixels <- pixels - splashTurretBasePrice
                                splashTurretBasePrice <- splashTurretBasePrice |> float32 |> (*) turretRisePriceCoefficient |> int
                            | _ -> ()
                        | _ -> ()

        register.Register (TurretCreatedMessageHandler(subPixels))
        register.Register (EnemyCreatedMessageHandler(entityProvider))
        register.Register (EnemyKilledMessageHandler(entityProvider, addPixels))
        register.Register (GamePlayInteractionMessageHandler(gameInteract))
        register.Register (CameraZoomMessageHandler(camera))
        register.Register (CameraMoveMessageHandler(camera))

        let factory = EnemyFactory (fun message -> 
                                                    match message with 
                                                    | EnemyCreatedMessage m -> queue.Push m
                                                    | EnemyKilledMessage m -> queue.Push m)

        let maze = mapInfo.Maze

        let minX = maze |> List.filter (fun (x, _) -> x <= 0) |> List.map (fun (x, _) -> x) |> List.min |> (+) -1
        let minY = maze |> List.filter (fun (_, y) -> y <= 0) |> List.map (fun (_, y) -> y) |> List.min |> (+) -1

        let maze = maze |> List.map (fun (x, y) -> x - minX, y - minY)
        
        columns <- maze |> List.map (fun (x, _) -> x) |> List.max |> (+) 2
        raws    <- maze |> List.map (fun (_, y) -> y) |> List.max |> (+) 2
        grid <- Array2D.init columns raws (fun _ _ -> None)

        let ((x, y)::maze) = maze

        let entity = createEntity 0 x y entityProvider factory
        entityProvider.RegisterEntity entity
        grid.[x, y] <- Some entity
        spawner <- entity :?> Spawner

        let rec buildGrid maze = 
            match maze with 
            | (x, y)::[] -> 
                let entity = createEntity 2 x y entityProvider factory
                entityProvider.RegisterEntity entity
                grid.[x, y] <- Some entity
            | (x, y)::tail -> 
                let entity = createEntity 1 x y entityProvider factory
                entityProvider.RegisterEntity entity
                grid.[x, y] <- Some entity
                buildGrid tail

        buildGrid maze

        grid 
            |> createPath 
            |> List.rev 
            |> List.map (fun (x, y) -> Vector.init (float32 x * cellWidth + cellWidth / 2.0f) (float32 y * cellHeight + cellHeight / 2.0f))
            |> factory.UpdatePath

        match mapInfo.Difficult with 
        | Easy -> 
            enemyPixelGrowthCoefficient <- 1.85f
            turretRisePriceCoefficient  <- 1.0f
        | Normal ->
            enemyPixelGrowthCoefficient <- 2.0f
            turretRisePriceCoefficient  <- 1.05f
        | Hard ->
            enemyPixelGrowthCoefficient <- 2.2f
            turretRisePriceCoefficient  <- 1.1f

    interface IScene with 
        member _.Update (delta: float32<second>) =

            input.Update delta

            gameSpeedCoefficient <- match playButtons.Button with 
                                    | GamePlaySceneHud.Pause -> 0.0f
                                    | GamePlaySceneHud.Slow -> 0.1f
                                    | GamePlaySceneHud.Play -> 1.0f
                                    | GamePlaySceneHud.Fast -> 2.0f
                                            
            if waveNumber > mapInfo.Waves then queue.Push(GameVictoryMessage()) else ()

            match actionDelay with 
            | _ when actionDelay <= 0.0f<second> -> 
                match wave with 
                | [] when entityProvider.GetEntities() |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>) |> Seq.length = 0 ->

                    waveNumber <- waveNumber + 1

                    let k = (waveNumber |> float32) ** enemyPixelGrowthCoefficient |> int |> (*) 1<pixel>
                    let pixels = if k < 15<pixel> then 15<pixel> else k
                    wave <- Wave.create (Random.random) pixels 1 waveNumber
                        
                | head::tail ->
                    match head with 
                    | Create enemyType -> 
                        spawner.Spawn (enemyType, waveNumber)
                    | Delay time ->
                        actionDelay <- time

                    wave <- tail
                | _ -> ()
            | _ -> 
                actionDelay <- actionDelay - delta * gameSpeedCoefficient

            entityProvider.Update (delta * gameSpeedCoefficient)

            for x in [0..(columns) - 1] do 
                for y in [0..(raws) - 1] do 
                    match grid.[x, y] with
                    | None -> ()
                    | Some cell ->
                        match cell with 
                        | :? Receiver as receiver when receiver.Life > 0 -> lifes <- receiver.Life
                        | :? Receiver as receiver when receiver.Life <= 0 -> queue.Push(GameOverMessage())
                        | _ -> ()

        member _.Draw (_: float32<second>) = 

            entityProvider.GetEntities() 
            |> Seq.map (fun x -> x.Draw())
            |> Seq.rev
            |> Seq.fold (fun acc x -> x::acc) []
            |> (fun list -> Shape(list)) 
            |> draw (Some camera.Matrix)

            for x in [0 .. columns - 1] do
                for y in [0 .. raws - 1] do
                    Rectangle(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight, false, Color.black ) |> draw (Some camera.Matrix)

            [ 
                GamePlaySceneHud.drawPlayButtons playButtons; 
                GamePlaySceneHud.drawStatusLable mapInfo.ScreenWidth font pixels lifes waveNumber regularTurretBasePrice slowTurretBasePrice splashTurretBasePrice
            ] |> Shape |> draw None