namespace TowerDefenseSandbox.Game.Screens

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open MonoGame.Extended

open Myra.Graphics2D.UI

open System.IO
open Newtonsoft.Json

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine
open TowerDefenseSandbox.Game.Entities

type GamePlayScreen (manager: IScreenManager, draw: Shape -> unit, content: ContentManager, screenWith: int, screenHeight: int) =

    let h3 = content.Load<SpriteFont>("Fonts\H3")
    let entityProvider = EntityProvider() :> IEntityProvider
    let cellWidth = 48.0f
    let cellHeight = 45.0f
    let columns = screenWith / int cellWidth
    let raws = screenHeight / int cellHeight

    let lifes = new Label()

    let mutable isEscUpPrev = true
    let mutable grid = Unchecked.defaultof<Grid> 
    let mutable previousButtonState = ButtonState.Released

    let createPath (grid: Grid) =
        let rec findSpawner (grid: Grid) x y raws columns =
            match grid.[x, y] with
            | Some t when t.GetType() = typeof<Spawner> -> Some (x, y)
            | _ when x = columns - 1 -> findSpawner grid 0 (y + 1) raws columns
            | _ when x = columns - 1 && y = raws - 1 -> None
            | _ -> findSpawner grid (x + 1) y raws columns

        let isPath (grid: Grid) (x, y) =
            match grid.[x, y] with 
             | Some x when x.GetType() = typeof<Road> || x.GetType() = typeof<Receiver> -> true
             | _ -> false

        let rec findPath (grid: Grid) (x, y) (raws: int) (columns: int) path = 
            if 0 <= y - 1 && y - 1 < raws && List.contains (x, y - 1) path |> not && isPath grid (x, y - 1) then findPath grid (x, y - 1) raws columns ((x, y - 1)::path)
            else if 0 <= x + 1 && x + 1 < columns && List.contains (x + 1, y) path |> not && isPath grid (x + 1, y) then findPath grid (x + 1, y) raws columns ((x + 1, y)::path)
            else if 0 <= y + 1 && y + 1 < raws && List.contains (x, y + 1) path |> not && isPath grid (x, y + 1) then findPath grid (x, y + 1) raws columns ((x, y + 1)::path)
            else if 0 <= x - 1 && x - 1 < columns && List.contains (x - 1, y) path |> not && isPath grid (x - 1, y) then findPath grid (x - 1, y) raws columns ((x - 1, y)::path)
            else path

        match findSpawner grid 0 0 raws columns with 
        | Some (x, y) -> findPath grid (x, y) raws columns ((x, y)::[])
        | _ -> raise (System.Exception("Spawner not found!"))

    let createEntity (t: int) (draw: Shape -> unit) (entityProvider: IEntityProvider) (factory: EnemyFactory) =
        match t with 
        | 0 -> Spawner (1, draw, factory) :> ICell |> Some
        | 1 -> Road (draw, 0) :> ICell |> Some
        | 2 -> Receiver (draw, entityProvider, 1) :> ICell |> Some

    do
        grid <- Grid (draw, columns, raws, cellWidth, cellHeight)

        let factory = EnemyFactory (draw, entityProvider)
        let data = JsonConvert.DeserializeObject<(int*int*int) list>(File.ReadAllText("level.json"));

        data 
            |> List.iter (fun (x, y, t) ->  grid.[x, y] <- createEntity t draw entityProvider factory)

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
        member _.Update (time: float32<second>) =

            if not isEscUpPrev && Keyboard.GetState().IsKeyUp(Keys.Escape) then manager.ToMainMenu() else ()

            isEscUpPrev <- Keyboard.GetState().IsKeyUp(Keys.Escape)

            grid.Update(time)

            let state = Mouse.GetState ()

            if previousButtonState = ButtonState.Pressed && state.LeftButton = ButtonState.Released then
                let x = state.X / int cellWidth
                let y = state.Y / int cellHeight

                match grid.[x, y] with
                | None -> grid.[x, y] <- TurretPicker(1, draw, grid, entityProvider, x, y) :> ICell |> Some
                | Some cell ->
                    match cell with 
                    | :? TurretPicker as picker -> picker.Click(RectangleF(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight), Vector(float32 state.X, float32 state.Y))
                    | _ -> ()
            else 
                ()

            previousButtonState <- state.LeftButton

            entityProvider.GetEntities() |> Seq.iter (fun x -> x.Update(time))

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

        member _.Draw (time: float32<second>) = 

            grid.Draw(time)
            entityProvider.GetEntities() |> Seq.iter (fun x -> x.Draw(time))