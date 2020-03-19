namespace TowerDefenseSandbox.Engine.Input

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

[<RequireQualifiedAccess>]
type Key =
    | Esc
    | Enter
    | None

[<RequireQualifiedAccess>]
type Button = 
    | Left
    | Middle
    | Right

type MouseButtonPressedMessage(button: Button, x: int, y: int) = 

    member _.Button = button
    member _.X = x
    member _.Y = y   

 type KeyPresedMessage (key: Key) = 

    member _.Key = key

type IInputController =
    abstract Update: float32<second> -> unit

type AggregatedInputController (inputs: IInputController list) =
    interface IInputController with 
        member _.Update (delta: float32<second>) =
            inputs |> List.iter (fun x -> x.Update delta)