﻿namespace TowerDefenseSandbox.Game.Scenes

open System.IO
open Newtonsoft.Json
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open Fame
open Fame.Input
open Fame.Messaging
open Fame.Scene
open Fame.Graphics
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

module HUD = 

    type PlayButton = | Pause | Slow | Play | Fast

    type PlayButtonsInfo = 
        { Position: Vector<pixel>
          Button: PlayButton
          Scale: float32 }

    let drawPlayButtons state  = 

        let scale = state.Scale
        let transform = Matrix.scale scale
        let (Vector(x, y)) = state.Position
        let foo = state.Button
        let pause = Shape
                        ([  
                            Rectangle(x + 00.0f<pixel> * scale, y + 00.0f<pixel> * scale, 10.0f<pixel> * scale, 30.0f<pixel> * scale, false, if foo = Pause then Color.red else Color.white); 
                            Rectangle(x + 15.0f<pixel> * scale, y + 00.0f<pixel> * scale, 10.0f<pixel> * scale, 30.0f<pixel> * scale, false, if foo = Pause then Color.red else Color.white) 
                        ])

        let x = x + 35.0f<pixel> * scale
        let slow = Triangle(x, y, Vector(20.0f<pixel>, 00.0f<pixel>) * transform, Vector(20.0f<pixel>, 30.0f<pixel>) * transform, Vector(00.0f<pixel>, 15.0f<pixel>) * transform, if foo = Slow then Color.red else Color.white)

        let x = x + 35.0f<pixel> * scale
        let play = Triangle(x, y, Vector(00.0f<pixel>, 00.0f<pixel>) * transform, Vector(00.0f<pixel>, 30.0f<pixel>) * transform, Vector(20.0f<pixel>, 15.0f<pixel>) * transform, if foo = Play then Color.red else Color.white)
        
        let x = x + 35.0f<pixel> * scale
        let fast1 = Triangle(x, y, Vector(00.0f<pixel>, 00.0f<pixel>) * transform, Vector(00.0f<pixel>, 30.0f<pixel>) * transform, Vector(20.0f<pixel>, 15.0f<pixel>) * transform, if foo = Fast then Color.red else Color.white)
        let x = x + 15.0f<pixel> * scale
        let fast2 = Triangle(x, y, Vector(00.0f<pixel>, 00.0f<pixel>) * transform, Vector(00.0f<pixel>, 30.0f<pixel>) * transform, Vector(20.0f<pixel>, 15.0f<pixel>) * transform, if foo = Fast then Color.red else Color.white)
        let fast = Shape([ fast1; fast2 ])

        Shape([ pause; slow; play; fast ])

    let isInPlayButtons state (Vector(x, y)) =
        let scale = state.Scale
        let (Vector(px, py)) = state.Position
        let inBound x y (x', y', width, height) = x >= x' && x <= x' + width && y >= y' && y <= y' + height

        inBound x y (px, py, 140.0f<pixel> * scale, 30.0f<pixel> * scale)

    let update (Vector(x, y)) state =
        let scale = state.Scale
        let (Vector(xp, yp)) = state.Position
        let inBound x y (x', y', width, height) = x >= x' && x <= x' + width && y >= y' && y <= y' + height

        { 
            state with 
                Button = if inBound x y (xp + 0.0f<pixel>, yp + 0.0f<pixel>, 35.0f<pixel> * scale, 30.0f<pixel> * scale)
                            then Pause 
                            else if inBound x y (xp + 35.0f<pixel> * scale, yp + 00.0f<pixel> * scale, 35.0f<pixel> * scale, 30.0f<pixel> * scale) 
                                then Slow
                                else if inBound x y (xp + 70.0f<pixel> * scale, yp + 00.0f<pixel> * scale, 35.0f<pixel> * scale, 30.0f<pixel> * scale) 
                                    then Play
                                    else Fast
        }

