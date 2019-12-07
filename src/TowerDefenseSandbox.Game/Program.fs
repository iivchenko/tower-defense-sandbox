open Microsoft.Xna.Framework
open System
open TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open System.IO
open System.Collections.Generic

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch> 
    let mutable enemy = Unchecked.defaultof<Enemy>
    let mutable turret = Unchecked.defaultof<Turret>

    let createTexture path =
        use stream = new FileStream(path, FileMode.Open)
        Texture2D.FromStream(graphics.GraphicsDevice, stream);

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)        

    override this.Initialize () =
        base.Initialize()

        enemy <- Enemy (100, spriteBatch, createTexture "Content/Enemy.png", Vector2(200.0f, 0.0f))
        let enemies = new List<Enemy>()
        enemies.Add(enemy);
        turret <- Turret (spriteBatch, createTexture "Content/Turret.png", Vector2(300.0f, 300.0f), enemies)

    override this.Update (gameTime : GameTime) =
        enemy.Update()
        turret.Update()

    override this.Draw (gameTime : GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()

        enemy.Draw()
        turret.Draw()

        spriteBatch.End()

[<STAThread>]
[<EntryPoint>]
let main argv =
    let game = new TheGame()
    game.Run()

    0 // return an integer exit code
