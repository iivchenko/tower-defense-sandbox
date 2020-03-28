namespace TowerDefenseSandbox.Game.Windows

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
open TowerDefenseSandbox.Game.Scenes
open TowerDefenseSandbox.Game.Entities

// Main Menu
type StartGameMessageHandler (
                                manager: ISceneManager,
                                draw: CameraMatrix option -> Shape -> unit, 
                                content: ContentManager, 
                                screenWidth: int, 
                                screenHeight: int,
                                exit: unit -> unit) =
    
    interface IMessageHandler<StartGameMessage> with
    
        member _.Handle (_: StartGameMessage) =
            let camera = Camera (0.5f, 10.0f)
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([MonoGameKeyboardInputController([Key.Esc], bus); MonoGameMouseInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))            

            manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

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
                                draw: CameraMatrix option -> Shape -> unit, 
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
                            draw: CameraMatrix option -> Shape -> unit, 
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
                            draw: CameraMatrix option -> Shape -> unit, 
                            content: ContentManager, 
                            screenWidth: int, 
                            screenHeight: int,
                            exit: unit -> unit) =
    
    interface IMessageHandler<GameExitMessage> with
    
        member _.Handle (_: GameExitMessage) =

            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (SettingsGameMessageHandler(manager))
            register.Register (ExitApplicationMessageHandler(exit))

            manager.Scene <- MainMenuScene(bus, content)

