namespace TowerDefenseSandbox.Game.Android

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Input.Touch
open Myra

open Fame
open Fame.Input
open Fame.Messaging
open Fame.Scene
open Fame.Graphics
open TowerDefenseSandbox.Game.Entities
open TowerDefenseSandbox.Game.Scenes

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
            let input = AggregatedInputController([ MonoGameTouchInputController(bus) ]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler())
            register.Register (GameOverMessageHandler())
            register.Register (GameExitMessageHandler())
            
            GlobalContext.maze <- message.Maze

            let mapInfo = { ScreenWidth = GlobalContext.screenWidth; ScreenHeight = GlobalContext.screenHeight; Maze = message.Maze }
            GlobalContext.manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, GlobalContext.draw, GlobalContext.content, mapInfo, 3.0f)

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
    
    interface IMessageHandler<WavesOverMessage> with
    
        member _.Handle (_: WavesOverMessage) =
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

and MonoGameTouchInputController(queue: IMessageQueue) =

    let mutable history = []
    interface IInputController with 
        member _.Update (_: float32<second>) = 
            let touches = TouchPanel.GetState() |> Seq.map (fun touch -> (touch.Id, touch.State, touch.Position.X, touch.Position.Y)) |> Seq.toList

            match touches with 
            | [] -> ()
            | (id, state, x, y)::[] when state = TouchLocationState.Pressed -> 
                history <- (id, state, x, y)::history

            | (id, state, x, y)::[] when state = TouchLocationState.Released -> 

                if List.length history = 1 
                    then 
                        let h =  List.filter (fun (xid, _, _, _) -> xid = id) history |> List.tryExactlyOne

                        match h with
                        | Some(hid, TouchLocationState.Pressed, _, _) -> 
                            history <- List.filter (fun (xid, _, _, _) -> xid <> hid) history
                            queue.Push(GamePlayInteractionMessage(int x, int y))
                        | Some(hid, _, _, _) -> 
                            history <- List.filter (fun (xid, _, _, _) -> xid <> hid) history
                        | _ -> ()
                    else
                        history <- []
            | (id, state, x, y)::[] when state = TouchLocationState.Moved ->
                let h =  List.filter (fun (xid, _, _, _) -> xid = id) history |> List.tryExactlyOne
                
                match h with
                | Some(hid, state', x', y') -> 
                    let dif = Vector.init ((x - x') * 1.0f<pixel>) ((y - y') * 1.0f<pixel>)
                    history <- List.filter (fun (xid, _, _, _) -> xid <> hid) history
                    history <- (id, (if dif = (Vector.init 0.0f<pixel> 0.0f<pixel>) && state' = TouchLocationState.Pressed then TouchLocationState.Pressed else  TouchLocationState.Moved), x, y)::history
                    queue.Push(CameraMoveMessage(dif))
                | _ -> ()
                
            | (id1, state1, x1, y1)::(id2, state2, x2, y2)::[]  when state1 = TouchLocationState.Pressed || state2 = TouchLocationState.Pressed -> 
                history <- (id1, TouchLocationState.Pressed , x1, y1)::(id2, TouchLocationState.Pressed, x2, y2)::[]

            | (id1, state1, x1, y1)::(id2, state2, x2, y2)::[]  when state1 = TouchLocationState.Moved || state2 = TouchLocationState.Moved ->
                let (_, _, x1', y1') = List.filter (fun (id, _, _, _) -> id1 = id) history |> List.exactlyOne
                let (_, _, x2', y2') = List.filter (fun (id, _, _, _) -> id2 = id) history |> List.exactlyOne

                let scale = (Vector.distance (Vector.init x1 y1) (Vector.init x2 y2)) - (Vector.distance (Vector.init x1' y1') (Vector.init x2' y2'))
                history <- (id1, TouchLocationState.Pressed, x1, y1)::(id2, TouchLocationState.Pressed, x2, y2)::[]

                queue.Push(CameraZoomMessage(scale/250.0f))

            | _ when List.length history > 0 -> 
                history <- []
            | _ -> ()

// Game Victory
and GameVictoryRestartMessageHandler () =
    
    interface IMessageHandler<GameVictoryRestartMessage> with
        
        member _.Handle(_: GameVictoryRestartMessage) =
            let camera = Camera (0.5f, 10.0f)
            let entityProvider = new EntityProvider()
            let bus = MessageBus()
            let input = AggregatedInputController([ MonoGameTouchInputController(bus) ]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler())
            register.Register (GameOverMessageHandler())
            register.Register (GameExitMessageHandler())

            let mapInfo = { 
                ScreenWidth = GlobalContext.screenWidth; 
                ScreenHeight = GlobalContext.screenHeight; 
                Maze = GlobalContext.maze }

            GlobalContext.manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, GlobalContext.draw, GlobalContext.content, mapInfo, 3.0f)

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
            let input = AggregatedInputController([MonoGameTouchInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler())
            register.Register (GameOverMessageHandler())
            register.Register (GameExitMessageHandler())

            let mapInfo = { 
                ScreenWidth = GlobalContext.screenWidth
                ScreenHeight = GlobalContext.screenHeight
                Maze = GlobalContext.maze }

            GlobalContext.manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, GlobalContext.draw, GlobalContext.content, mapInfo, 3.0f)

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

        scene.Update (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)

        base.Update(gameTime)

    override _.Draw (gameTime: GameTime) =
        
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue)
        
        scene.Draw (float32 gameTime.ElapsedGameTime.TotalSeconds * 1.0f<second>)