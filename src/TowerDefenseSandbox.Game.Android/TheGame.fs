namespace TowerDefenseSandbox.Game.Android

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Input.Touch
open Myra

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Engine.Input
open TowerDefenseSandbox.Engine.Messaging
open TowerDefenseSandbox.Engine.Scene
open TowerDefenseSandbox.Engine.MonoGame
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
            let input = AggregatedInputController([ MonoGameTouchInputController(bus) ]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))            

            manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight, 3.0f)

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
            let input = AggregatedInputController([MonoGameTouchInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight, 3.0f)

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
            let input = AggregatedInputController([MonoGameTouchInputController(bus)]) 
            let register = bus :> IMessageHandlerRegister
            register.Register (GameVictoryMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameOverMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))
            register.Register (GameExitMessageHandler(manager, draw, content, screenWidth, screenHeight, exit))

            manager.Scene <- GamePlayScene(camera, input, entityProvider, bus, bus, draw, content, screenWidth, screenHeight, 3.0f)

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