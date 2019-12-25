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

    let mutable grid = Unchecked.defaultof<Grid> 
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch> 
    let mutable previousButtonState = ButtonState.Released

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

    override _.Initialize () =
        
        base.Initialize()
       
        let raws = screenWith / int cellWidth
        let columns = screenHeight / int cellHeight

        graphics.PreferredBackBufferWidth <- screenWith
        graphics.PreferredBackBufferHeight <- screenHeight
        graphics.IsFullScreen <- true;
        graphics.ApplyChanges();

        base.IsMouseVisible <- true

        grid <- Grid (spriteBatch, raws, columns, cellWidth, cellHeight)

        let path = [Vector2 (1.0f * cellWidth + cellWidth/2.0f, 4.0f * cellHeight + cellHeight/2.0f); Vector2(5.0f * cellWidth + cellWidth/2.0f, 4.0f * cellHeight + cellHeight/2.0f)]
        grid.[1, 0] <- Spawner (1, spriteBatch, entityProvider, path) :> ICell |> Some
        grid.[1, 1] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[1, 2] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[1, 3] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[1, 4] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[2, 4] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[3, 4] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[4, 4] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[5, 4] <- Receiver (spriteBatch, entityProvider, 1) :> ICell |> Some

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