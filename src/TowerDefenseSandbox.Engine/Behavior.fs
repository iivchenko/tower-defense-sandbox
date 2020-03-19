namespace TowerDefenseSandbox.Engine

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine

module Behavior =
    
    let seek (character: Vector) (target: Vector) (speed: float32<pixel/second>) = 
        Vector.direction character target
        |> Vector.normalize 
        |> (*) (float32 speed)

    let face (character: Vector) (target: Vector) = 
        let direction = Vector.direction character target
        let (x, y) = Vector.unwrap direction
        atan2 -x y

