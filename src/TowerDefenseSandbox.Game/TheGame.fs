open Microsoft.Xna.Framework
open System
open TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open TowerDefenseSandbox.Game.Engine
open Microsoft.FSharp.Collections
open Microsoft.Xna.Framework.Input

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let entityProvider = EntityProvider() :> IEntityProvider
    let screenWith = 1920 
    let screenHeight = 1080
    let cellWidth = 48.0f
    let cellHeight = 45.0f
    let columns = screenWith / int cellWidth
    let raws = screenHeight / int cellHeight

    let mutable grid = Unchecked.defaultof<Grid> 
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch> 
    let mutable previousButtonState = ButtonState.Released

    let createPath (grid : Grid) =
        let rec findSpawner (grid : Grid) x y raws columns =
            match grid.[x, y] with
            | Some t when t.GetType() = typeof<Spawner> -> Some (x, y)
            | _ when x = columns - 1 -> findSpawner grid 0 (y + 1) raws columns
            | _ when x = columns - 1 && y = raws - 1 -> None
            | _ -> findSpawner grid (x + 1) y raws columns

        let isPath (grid : Grid) (x, y) =
            match grid.[x, y] with 
             | Some x when x.GetType() = typeof<Road> || x.GetType() = typeof<Receiver> -> true
             | _ -> false

        let rec findPath (grid : Grid) (x, y) (raws : int) (columns : int) path = 
            if 0 <= y - 1 && y - 1 < raws && List.contains (x, y - 1) path |> not && isPath grid (x, y - 1) then findPath grid (x, y - 1) raws columns ((x, y - 1)::path)
            else if 0 <= x + 1 && x + 1 < columns && List.contains (x + 1, y) path |> not && isPath grid (x + 1, y) then findPath grid (x + 1, y) raws columns ((x + 1, y)::path)
            else if 0 <= y + 1 && y + 1 < raws && List.contains (x, y + 1) path |> not && isPath grid (x, y + 1) then findPath grid (x, y + 1) raws columns ((x, y + 1)::path)
            else if 0 <= x - 1 && x - 1 < columns && List.contains (x - 1, y) path |> not && isPath grid (x - 1, y) then findPath grid (x - 1, y) raws columns ((x - 1, y)::path)
            else path

        match findSpawner grid 0 0 raws columns with 
        | Some (x, y) -> findPath grid (x, y) raws columns ((x, y)::[])
        | _ -> raise (System.Exception("Spawner not found!"))

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

    override _.Initialize () =
        
        base.Initialize()

        graphics.PreferredBackBufferWidth <- screenWith
        graphics.PreferredBackBufferHeight <- screenHeight
        
        #if RELEASE 
        graphics.IsFullScreen <- true;
        #endif

        graphics.ApplyChanges();

        base.IsMouseVisible <- true

        grid <- Grid (spriteBatch, columns, raws, cellWidth, cellHeight)

        let factory = EnemyFactory (spriteBatch, entityProvider)

        grid.[1, 0] <- Spawner (1, spriteBatch, factory) :> ICell |> Some
        grid.[1, 1] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[1, 2] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[1, 3] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[1, 4] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[2, 4] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[3, 4] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[4, 4] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[5, 4] <- Receiver (spriteBatch, entityProvider, 1) :> ICell |> Some

        createPath grid |> List.rev |> List.map (fun (x, y) -> Vector2 (float32 x * cellWidth + cellWidth / 2.0f, float32 y * cellHeight + cellHeight / 2.0f)) |> factory.UpdatePath

    override _.Update (gameTime : GameTime) =

        grid.Update(gameTime)

        let state = Mouse.GetState()

        if Keyboard.GetState().IsKeyDown(Keys.Escape) then this.Exit() else ()

        if previousButtonState = ButtonState.Pressed && state.LeftButton = ButtonState.Released then
            let x = state.X / int cellWidth
            let y = state.Y / int cellHeight

            grid.[x, y] <- Turret (1, spriteBatch, entityProvider) :> ICell |> Some
        else 
            ()

        previousButtonState <- state.LeftButton

        entityProvider.GetEntities() |> Seq.iter (fun x -> x.Update(gameTime))

        entityProvider.Flush ()

        base.Update(gameTime)

    override _.Draw (gameTime : GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()

        grid.Draw(gameTime)
        entityProvider.GetEntities() |> Seq.iter (fun x -> x.Draw(gameTime))

        spriteBatch.End()
[<STAThread>]
[<EntryPoint>]
let main argv =
    let game = new TheGame()
    game.Run()

    0