and MouseGamePlayMessageHandler(queue: IMessageQueue) =

    let mutable left = MouseButtonState.Released
    let mutable drag = false
    interface IMessageHandler<MouseInputMessage> with
        member _.Handle(message: MouseInputMessage) =
            match message.Event with 
            |  (MouseButton(MouseButton.Left, MouseButtonState.Released, mouse)) when not drag ->
                left <- MouseButtonState.Released
                queue.Push(GamePlayInteractionMessage(mouse.X, mouse.Y))
            |  (MouseButton(MouseButton.Left, MouseButtonState.Released, _)) when drag ->
                left <- MouseButtonState.Released
                drag <- false
            |  (MouseButton(MouseButton.Left, MouseButtonState.Pressed, _)) ->
                left <- MouseButtonState.Pressed
            | (MouseMoved(position', position'')) when left = MouseButtonState.Pressed && not drag && Vector.length (position'' - position') > 5.0f<pixel> -> 
                drag <- true;
                queue.Push(CameraMoveMessage(position'' - position'))
            | (MouseMoved(position', position'')) when left = MouseButtonState.Pressed && drag -> 
                queue.Push(CameraMoveMessage(position'' - position'))
            | (MouseScrollWheelChanged(value, value')) -> 
                queue.Push(CameraZoomMessage((value' - value)/1000.0f))
            | _ -> ()

and KeyboardGamePlayMessageHandler(
                                    manager: ISceneManager,
                                    draw: CameraMatrix option -> Shape -> unit, 
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
                register.Register (SettingsGameMessageHandler(manager))
                register.Register (ExitApplicationMessageHandler(exit))

                manager.Scene <- MainMenuScene(bus, content)
            | _ -> ()

// Game Edit
and KeyboardGameEditorMessageHandler(
                                      bus: IMessageQueue,
                                      manager: ISceneManager,
                                      draw: CameraMatrix option -> Shape -> unit, 
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
                register.Register (SettingsGameMessageHandler(manager))
                register.Register (ExitApplicationMessageHandler(exit))

                manager.Scene <- MainMenuScene(bus, content)
            | Key.Enter -> 
                bus.Push(SaveGameEditMessage())
            | _ -> ()

and MouseGameEditorMessageHandler(queue: IMessageQueue) =
    interface IMessageHandler<MouseInputMessage> with
        member _.Handle(message: MouseInputMessage) =
            match message.Event with 
            | (MouseButton(MouseButton.Left, MouseButtonState.Released, mouse)) ->
                queue.Push(PlaceEntityMessage(mouse.X, mouse.Y))
            | (MouseButton(MouseButton.Middle, MouseButtonState.Released, _)) -> 
                queue.Push(UpdateEditMessage())
            |(MouseButton(MouseButton.Right, MouseButtonState.Released, mouse)) -> 
                queue.Push(RemoveEntityMessage(mouse.X, mouse.Y))
            | _ -> ()

// Game Victory
and GameVictoryRestartMessageHandler (
                                        manager: ISceneManager,
                                        draw: CameraMatrix option -> Shape -> unit, 
                                        content: ContentManager, 
                                        screenWidth: int, 
                                        screenHeight: int,
                                        exit: unit -> unit) =
    
    interface IMessageHandler<GameVictoryRestartMessage> with
        
        member _.Handle(_: GameVictoryRestartMessage) =
            let camera = Camera (0.5f, 10.0f)
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([MonoGameKeyboardInputController([Key.Esc], bus); MonoGameMouseInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

and GameVictoryExitMessageHandler (
                                    manager: ISceneManager,
                                    draw: CameraMatrix option -> Shape -> unit, 
                                    content: ContentManager, 
                                    screenWidth: int, 
                                    screenHeight: int,
                                    exit: unit -> unit) =
    
    interface IMessageHandler<GameVictoryExitMessage> with
        
        member _.Handle(_: GameVictoryExitMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (SettingsGameMessageHandler(manager))
            register.Register (ExitApplicationMessageHandler(exit))

            manager.Scene <- MainMenuScene(bus, content)

// Game Over
and RestartGameOverMessageHandler (
                                    manager: ISceneManager,
                                    draw: CameraMatrix option -> Shape -> unit, 
                                    content: ContentManager, 
                                    screenWidth: int, 
                                    screenHeight: int,
                                    exit: unit -> unit) =
    
    interface IMessageHandler<RestartGameOverMessage> with
    
        member _.Handle (_: RestartGameOverMessage) =
            let camera = Camera (0.5f, 10.0f)
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([MonoGameKeyboardInputController([Key.Esc], bus); MonoGameMouseInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight)

and ExitGameOverMessageHandler(
                                manager: ISceneManager,
                                draw: CameraMatrix option -> Shape -> unit, 
                                content: ContentManager, 
                                screenWidth: int, 
                                screenHeight: int,
                                exit: unit -> unit) =
    
    interface IMessageHandler<ExitGameOverMessage> with
    
        member _.Handle (_: ExitGameOverMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (StartGameMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (SettingsGameMessageHandler(manager))
            register.Register (ExitApplicationMessageHandler(exit))

            manager.Scene <- MainMenuScene(bus, content)

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)

    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable scene: IScene = EmptyScene() :> IScene

    interface ISceneManager with 
        
        member _.Scene
            with get() = scene
            and set(value) = scene <- value

    override _.LoadContent() =
        this.Content.RootDirectory <- "Content"
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

    override this.Initialize () =

        base.Initialize()
        
        let screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width 
        let screenHeight =  GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height

        graphics.PreferredBackBufferWidth <- screenWidth
        graphics.PreferredBackBufferHeight <- screenHeight
        
        #if RELEASE 
        graphics.IsFullScreen <- true
        #else
        this.Window.AllowUserResizing <- true
        #endif

        graphics.ApplyChanges();

        base.IsMouseVisible <- true

        MyraEnvironment.Game <- this
       
        let bus = MessageBus()
        let register = bus :> IMessageHandlerRegister
        let draw = Graphic.draw (MonoGameGraphic spriteBatch)
        register.Register (StartGameMessageHandler(this, draw, this.Content, screenWidth, screenHeight, this.Exit))
        register.Register (SettingsGameMessageHandler(this))
        register.Register (ExitApplicationMessageHandler(this.Exit))

        scene <- MainMenuScene(bus, this.Content)

    override _.Update (gameTime: GameTime) =

        scene.Update (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        base.Update(gameTime)

    override _.Draw (gameTime: GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)
        
        scene.Draw (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        Desktop.Render ()