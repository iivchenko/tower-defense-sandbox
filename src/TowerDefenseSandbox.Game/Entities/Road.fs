namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Road (position: Vector, width: float32, height: float32, draw: Shape -> unit) = 

    let (Vector(x, y)) = position
    let body = Rectangle(x, y, width, height, true, Color.grey)

    interface IEntity with

        member _.Update (_: float32<second>) =
            ()
            
        member _.Draw (_: float32<second>) =
            
            draw body

