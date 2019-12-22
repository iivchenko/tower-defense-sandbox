open Microsoft.Xna.Framework
open System
open TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open TowerDefenseSandbox.Game.Engine
open Microsoft.FSharp.Collections
open MonoGame.Extended

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let entityProvider = EntityProvider() :> IEntityProvider
    let mutable grid = Unchecked.defaultof<Grid> 
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch> 

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

    override this.Initialize () =
        
        base.Initialize()
        grid <- Grid (spriteBatch, 10, 10, 75.0f, 75.0f)
        grid.[1, 0] <- Spawner (1, spriteBatch, entityProvider) :> ICell |> Some
        grid.[1, 1] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[1, 2] <- Road (spriteBatch, 0) :> ICell |> Some
        grid.[1, 3] <- Road (spriteBatch, 0) :> ICell |> Some
        
        grid.[0, 1] <- Turret (1, spriteBatch, entityProvider) :> ICell |> Some
        grid.[2, 2] <- Turret (1, spriteBatch, entityProvider) :> ICell |> Some
        grid.[0, 3] <- Turret (1, spriteBatch, entityProvider) :> ICell |> Some

        grid.[1, 4] <- Receiver (spriteBatch, entityProvider, 1) :> ICell |> Some

    override this.Update (gameTime : GameTime) =
        entityProvider.GetEntities() |> List.iter (fun x -> x.Update(gameTime))
        grid.Update(gameTime)

        base.Update(gameTime)

    override this.Draw (gameTime : GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()

        grid.Draw(gameTime)
        entityProvider.GetEntities() |> List.iter (fun x -> x.Draw(gameTime))

        spriteBatch.End()

[<STAThread>]
[<EntryPoint>]
let main argv =
    let game = new TheGame()
    game.Run()

    0
