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
open Microsoft.Xna.Framework.Content

// Main Menu
type StartGameMessageHandler (
                                manager: SceneManager,
                                draw: Shape -> unit, 
                                content: ContentManager, 
                                screenWidth: int, 
                                screenHeight: int,
                                exit: unit -> unit) =
    
    interface IMessageHandler<StartGameMessage> with
    
        member _.Handle (_: StartGameMessage) =
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

and EditGameMessageHandler (
                           manager: SceneManager,
                           draw: Shape -> unit, 
                           content: ContentManager, 
                           screenWidth: int, 
                           screenHeight: int,
                           exit: unit -> unit) =
    
    interface IMessageHandler<EditGameMessage> with
    
        member _.Handle (_: EditGameMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (ExitGameEditorMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GameEditorScene(bus, draw, screenWidth, screenHeight)

and SettingsGameMessageHandler (manager: SceneManager) =
    
    interface IMessageHandler<SettingsGameMessage> with
    
        member _.Handle (_: SettingsGameMessage) =          

            manager.Scene <- EmptyScene()

and ExitApplicationMessageHandler (exit: unit -> unit) =
    
    interface IMessageHandler<ExitApplicationMessage> with
    
        member _.Handle (_: ExitApplicationMessage) = exit()

// Game Play
and GameVictoryMessageHandler (
                                manager: SceneManager,
                                draw: Shape -> unit, 
                                content: ContentManager, 
                                screenWidth: int, 
                                screenHeight: int,
                                exit: unit -> unit) =
    
    interface IMessageHandler<WavesOverMessage> with
    
        member _.Handle (_: WavesOverMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryRestartMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameVictoryExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GameVictoryScene(bus, content)

and GameOverMessageHandler (
                            manager: SceneManager,
                            draw: Shape -> unit, 
                            content: ContentManager, 
                            screenWidth: int, 
                            screenHeight: int,
                            exit: unit -> unit) =
    
    interface IMessageHandler<GameOverMessage> with
    
        member _.Handle (_: GameOverMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (RestartGameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (ExitGameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GameOverScene(bus, content)

and GameExitMessageHandler (   
                            manager: SceneManager,
                            draw: Shape -> unit, 
                            content: ContentManager, 
                            screenWidth: int, 
                            screenHeight: int,
                            exit: unit -> unit) =
    
    interface IMessageHandler<GameExitMessage> with
    
        member _.Handle (_: GameExitMessage) =

            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (EditGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (SettingsGameMessageHandler(manager))
            register.Register (ExitApplicationMessageHandler(exit))

            manager.Scene <- MainMenuScene(bus, content)

// Game Edit
and ExitGameEditorMessageHandler(
                                   manager: SceneManager,
                                   draw: Shape -> unit, 
                                   content: ContentManager, 
                                   screenWidth: int, 
                                   screenHeight: int,
                                   exit: unit -> unit) =

    interface IMessageHandler<ExitGameEditorMessage> with 
        member _.Handle(_: ExitGameEditorMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (EditGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (SettingsGameMessageHandler(manager))
            register.Register (ExitApplicationMessageHandler(exit))

            manager.Scene <- MainMenuScene(bus, content)

// Game Victory
and GameVictoryRestartMessageHandler (
                                        manager: SceneManager,
                                        draw: Shape -> unit, 
                                        content: ContentManager, 
                                        screenWidth: int, 
                                        screenHeight: int,
                                        exit: unit -> unit) =
    
    interface IMessageHandler<GameVictoryRestartMessage> with
        
        member _.Handle(_: GameVictoryRestartMessage) =
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

and GameVictoryExitMessageHandler (
                                    manager: SceneManager,
                                    draw: Shape -> unit, 
                                    content: ContentManager, 
                                    screenWidth: int, 
                                    screenHeight: int,
                                    exit: unit -> unit) =
    
    interface IMessageHandler<GameVictoryExitMessage> with
        
        member _.Handle(_: GameVictoryExitMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (EditGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (SettingsGameMessageHandler(manager))
            register.Register (ExitApplicationMessageHandler(exit))

            manager.Scene <- MainMenuScene(bus, content)

// Game Over
and RestartGameOverMessageHandler (
                                    manager: SceneManager,
                                    draw: Shape -> unit, 
                                    content: ContentManager, 
                                    screenWidth: int, 
                                    screenHeight: int,
                                    exit: unit -> unit) =
    
    interface IMessageHandler<RestartGameOverMessage> with
    
        member _.Handle (_: RestartGameOverMessage) =
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

and ExitGameOverMessageHandler(
                                manager: SceneManager,
                                draw: Shape -> unit, 
                                content: ContentManager, 
                                screenWidth: int, 
                                screenHeight: int,
                                exit: unit -> unit) =
    
    interface IMessageHandler<ExitGameOverMessage> with
    
        member _.Handle (_: ExitGameOverMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (EditGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (SettingsGameMessageHandler(manager))
            register.Register (ExitApplicationMessageHandler(exit))

            manager.Scene <- MainMenuScene(bus, content)

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let screenWidth = 1920 
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

        graphics.PreferredBackBufferWidth <- screenWidth
        graphics.PreferredBackBufferHeight <- screenHeight
        
        #if RELEASE 
        graphics.IsFullScreen <- true
        #endif

        graphics.ApplyChanges();

        base.IsMouseVisible <- true

        MyraEnvironment.Game <- this
       
        let bus = MessageBus()
        let register = bus :> IMessageHandlerRegister
        register.Register (StartGameMessageHandler(screenManager, draw, this.Content, screenWidth, screenHeight, this.Exit))
        register.Register (EditGameMessageHandler(screenManager, draw, this.Content, screenWidth, screenHeight, this.Exit))
        register.Register (SettingsGameMessageHandler(screenManager))
        register.Register (ExitApplicationMessageHandler(this.Exit))

        screenManager.Scene <- MainMenuScene(bus, this.Content)

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