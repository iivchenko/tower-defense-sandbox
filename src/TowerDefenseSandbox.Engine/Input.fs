namespace TowerDefenseSandbox.Engine.Input

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine

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

type AggregatedInputController (inputs: IInputController list) =
    interface IInputController with 
        member _.Update (delta: float32<second>) =
            inputs |> List.iter (fun x -> x.Update delta)