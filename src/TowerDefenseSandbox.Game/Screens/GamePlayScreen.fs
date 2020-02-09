namespace TowerDefenseSandbox.Game.Screens

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Myra.Graphics2D.UI

open System.IO
open Newtonsoft.Json

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine
open TowerDefenseSandbox.Game.Entities

type EnemyKilledMessageHandler (entityProvier: IEntityProvider) =
    
    interface IMessageHandler<EnemyKilledMessage> with

        member _.Handle (message: EnemyKilledMessage) =
            entityProvier.RemoveEntity message.Enemy

type GamePlayScreen (manager: IScreenManager, register: IMessageHandlerRegister, queue: IMessageQueue, draw: Shape -> unit, content: ContentManager, screenWith: int, screenHeight: int) =

    let h3 = content.Load<SpriteFont>("Fonts\H3")
    let entityProvider = EntityProvider() :> IEntityProvider
    let cellWidth = 48.0f
    let cellHeight = 45.0f
    let columns = screenWith / int cellWidth
    let raws = screenHeight / int cellHeight

    let lifes = new Label()

    let mutable isEscUpPrev = true
    let grid: IEntity option [,] = Array2D.init columns raws (fun _ _ -> None)
    let mutable previousButtonState = ButtonState.Released

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
    let createEntity (t: int) (draw: Shape -> unit) column raw (entityProvider: IEntityProvider) (factory: EnemyFactory) =
        match t with 
        | 0 -> Spawner (center column raw, draw, factory) :> IEntity
        | 1 -> Road (Vector.init (float32 column * cellWidth) (float32 raw * cellHeight), cellWidth, cellHeight, draw) :> IEntity
        | 2 -> Receiver (center column raw, draw, entityProvider) :> IEntity

    do
        register.Register (EnemyKilledMessageHandler(entityProvider) :> IMessageHandler<EnemyKilledMessage>)

        let factory = EnemyFactory (draw, entityProvider, queue)
        let data = JsonConvert.DeserializeObject<(int*int*int) list>(File.ReadAllText("level.json"));

        data 
            |> List.iter (fun (x, y, t) -> 
                            let entity = createEntity t draw x y entityProvider factory
                            entityProvider.RegisterEntity entity
                            grid.[x, y] <- Some entity)

        grid 
            |> createPath 
            |> List.rev 
            |> List.map (fun (x, y) -> Vector.init (float32 x * cellWidth + cellWidth / 2.0f) (float32 y * cellHeight + cellHeight / 2.0f))
            |> factory.UpdatePath
    
        Desktop.Widgets.Clear()

        let panel = new HorizontalStackPanel()
        panel.HorizontalAlignment <- HorizontalAlignment.Right
        panel.VerticalAlignment <- VerticalAlignment.Top
        
        lifes.Text <- "Life: 0"
        lifes.Font <- h3
        lifes.HorizontalAlignment <- HorizontalAlignment.Right
        lifes.VerticalAlignment <- VerticalAlignment.Center 
        lifes.PaddingRight <- 20
        lifes.PaddingTop <- 20

        panel.Widgets.Add(lifes)

        Desktop.Widgets.Add(panel)
       
    interface IScreen with 
        member _.Update (delta: float32<second>) =

            if not isEscUpPrev && Keyboard.GetState().IsKeyUp(Keys.Escape) then manager.ToMainMenu() else ()

            isEscUpPrev <- Keyboard.GetState().IsKeyUp(Keys.Escape)

            let state = Mouse.GetState ()

            if previousButtonState = ButtonState.Pressed && state.LeftButton = ButtonState.Released then
                let column = state.X / int cellWidth
                let raw = state.Y / int cellHeight

                match grid.[column, raw] with
                | None -> 
                    let picker = TurretPicker(Vector.init (float32 column * cellWidth) (float32 raw * cellHeight), cellWidth, cellHeight, draw, grid, entityProvider, column, raw) :> IEntity
                    grid.[column, raw] <- Some picker
                    entityProvider.RegisterEntity picker
                | Some cell ->
                    match cell with 
                    | :? TurretPicker as picker -> picker.Click(Vector(float32 state.X, float32 state.Y))
                    | _ -> ()
            else 
                ()

            previousButtonState <- state.LeftButton

            entityProvider.Update delta

            entityProvider.Flush ()

            for x in [0..(columns) - 1] do 
                for y in [0..(raws) - 1] do 
                    match grid.[x, y] with
                    | None -> ()
                    | Some cell ->
                        match cell with 
                        | :? Receiver as receiver when receiver.Life > 0 -> lifes.Text <- sprintf "Life: %i" receiver.Life
                        | :? Receiver as receiver when receiver.Life <= 0 -> manager.ToGameOver()
                        | _ -> ()

        member _.Draw (delta: float32<second>) = 

            entityProvider.Draw delta

            for x in [0 .. columns - 1] do
                for y in [0 .. raws - 1] do
                    Rectangle(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight, false, Color.black ) |> draw