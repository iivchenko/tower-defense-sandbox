open Microsoft.Xna.Framework
open System
open TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open TowerDefenseSandbox.Game.Engine
open Microsoft.FSharp.Collections

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let entityProvider = EntityProvider() :> IEntityProvider
    let grid = Grid (10, 10, 75.0f, 75.0f)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch> 

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

    override this.Initialize () =
        
        base.Initialize()

        grid.[4, 1] <- Spawner (spriteBatch, entityProvider) :> ICell |> Some
        grid.[3, 1] <- Spawner (spriteBatch, entityProvider) :> ICell |> Some
        grid.[3, 2] <- Spawner (spriteBatch, entityProvider) :> ICell |> Some
        
        grid.[2, 2] <- Turret (spriteBatch, entityProvider) :> ICell |> Some
        grid.[4, 2] <- Turret (spriteBatch, entityProvider) :> ICell |> Some
        grid.[4, 4] <- Turret (spriteBatch, entityProvider) :> ICell |> Some

    override this.Update (gameTime : GameTime) =
        entityProvider.GetEntities() |> List.iter (fun x -> x.Update(gameTime))
        grid.Update(gameTime)

        base.Update(gameTime)

    override this.Draw (gameTime : GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()

        entityProvider.GetEntities() |> List.iter (fun x -> x.Draw(gameTime))
        grid.Draw(gameTime)

        spriteBatch.End()

[<STAThread>]
[<EntryPoint>]
let main argv =
    let game = new TheGame()
    game.Run()

    0
