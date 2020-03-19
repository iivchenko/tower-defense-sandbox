open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Myra
open Myra.Graphics2D.UI

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Engine.Input
open TowerDefenseSandbox.Engine.Messaging
open TowerDefenseSandbox.Engine.Scene
open TowerDefenseSandbox.Engine.MonoGame
open TowerDefenseSandbox.Engine.MonoGame.Input
open TowerDefenseSandbox.Engine.MonoGame.Graphic
open TowerDefenseSandbox.Game.Engine
open TowerDefenseSandbox.Game.Scenes
open TowerDefenseSandbox.Game.Entities

// Main Menu
type StartGameMessageHandler (
                                manager: ISceneManager,
                                draw: Shape -> unit, 
                                content: ContentManager, 
                                screenWidth: int, 
                                screenHeight: int,
                                exit: unit -> unit) =
    
    interface IMessageHandler<StartGameMessage> with
    
        member _.Handle (_: StartGameMessage) =
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([MonoGameKeyboardInputController([Key.Esc], bus); MonoGameMouseInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler(bus, manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

and EditGameMessageHandler (
                           manager: ISceneManager,
                           draw: Shape -> unit, 
                           content: ContentManager, 
                           screenWidth: int, 
                           screenHeight: int,
                           exit: unit -> unit) =
    
    interface IMessageHandler<EditGameMessage> with
    
        member _.Handle (_: EditGameMessage) = 
            let bus = MessageBus()
            let input = AggregatedInputController([MonoGameKeyboardInputController([Key.Esc; Key.Enter], bus); MonoGameMouseInputController(bus)])
            let register = bus :> IMessageHandlerRegister
            register.Register (KeyboardGameEditorMessageHandler(bus, manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (MouseGameEditorMessageHandler(bus))

            manager.Scene <- GameEditorScene(input, register, draw, screenWidth, screenHeight)

and SettingsGameMessageHandler (manager: ISceneManager) =
    
    interface IMessageHandler<SettingsGameMessage> with
    
        member _.Handle (_: SettingsGameMessage) =

            manager.Scene <- EmptyScene()

and ExitApplicationMessageHandler (exit: unit -> unit) =
    
    interface IMessageHandler<ExitApplicationMessage> with
    
        member _.Handle (_: ExitApplicationMessage) = exit()

// Game Play
and GameVictoryMessageHandler (
                                manager: ISceneManager,
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
                            manager: ISceneManager,
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
                            manager: ISceneManager,
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

and MouseGamePlayMessageHandler(queue: IMessageQueue) =
    interface IMessageHandler<MouseButtonPressedMessage> with
        member _.Handle(message: MouseButtonPressedMessage) =
            match message.Button with 
            | Button.Left ->
                queue.Push(GamePlayInteractionMessage(message.X, message.Y))
            | _ -> ()

and KeyboardGamePlayMessageHandler(
                                    bus: IMessageQueue,
                                    manager: ISceneManager,
                                    draw: Shape -> unit, 
                                    content: ContentManager, 
                                    screenWidth: int, 
                                    screenHeight: int,
                                    exit: unit -> unit) =
    
    interface IMessageHandler<KeyPresedMessage> with 

        member _.Handle(message: KeyPresedMessage) = 
            match message.Key with
            | Key.Esc -> 
                let bus = MessageBus()
                let register = bus :> IMessageHandlerRegister
                register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
                register.Register (EditGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
                register.Register (SettingsGameMessageHandler(manager))
                register.Register (ExitApplicationMessageHandler(exit))

                manager.Scene <- MainMenuScene(bus, content)
            | _ -> ()

// Game Edit
and KeyboardGameEditorMessageHandler(
                                      bus: IMessageQueue,
                                      manager: ISceneManager,
                                      draw: Shape -> unit, 
                                      content: ContentManager, 
                                      screenWidth: int, 
                                      screenHeight: int,
                                      exit: unit -> unit) =
    
    interface IMessageHandler<KeyPresedMessage> with 

        member _.Handle(message: KeyPresedMessage) = 
            match message.Key with
            | Key.Esc -> 
                let bus = MessageBus()
                let register = bus :> IMessageHandlerRegister
                register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
                register.Register (EditGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
                register.Register (SettingsGameMessageHandler(manager))
                register.Register (ExitApplicationMessageHandler(exit))

                manager.Scene <- MainMenuScene(bus, content)
            | Key.Enter -> 
                bus.Push(SaveGameEditMessage())
            | _ -> ()

and MouseGameEditorMessageHandler(queue: IMessageQueue) =
    interface IMessageHandler<MouseButtonPressedMessage> with
        member _.Handle(message: MouseButtonPressedMessage) =
            match message.Button with 
            | Button.Left ->
                queue.Push(PlaceEntityMessage(message.X, message.Y))
            | Button.Middle -> 
                queue.Push(UpdateEditMessage())
            | Button.Right -> 
                queue.Push(RemoveEntityMessage(message.X, message.Y))
            | _ -> ()

// Game Victory
and GameVictoryRestartMessageHandler (
                                        manager: ISceneManager,
                                        draw: Shape -> unit, 
                                        content: ContentManager, 
                                        screenWidth: int, 
                                        screenHeight: int,
                                        exit: unit -> unit) =
    
    interface IMessageHandler<GameVictoryRestartMessage> with
        
        member _.Handle(_: GameVictoryRestartMessage) =
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([MonoGameKeyboardInputController([Key.Esc], bus); MonoGameMouseInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler(bus, manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

and GameVictoryExitMessageHandler (
                                    manager: ISceneManager,
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
                                    manager: ISceneManager,
                                    draw: Shape -> unit, 
                                    content: ContentManager, 
                                    screenWidth: int, 
                                    screenHeight: int,
                                    exit: unit -> unit) =
    
    interface IMessageHandler<RestartGameOverMessage> with
    
        member _.Handle (_: RestartGameOverMessage) =
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([MonoGameKeyboardInputController([Key.Esc], bus); MonoGameMouseInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler(bus, manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

and ExitGameOverMessageHandler(
                                manager: ISceneManager,
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

    let mutable scene: IScene = EmptyScene() :> IScene

    interface ISceneManager with 
        
        member _.Scene
            with get() = scene
            and set(value) = scene <- value

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
        register.Register (StartGameMessageHandler(this, draw, this.Content, screenWidth, screenHeight, this.Exit))
        register.Register (EditGameMessageHandler(this, draw, this.Content, screenWidth, screenHeight, this.Exit))
        register.Register (SettingsGameMessageHandler(this))
        register.Register (ExitApplicationMessageHandler(this.Exit))

        scene <- MainMenuScene(bus, this.Content)

    override _.Update (gameTime: GameTime) =

        scene.Update (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        base.Update(gameTime)

    override _.Draw (gameTime: GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)

        spriteBatch.Begin()
        
        scene.Draw (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        spriteBatch.End()

        Desktop.Render ()

[<EntryPoint>]
let main _ =
    let game = new TheGame()
    game.Run()

    0