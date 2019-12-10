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
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch> 

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

    override this.Initialize () =
        base.Initialize()

        [
            Enemy (100, spriteBatch, Vector2(400.0f, 0.0f), entityProvider) :> IEntity; 
            Turret (spriteBatch, Vector2(300.0f, 300.0f), entityProvider) :> IEntity
        ] |> List.iter entityProvider.RegisterEntity


    override this.Update (gameTime : GameTime) =
        entityProvider.GetEntities() |> List.iter (fun x -> x.Update())

        base.Update(gameTime)

    override this.Draw (gameTime : GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()

        entityProvider.GetEntities() |> List.iter (fun x -> x.Draw())

        spriteBatch.End()

[<STAThread>]
[<EntryPoint>]
let main argv =
    let game = new TheGame()
    game.Run()

    0
