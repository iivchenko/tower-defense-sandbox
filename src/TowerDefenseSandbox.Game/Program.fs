open Microsoft.Xna.Framework
open System
open TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open System.IO

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable texture = Unchecked.defaultof<Texture2D>
    let mutable enemy = Unchecked.defaultof<Enemy>

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        let fileStream = new FileStream("Content/Enemy.png", FileMode.Open)
        texture <- Texture2D.FromStream(graphics.GraphicsDevice, fileStream);

        enemy <- Enemy (100, spriteBatch, texture, Vector2(200.0f, 200.0f))

    override this.Update (gameTime : GameTime) =
        enemy.Update()

    override this.Draw (gameTime : GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()

        enemy.Draw()

        spriteBatch.End()

[<STAThread>]
[<EntryPoint>]
let main argv =
    let game = new TheGame()
    game.Run()

    0 // return an integer exit code
