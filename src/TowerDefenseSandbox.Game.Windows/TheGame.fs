namespace TowerDefenseSandbox.Game.Windows

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Myra

open Fame
open Fame.Input
open Fame.Messaging
open Fame.Scene
open Fame.Graphics
open TowerDefenseSandbox.Game.Scenes
open TowerDefenseSandbox.Game.Entities

module GlobalContext =
    let mutable screenHeight = 0
    let mutable screenWidth = 0
    let mutable maze = []
    let mutable manager = Unchecked.defaultof<ISceneManager>
    let mutable content =  Unchecked.defaultof<ContentManager>
    let mutable draw = (fun (camera: CameraMatrix option) (shape: Shape) -> ())
    let mutable exit = (fun () -> ())

// Main Menu
type MainMenuStartGameMessageHandler () =
    
    interface IMessageHandler<MainMenuStartGameMessage> with
    
        member _.Handle (_: MainMenuStartGameMessage) =

            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register(GamePlaySetupStartGameMessageHandler())
            register.Register(GamePlaySetupExitMessageHandler())

            GlobalContext.manager.Scene <- GamePlaySetupScene(bus, GlobalContext.content)

and ExitApplicationMessageHandler () =
    
    interface IMessageHandler<ExitApplicationMessage> with
    
        member _.Handle (_: ExitApplicationMessage) = GlobalContext.exit()

// Game Play Setup
and GamePlaySetupStartGameMessageHandler () =
    
    interface IMessageHandler<GamePlaySetupStartGameMessage> with 
        
        member _.Handle(message: GamePlaySetupStartGameMessage) = 
            let camera = Camera (0.5f, 10.0f)
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([KeyboardController([Key.Esc], bus); MouseController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler())
            register.Register (GameVictoryMessageHandler())
            register.Register (GameOverMessageHandler())
            register.Register (GameExitMessageHandler())
            
            GlobalContext.maze <- message.Maze

            let mapInfo = { ScreenWidth = GlobalContext.screenWidth; ScreenHeight = GlobalContext.screenHeight; Maze = message.Maze }
            GlobalContext.manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, GlobalContext.draw, GlobalContext.content, mapInfo, 1.0f)

and GamePlaySetupExitMessageHandler () =
    
    interface IMessageHandler<GamePlaySetupExitMessage> with 
        
        member _.Handle(_: GamePlaySetupExitMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (MainMenuStartGameMessageHandler())
            register.Register (ExitApplicationMessageHandler())

            GlobalContext.manager.Scene <- MainMenuScene(bus, GlobalContext.content)

// Game Play
and GameVictoryMessageHandler () =
    
    interface IMessageHandler<GameVictoryMessage> with
    
        member _.Handle (_: GameVictoryMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryRestartMessageHandler())
            register.Register (GameVictoryExitMessageHandler())

            GlobalContext.manager.Scene <- GameVictoryScene(bus, GlobalContext.content)

and GameOverMessageHandler () =
    
    interface IMessageHandler<GameOverMessage> with
    
        member _.Handle (_: GameOverMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (RestartGameOverMessageHandler())
            register.Register (ExitGameOverMessageHandler())

            GlobalContext.manager.Scene <- GameOverScene(bus, GlobalContext.content)

and GameExitMessageHandler () =
    
    interface IMessageHandler<GameExitMessage> with
    
        member _.Handle (_: GameExitMessage) =

            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (MainMenuStartGameMessageHandler())
            register.Register (ExitApplicationMessageHandler())

            GlobalContext.manager.Scene <- MainMenuScene(bus, GlobalContext.content)

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

and KeyboardGamePlayMessageHandler() =
    
    interface IMessageHandler<KeyPresedMessage> with 

        member _.Handle(message: KeyPresedMessage) = 
            match message.Key with
            | Key.Esc -> 
                let bus = MessageBus()
                let register = bus :> IMessageHandlerRegister
                register.Register (MainMenuStartGameMessageHandler())
                register.Register (ExitApplicationMessageHandler())

                GlobalContext.manager.Scene <- MainMenuScene(bus, GlobalContext.content)
            | _ -> ()

// Game Victory
and GameVictoryRestartMessageHandler () =
    
    interface IMessageHandler<GameVictoryRestartMessage> with
        
        member _.Handle(_: GameVictoryRestartMessage) =
            let camera = Camera (0.5f, 10.0f)
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([KeyboardController([Key.Esc], bus); MouseController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler())
            register.Register (GameVictoryMessageHandler())
            register.Register (GameOverMessageHandler())
            register.Register (GameExitMessageHandler())

            let mapInfo = { 
                ScreenWidth = GlobalContext.screenWidth; 
                ScreenHeight = GlobalContext.screenHeight; 
                Maze = GlobalContext.maze }

            GlobalContext.manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, GlobalContext.draw, GlobalContext.content, mapInfo, 1.0f)

and GameVictoryExitMessageHandler () =
    
    interface IMessageHandler<GameVictoryExitMessage> with
        
        member _.Handle(_: GameVictoryExitMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (MainMenuStartGameMessageHandler())
            register.Register (ExitApplicationMessageHandler())

            GlobalContext.manager.Scene <- MainMenuScene(bus, GlobalContext.content)

// Game Over
and RestartGameOverMessageHandler () =
    
    interface IMessageHandler<RestartGameOverMessage> with
    
        member _.Handle (_: RestartGameOverMessage) =
            let camera = Camera (0.5f, 10.0f)
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([KeyboardController([Key.Esc], bus); MouseController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (MouseGamePlayMessageHandler(bus))
            register.Register (KeyboardGamePlayMessageHandler())
            register.Register (GameVictoryMessageHandler())
            register.Register (GameOverMessageHandler())
            register.Register (GameExitMessageHandler())

            let mapInfo = { 
                ScreenWidth = GlobalContext.screenWidth
                ScreenHeight = GlobalContext.screenHeight
                Maze = GlobalContext.maze }

            GlobalContext.manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, GlobalContext.draw, GlobalContext.content, mapInfo, 1.0f)

and ExitGameOverMessageHandler() =
    
    interface IMessageHandler<ExitGameOverMessage> with
    
        member _.Handle (_: ExitGameOverMessage) =
            let bus = MessageBus()
            let register = bus :> IMessageHandlerRegister
            register.Register (MainMenuStartGameMessageHandler())
            register.Register (ExitApplicationMessageHandler())

            GlobalContext.manager.Scene <- MainMenuScene(bus, GlobalContext.content)

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
        
        GlobalContext.manager <- this
        GlobalContext.content <- this.Content
        GlobalContext.screenWidth <- GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width 
        GlobalContext.screenHeight <- GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height
        GlobalContext.draw <- Graphic.draw spriteBatch
        GlobalContext.exit <- this.Exit

        graphics.PreferredBackBufferWidth <- GlobalContext.screenWidth 
        graphics.PreferredBackBufferHeight <- GlobalContext.screenHeight
        
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
        register.Register (MainMenuStartGameMessageHandler())
        register.Register (ExitApplicationMessageHandler())

        scene <- MainMenuScene(bus, this.Content)

    override _.Update (gameTime: GameTime) =

        match this.IsActive with 
        | true -> scene.Update (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)
        | _ -> ()

        base.Update(gameTime)

    override _.Draw (gameTime: GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)
        
        scene.Draw (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)