type GamePlayScene (
                    camera: Camera,
                    input: IInputController,
                    entityProvider: IEntityProvider,
                    register: IMessageHandlerRegister, 
                    queue: IMessageQueue, 
                    draw: CameraMatrix option -> Shape -> unit, 
                    content: ContentManager, 
                    screenWith: int, 
                    screenHeight: int, 
                    playButtonScale: float32) =

    let font = content.Load<SpriteFont>("Fonts\HUD")
    let cellWidth = 50.0f<pixel>
    let cellHeight = 50.0f<pixel>
    let columns = screenWith / int cellWidth
    let raws = screenHeight / int cellHeight
    
    let mutable gameSpeedCoefficient = 1.0f
    let mutable lifes = ""
    let mutable pixelsLabel = ""
    let mutable pixels = 100
    let mutable playButtons : HUD.PlayButtonsInfo = { Button = HUD.PlayButton.Play; Position = (Vector(25.0f<pixel>, 25.0f<pixel>)); Scale = playButtonScale }

    let grid: IEntity option [,] = Array2D.init columns raws (fun _ _ -> None)

    let pushTurretMessage (message: TurretCreatedMessage) = queue.Push message

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
        | 0 -> Spawner (center column raw, factory, (fun m -> queue.Push m), entityProvider) :> IEntity
        | 1 -> Road (Vector.init (float32 column * cellWidth) (float32 raw * cellHeight), cellWidth, cellHeight) :> IEntity
        | 2 -> Receiver (center column raw, entityProvider) :> IEntity

    do

        let addPixels p =
            pixels <- pixels + p
            pixelsLabel <- sprintf "Pixels: %i" pixels

        let subPixels p =
            pixels <- pixels - p
            pixelsLabel <- sprintf "Pixels: %i" pixels

        let gameInteract (x: int) (y: int) =

            let pos = Vector.init ((float32 x) * 1.0f<pixel>) ((float32 y) * 1.0f<pixel>)
            if HUD.isInPlayButtons playButtons pos
                then
                    playButtons <- HUD.update pos playButtons
                else
                    let (Vector(x, y)) = (Vector.init (float32 x) (float32 y)) * camera.Inverse
                    let column = x / cellWidth |> int
                    let raw = y / cellHeight |> int

                    match grid.[column, raw] with
                    | None -> 
                        let picker = TurretPicker(Vector.init (float32 column * cellWidth) (float32 raw * cellHeight), cellWidth, cellHeight, grid, pushTurretMessage, entityProvider, column, raw) :> IEntity
                        grid.[column, raw] <- Some picker
                        entityProvider.RegisterEntity picker
                    | Some cell ->
                        match cell with 
                        | :? TurretPicker as picker -> picker.Click(Vector(x * 1.0f<pixel>, y * 1.0f<pixel>), pixels)
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

        use file = TowerDefenseSandbox.Engine.MonoGame.File.read "level.json"
        use reader = new StreamReader(file)
        let data = JsonConvert.DeserializeObject<(int*int*int) list>(reader.ReadToEnd());

        data 
            |> List.iter (fun (x, y, t) -> 
                            let entity = createEntity t x y entityProvider factory
                            entityProvider.RegisterEntity entity
                            grid.[x, y] <- Some entity)

        grid 
            |> createPath 
            |> List.rev 
            |> List.map (fun (x, y) -> Vector.init (float32 x * cellWidth + cellWidth / 2.0f) (float32 y * cellHeight + cellHeight / 2.0f))
            |> factory.UpdatePath
        
        lifes <- "Life: 0"
        pixelsLabel <- sprintf "Pixels: %i" pixels       

    interface IScene with 
        member _.Update (delta: float32<second>) =

            input.Update delta

            gameSpeedCoefficient <- match playButtons.Button with 
                                    | HUD.Pause -> 0.0f
                                    | HUD.Slow -> 0.1f
                                    | HUD.Play -> 1.0f
                                    | HUD.Fast -> 2.0f

            entityProvider.Update (delta * gameSpeedCoefficient)

            entityProvider.Flush ()

            for x in [0..(columns) - 1] do 
                for y in [0..(raws) - 1] do 
                    match grid.[x, y] with
                    | None -> ()
                    | Some cell ->
                        match cell with 
                        | :? Receiver as receiver when receiver.Life > 0 -> lifes <- sprintf "Life: %i" receiver.Life
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

            let statusLable = sprintf "%s %s" lifes pixelsLabel
            let size = font.MeasureString(statusLable);

            draw None (HUD.drawPlayButtons playButtons)
            // TODO: F# refactoring make int, float, pixle friends
            draw None (Text((float32 screenWith) * 1.0f<pixel> - size.X * 1.0f<pixel> - 20.0f<pixel>, 20.0f<pixel>, statusLable, font, Color.white))