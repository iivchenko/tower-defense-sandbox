open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Myra
open Myra.Graphics2D.UI

open Fame
open Fame.Input
open Fame.Messaging
open Fame.Scene
open Fame.Graphics
open TowerDefenseSandbox.Game.Entities
open TowerDefenseSandbox.Game.Scenes
open TowerDefenseSandbox.Game.Editor.Scenes

// Game Edit
type KeyboardGameEditorMessageHandler(
                                      bus: IMessageQueue,
                                      manager: ISceneManager,
                                      draw: CameraMatrix option -> Shape -> unit, 
                                      screenWidth: int, 
                                      screenHeight: int,
                                      content: ContentManager, 
                                      exit: unit -> unit) =
    
    interface IMessageHandler<KeyPresedMessage> with 

        member _.Handle(message: KeyPresedMessage) = 
            match message.Key with
            | Key.Esc -> 
               exit()
            | Key.Enter -> 
                bus.Push(SaveGameEditMessage())
                
            | Key.Space ->
                let camera = Camera (0.5f, 10.0f)
                let entityProvider = new EntityProvider()
                let bus = MessageBus()
                let input = AggregatedInputController([KeyboardController([Key.Esc], bus); MouseController(bus)]) 
                let register = bus :> IMessageHandlerRegister
                register.Register (MouseGamePlayMessageHandler(bus))
                register.Register (KeyboardGamePlayMessageHandler(manager, content, draw, screenWidth, screenHeight, exit))
                register.Register (GameVictoryMessageHandler(manager, content, draw, screenWidth, screenHeight, exit))
                register.Register (GameOverMessageHandler(manager, content, draw, screenWidth, screenHeight, exit))
                register.Register (GameExitMessageHandler(manager, content, draw, screenWidth, screenHeight, exit))

                manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight, 1.0f)
            | _ -> ()

// Game Play
and GameVictoryMessageHandler (
                                manager: ISceneManager,
                                content: ContentManager, 
                                draw: CameraMatrix option -> Shape -> unit, 
                                screenWidth: int, 
                                screenHeight: int,
                                exit: unit -> unit) =
    
    interface IMessageHandler<WavesOverMessage> with
    
        member _.Handle (_: WavesOverMessage) =
            let bus = MessageBus()
            let input = AggregatedInputController([KeyboardController([Key.Esc; Key.Enter; Key.Space], bus); MouseController(bus)])
            let register = bus :> IMessageHandlerRegister
            register.Register (KeyboardGameEditorMessageHandler(bus, manager, draw, screenWidth, screenHeight, content, exit))
            register.Register (MouseGameEditorMessageHandler(bus))

            manager.Scene <- GameEditorScene(input, register, draw, screenWidth, screenHeight)

and GameOverMessageHandler (
                            manager: ISceneManager,
                            content: ContentManager, 
                            draw: CameraMatrix option -> Shape -> unit, 
                            screenWidth: int, 
                            screenHeight: int,
                            exit: unit -> unit) =
    
    interface IMessageHandler<GameOverMessage> with
    
        member _.Handle (_: GameOverMessage) =
            let bus = MessageBus()
            let input = AggregatedInputController([KeyboardController([Key.Esc; Key.Enter; Key.Space], bus); MouseController(bus)])
            let register = bus :> IMessageHandlerRegister
            register.Register (KeyboardGameEditorMessageHandler(bus, manager, draw, screenWidth, screenHeight, content, exit))
            register.Register (MouseGameEditorMessageHandler(bus))

            manager.Scene <- GameEditorScene(input, register, draw, screenWidth, screenHeight)

and GameExitMessageHandler (   
                            manager: ISceneManager,
                            content: ContentManager, 
                            draw: CameraMatrix option -> Shape -> unit, 
                            screenWidth: int, 
                            screenHeight: int,
                            exit: unit -> unit) =
    
    interface IMessageHandler<GameExitMessage> with
    
        member _.Handle (_: GameExitMessage) =

            let bus = MessageBus()
            let input = AggregatedInputController([KeyboardController([Key.Esc; Key.Enter; Key.Space], bus); MouseController(bus)])
            let register = bus :> IMessageHandlerRegister
            register.Register (KeyboardGameEditorMessageHandler(bus, manager, draw, screenWidth, screenHeight, content, exit))
            register.Register (MouseGameEditorMessageHandler(bus))

            manager.Scene <- GameEditorScene(input, register, draw, screenWidth, screenHeight)

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
                                    content: ContentManager, 
                                    draw: CameraMatrix option -> Shape -> unit, 
                                    screenWidth: int, 
                                    screenHeight: int,
                                    exit: unit -> unit) =
    
    interface IMessageHandler<KeyPresedMessage> with 

        member _.Handle(message: KeyPresedMessage) = 
            match message.Key with
            | Key.Esc -> 
                let bus = MessageBus()
                let input = AggregatedInputController([KeyboardController([Key.Esc; Key.Enter; Key.Space], bus); MouseController(bus)])
                let register = bus :> IMessageHandlerRegister
                register.Register (KeyboardGameEditorMessageHandler(bus, manager, draw, screenWidth, screenHeight, content, exit))
                register.Register (MouseGameEditorMessageHandler(bus))

                manager.Scene <- GameEditorScene(input, register, draw, screenWidth, screenHeight)
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

type TheGame () as this =
    inherit Microsoft.Xna.Framework.Game()

    let graphics = new GraphicsDeviceManager(this)
    let screenWidth = 1920 
    let screenHeight = 1080

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

        graphics.PreferredBackBufferWidth <- screenWidth
        graphics.PreferredBackBufferHeight <- screenHeight

        this.Window.AllowUserResizing <- true

        graphics.ApplyChanges();

        base.IsMouseVisible <- true

        MyraEnvironment.Game <- this

        let bus = MessageBus()
        let draw = Graphic.draw spriteBatch
        let input = AggregatedInputController([KeyboardController([Key.Esc; Key.Enter; Key.Space], bus); MouseController(bus)])
        let register = bus :> IMessageHandlerRegister
        register.Register (KeyboardGameEditorMessageHandler(bus, this, draw, screenWidth, screenHeight, this.Content, this.Exit))
        register.Register (MouseGameEditorMessageHandler(bus))

        scene <- GameEditorScene(input, register, draw, screenWidth, screenHeight)

    override _.Update (gameTime: GameTime) =

        scene.Update (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        base.Update(gameTime)

    override _.Draw (gameTime: GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)
        
        scene.Draw (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        Desktop.Render ()

[<EntryPoint>]
let main _ =
    let game = new TheGame()
    game.Run()

    0