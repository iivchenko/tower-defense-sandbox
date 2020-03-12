open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Myra

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Engine.MonoGame
open TowerDefenseSandbox.Game.Engine
open TowerDefenseSandbox.Game.Scenes
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Myra.Graphics2D.UI
open TowerDefenseSandbox.Game.Entities

type GameVictoryMessageHandler (manager: IScreenManager) =
    
    interface IMessageHandler<WavesOverMessage> with
    
        member _.Handle (_: WavesOverMessage) =
            manager.ToGameVictory()

type GameOverMessageHandler (manager: IScreenManager) =
    
    interface IMessageHandler<GameOverMessage> with
    
        member _.Handle (_: GameOverMessage) =
            manager.ToGameOver()

type GameExitMessageHandler (manager: IScreenManager) =
    
    interface IMessageHandler<GameExitMessage> with
    
        member _.Handle (_: GameExitMessage) =
            manager.ToMainMenu()
            
type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let screenWith = 1920 
    let screenHeight = 1080

    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable draw: Shape -> unit = (fun _ -> ())

    let screenManager = SceneManager ()

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

        let createMainScreen () = MainMenuScreen(screenManager, this.Content, this.Exit) :> IScene

        let createGamePlayScreen () = 
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler(screenManager) :> IMessageHandler<WavesOverMessage>)
            register.Register (GameOverMessageHandler(screenManager) :> IMessageHandler<GameOverMessage>)
            register.Register (GameExitMessageHandler(screenManager) :> IMessageHandler<GameExitMessage>)

            GamePlayScreen(entityProvider, bus, bus, draw, this.Content, screenWith, screenWith) :> IScene

        let createGameEditScreen () = GameEditorScreen(screenManager, draw, screenWith, screenHeight) :> IScene
        let createGameSettingsScreen () = EmptyScene() :> IScene
        let createGameVictoryScreen () = GameVictoryScreen(screenManager, this.Content) :> IScene
        let createGameOverScreen () = GameOverScreen(screenManager, this.Content) :> IScene

        screenManager.SetMainMenu createMainScreen
        screenManager.SetGamePlay createGamePlayScreen
        screenManager.SetGameEdit createGameEditScreen
        screenManager.SetGameSettings createGameSettingsScreen
        screenManager.SetGameVictory createGameVictoryScreen
        screenManager.SetGameOver createGameOverScreen

        (screenManager :> IScreenManager).ToMainMenu ()

    override _.Update (gameTime: GameTime) =

        screenManager.Scene.Update (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        base.Update(gameTime)

    override _.Draw (gameTime: GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()
        
        screenManager.Scene.Draw (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        spriteBatch.End()

        Desktop.Render ()

[<EntryPoint>]
let main _ =
    let game = new TheGame()
    game.Run()

    0