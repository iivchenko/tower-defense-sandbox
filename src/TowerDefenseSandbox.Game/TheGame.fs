open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Myra

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Engine.MonoGame
open TowerDefenseSandbox.Game.Screens

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let screenWith = 1920 
    let screenHeight = 1080

    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable draw: Shape -> unit = (fun _ -> ()) 

    let mutable screen: IScreen = Unchecked.defaultof<IScreen>

    override _.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)
        draw <- Graphic.draw (MonoGameGraphic(spriteBatch))

    override this.Initialize () =

        base.Initialize()

        graphics.PreferredBackBufferWidth <- screenWith
        graphics.PreferredBackBufferHeight <- screenHeight
        
        #if RELEASE 
        graphics.IsFullScreen <- true
        #endif

        graphics.ApplyChanges();

        base.IsMouseVisible <- true

        MyraEnvironment.Game <- this

        screen <- MainMenuScreen (
            (fun _ -> screen <- GamePlayScreen(draw, screenWith, screenHeight)), 
            (fun _ -> screen <- GameEditorScreen(draw, screenWith, screenHeight)))

    override _.Update (gameTime: GameTime) =

        if Keyboard.GetState().IsKeyDown(Keys.Escape) then this.Exit() else ()

        screen.Update gameTime

        base.Update(gameTime)

    override _.Draw (gameTime: GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()

        screen.Draw gameTime

        spriteBatch.End()

[<EntryPoint>]
let main _ =
    let game = new TheGame()
    game.Run()

    0