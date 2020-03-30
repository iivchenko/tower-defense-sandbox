namespace Fame.Input

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame
open Fame.Messaging
open Microsoft.Xna.Framework.Input

[<RequireQualifiedAccess>]
type MouseButton = 
    | Left
    | Middle
    | Right

[<RequireQualifiedAccess>]
type MouseButtonState = 
    | Released
    | Pressed

type MousePosition = 
    { X: int
      Y: int }

type MouseInputEvent =
    | MouseButton of buttonState: MouseButton * MouseButtonState * mousePossiton: MousePosition
    | MouseScrollWheelChanged of oldValue: float32 * newValue: float32
    | MouseMoved of oldPosition: Vector<pixel> * newPosition: Vector<pixel>

type MouseInputMessage(events: MouseInputEvent) =

    member _.Event = events

[<RequireQualifiedAccess>]
type Key =
    | Esc
    | Enter
    | None

 type KeyPresedMessage (key: Key) = 

    member _.Key = key

type IInputController =
    abstract Update: float32<second> -> unit

type MonoGameMouseInputController(queue: IMessageQueue) =

   let mapState state = 
       match state with 
       | ButtonState.Pressed -> MouseButtonState.Pressed
       | ButtonState.Released -> MouseButtonState.Released
       | _ -> MouseButtonState.Released

   let mutable buttons = 
       [ 
           (MouseButton.Left, MouseButtonState.Released); 
           (MouseButton.Middle, MouseButtonState.Released);
           (MouseButton.Right, MouseButtonState.Released) 
       ] |> Map.ofList

   let mutable wheele = Mouse.GetState().ScrollWheelValue

   let mutable mousePose : Vector<pixel> = Vector.init (float32 (Mouse.GetState().X) * 1.0f<pixel>) (float32 (Mouse.GetState().Y) * 1.0f<pixel>)

   interface IInputController with 
       member _.Update (_: float32<second>) =
           let state = Mouse.GetState()
           let mouse = { X = state.X;  Y = state.Y; }

           let buttons' = 
               [ 
                   (MouseButton.Left, buttons.[MouseButton.Left], mapState state.LeftButton); 
                   (MouseButton.Middle, buttons.[MouseButton.Middle], mapState state.MiddleButton); 
                   (MouseButton.Right, buttons.[MouseButton.Right], mapState state.RightButton) 
               ]

           buttons <- buttons' |> List.map (fun (key, _, state'') -> key, state'') |> Map.ofList
           
           buttons' |> List.iter(fun (button, state', state'') -> if state' <> state'' then queue.Push(MouseInputMessage(MouseButton(button, state'', mouse))) else ()) 

           if wheele <> state.ScrollWheelValue 
               then 
                   let old = wheele
                   wheele <- state.ScrollWheelValue
                   queue.Push(MouseInputMessage(MouseScrollWheelChanged(float32 old, float32 state.ScrollWheelValue)))
               else 
                   ()

           if mousePose <> (Vector.init (float32 (Mouse.GetState().X) * 1.0f<pixel>) (float32 (Mouse.GetState().Y) * 1.0f<pixel>))
               then 
                   let old = mousePose; 
                   let x = float32 (Mouse.GetState().X) * 1.0f<pixel>
                   let y = float32 (Mouse.GetState().Y) * 1.0f<pixel>
                   mousePose <- Vector.init x y
                   queue.Push(MouseInputMessage(MouseMoved(old, mousePose)))
               else 
                   ()

type MonoGameKeyboardInputController(keys: Key list, queue: IMessageQueue) =
  
   let map key = 
       match key with
       | Key.Esc -> Keys.Escape
       | Key.Enter -> Keys.Enter
       | _ -> Keys.None

   let asKey key = 
       match key with 
       | Keys.Escape -> Key.Esc
       | Keys.Enter -> Key.Enter
   let mutable keys = keys |> List.map map |> List.map (fun key -> (key, KeyState.Up))
       
   interface IInputController with
   
       member _.Update (_: float32<second>) =
           let keyboard = Keyboard.GetState()
           
           let tmp = keys |> List.map (fun (key, state) -> key, state, keyboard.Item(key))
           keys <- tmp |> List.map (fun (key, _, state'') -> key, state'')

           tmp |> List.iter (fun (key, state', state'') -> if state' <> state'' && state'' = KeyState.Up then queue.Push(KeyPresedMessage(asKey key)) else ())

type AggregatedInputController (inputs: IInputController list) =
    interface IInputController with 
        member _.Update (delta: float32<second>) =
            inputs |> List.iter (fun x -> x.Update delta)