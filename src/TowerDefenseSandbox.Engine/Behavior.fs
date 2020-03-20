namespace TowerDefenseSandbox.Engine

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine

module Behavior =
    
    let seek (character: Vector<pixel>) (target: Vector<pixel>) (speed: float32<pixel/second>): Vector<pixel/second> = 
        Vector.direction character target
        |> Vector.normalize 
        |> (*) speed

    let face character target = 
        let direction = Vector.direction character target
        let (x, y) = Vector.unwrap direction
        atan2 -x y

