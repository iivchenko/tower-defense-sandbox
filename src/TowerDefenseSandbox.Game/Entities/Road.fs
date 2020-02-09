namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Road (position: Vector, width: float32, height: float32, draw: Shape -> unit) = 

    interface IEntity with 

        member _.Radius = width / 2.0f

        member _.Position 
            with get () = position
            and set(value: Vector) = ()

        member _.Update (time: float32<second>) =
            ()
            
        member _.Draw (time: float32<second>) =
            let (Vector(x, y)) = position
            Rectangle(x, y, width, height, true, Color.grey) |> draw

