open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Myra
open Myra.Graphics2D.UI

open Fame
open Fame.Input
open Fame.Messaging
open Fame.Scene
open TowerDefenseSandbox.Game.Scenes

// Game Edit
type KeyboardGameEditorMessageHandler(
                                      bus: IMessageQueue,
                                      exit: unit -> unit) =
    
    interface IMessageHandler<KeyPresedMessage> with 

        member _.Handle(message: KeyPresedMessage) = 
            match message.Key with
            | Key.Esc -> 
               exit()
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
        let draw = Graphic.draw (MonoGameGraphic spriteBatch)
        let input = AggregatedInputController([MonoGameKeyboardInputController([Key.Esc; Key.Enter], bus); MonoGameMouseInputController(bus)])
        let register = bus :> IMessageHandlerRegister
        register.Register (KeyboardGameEditorMessageHandler(bus, this.Exit))
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