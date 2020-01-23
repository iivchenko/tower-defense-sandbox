namespace TowerDefenseSandbox.Game.Engine

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

type IScreen =
    abstract member Update: float32<second> -> unit
    abstract member Draw: float32<second> -> unit

type EmptyScreen () =
    interface IScreen with
        member _.Update (_) = ()
        member _.Draw (_) = ()