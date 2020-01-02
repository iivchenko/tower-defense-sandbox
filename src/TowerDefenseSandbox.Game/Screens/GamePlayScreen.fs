namespace TowerDefenseSandbox.Game.Screens

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open TowerDefenseSandbox.Game.Entities
open System.IO
open Newtonsoft.Json
open MonoGame.Extended

type GamePlayScreen (spriteBatch: SpriteBatch, screenWith: int, screenHeight: int) =

    let entityProvider = EntityProvider() :> IEntityProvider
    let cellWidth = 48.0f
    let cellHeight = 45.0f
    let columns = screenWith / int cellWidth
    let raws = screenHeight / int cellHeight

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

    let createEntity (t: int) (spriteBatch: SpriteBatch) (entityProvider: IEntityProvider) (factory: EnemyFactory) =
        match t with 
        | 0 -> Spawner (1, spriteBatch, factory) :> ICell |> Some
        | 1 -> Road (spriteBatch, 0) :> ICell |> Some
        | 2 -> Receiver (spriteBatch, entityProvider, 1) :> ICell |> Some

    do
        grid <- Grid (spriteBatch, columns, raws, cellWidth, cellHeight)

        let factory = EnemyFactory (spriteBatch, entityProvider)
        let data = JsonConvert.DeserializeObject<(int*int*int) list>(File.ReadAllText("level.json"));

        data 
            |> List.iter (fun (x, y, t) ->  grid.[x, y] <- createEntity t spriteBatch entityProvider factory)

        grid 
            |> createPath 
            |> List.rev 
            |> List.map (fun (x, y) -> Vector.init (float32 x * cellWidth + cellWidth / 2.0f) (float32 y * cellHeight + cellHeight / 2.0f)) 
            |> factory.UpdatePath
       
    interface IScreen with 
        member _.Update (gameTime: GameTime) =

            grid.Update(gameTime)

            let state = Mouse.GetState ()

            if previousButtonState = ButtonState.Pressed && state.LeftButton = ButtonState.Released then
                let x = state.X / int cellWidth
                let y = state.Y / int cellHeight

                match grid.[x, y] with
                | None -> grid.[x, y] <- TurretPicker(1, spriteBatch, grid, entityProvider, x, y) :> ICell |> Some
                | Some cell ->
                    match cell with 
                    | :? TurretPicker as picker -> picker.Click(RectangleF(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight), Vector(float32 state.X, float32 state.Y))
                    | _ -> ()
            else 
                ()

            previousButtonState <- state.LeftButton

            entityProvider.GetEntities() |> Seq.iter (fun x -> x.Update(gameTime))

            entityProvider.Flush ()

        member _.Draw (gameTime: GameTime) =

            grid.Draw(gameTime)
            entityProvider.GetEntities() |> Seq.iter (fun x -> x.Draw(gameTime))