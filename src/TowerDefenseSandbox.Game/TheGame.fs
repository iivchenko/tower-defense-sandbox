open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Myra

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Engine.MonoGame
open TowerDefenseSandbox.Game.Engine
open TowerDefenseSandbox.Game.Screens
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let screenWith = 1920 
    let screenHeight = 1080

    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable draw: Shape -> unit = (fun _ -> ())

    let screenManager = ScreenManager ()

    override _.LoadContent() =
        this.Content.RootDirectory <- "Content"
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

        let createMainScreen () = MainMenuScreen(screenManager, this.Content, this.Exit) :> IScreen
        let createGamePlayScreen () = GamePlayScreen(screenManager, draw, screenWith, screenWith) :> IScreen
        let createGameEditScreen () = GameEditorScreen(screenManager, draw, screenWith, screenHeight) :> IScreen
        let createGameSettingsScreen () = EmptyScreen() :> IScreen
        let createGameOverScreen () = GameOverScreen(screenManager, this.Content) :> IScreen

        screenManager.SetMainMenu createMainScreen
        screenManager.SetGamePlay createGamePlayScreen
        screenManager.SetGameEdit createGameEditScreen
        screenManager.SetGameSettings createGameSettingsScreen
        screenManager.SetGameOver createGameOverScreen

        (screenManager :> IScreenManager).ToMainMenu ()

    override _.Update (gameTime: GameTime) =

        screenManager.Screen.Update (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        base.Update(gameTime)

    override _.Draw (gameTime: GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()
        
        screenManager.Screen.Draw (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        spriteBatch.End()

[<EntryPoint>]
let main _ =
    let game = new TheGame()
    game.Run()

    0