namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame
open Fame.Graphics

open TowerDefenseSandbox.Game

type EnemyFactory (pushMessage: EnemyMessage -> unit) = 
    
    let mutable path = []

    member _.CreateRegular (center: Vector<pixel>, wave: int) =
        Enemy.CreateRegular (center, path, wave, pushMessage)

    member _.CreateFast (center: Vector<pixel>, wave: int) =
        Enemy.CreateFast (center, path, wave, pushMessage)

    member _.CreateHard (center: Vector<pixel>, wave: int) =
        Enemy.CreateHard (center, path, wave, pushMessage)

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

    let rec spawn enemy wave =
        match enemy with 
        | Standard -> factory.CreateRegular (position, wave) |> ignore
        | Fast     -> factory.CreateFast    (position, wave) |> ignore
        | Hard     -> factory.CreateHard    (position, wave) |> ignore
    member _.Spawn (enemyType: EnemyType, wave: int) = enemy <- Some (enemyType, wave)

    interface IEntity with

        member _.Update (delta: float32<second>) =
            match enemy with 
            | None -> ()
            | Some (enemyTpe, wave) -> 
                enemy <- None
                spawn enemyTpe wave

            if k <= maxK then k <- k + factor * frequency * delta else k <- 0.0f
            
        member _.Draw() = Shape(Circle(x, y, radius * k, false, Color.red)::body::[])