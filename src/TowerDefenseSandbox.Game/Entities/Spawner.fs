namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame
open Fame.Graphics

open TowerDefenseSandbox.Game

type EnemyFactory (pushMessage: EnemyMessage -> unit) = 
    
    let mutable path = []

    member _.CreateRegular (center: Vector<pixel>) =
        Enemy.CreateRegular (center, path, pushMessage)

    member _.CreateFast (center: Vector<pixel>) =
        Enemy.CreateFast (center, path, pushMessage)

    member _.CreateHard (center: Vector<pixel>) =
        Enemy.CreateHard (center, path, pushMessage)

    member _.UpdatePath (newPath: Vector<pixel> list) =
        path <- newPath

type Spawner (position: Vector<pixel>, factory: EnemyFactory) =

    let radius = 15.0f<pixel>
    let (Vector(x, y)) = position
    let body = Circle(x, y, radius, false, Color.aquamarine)

    let mutable enemy = None

    // Blinker
    let maxK = 1.0f
    let factor = 0.1f
    let frequency = 25.0f<1/second>
    let mutable k = 0.0f

    let rec spawn enemy =
        match enemy with 
        | Standard -> factory.CreateRegular position |> ignore
        | Fast -> factory.CreateFast position |> ignore
        | Hard -> factory.CreateHard position |> ignore
    member _.Spawn (enemyType: EnemyType) = enemy <- Some enemyType

    interface IEntity with

        member _.Update (delta: float32<second>) =
            match enemy with 
            | None -> ()
            | Some enemyTpe -> 
                enemy <- None
                spawn enemyTpe

            if k <= maxK then k <- k + factor * frequency * delta else k <- 0.0f
            
        member _.Draw() = Shape(Circle(x, y, radius * k, false, Color.red)::body::[])