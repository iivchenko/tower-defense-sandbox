namespace Fame.Input

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

type IInputController =
    abstract Update: float32<second> -> unit

type AggregatedInputController (inputs: IInputController list) =
    interface IInputController with 
        member _.Update (delta: float32<second>) =
            inputs |> List.iter (fun x -> x.Update delta)