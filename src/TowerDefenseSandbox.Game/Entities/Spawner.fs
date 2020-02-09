namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type EnemyFactory (draw: Shape -> unit, entityProvider: IEntityProvider, queue: IMessageQueue) = 
    
    let mutable path = []

    member _.Create (center: Vector) =
        Enemy(100, draw, center, path, queue) |> entityProvider.RegisterEntity

    member _.UpdatePath (newPath: Vector list) =
        path <- newPath

type Spawner (position: Vector, draw: Shape -> unit, factory: EnemyFactory) =

    [<Literal>] 
    let spawnTime = 1.0f<second>

    let radius = 15.0f
    let mutable nextSpawn = spawnTime
    let (Vector(x, y)) = position
    let body = Circle(x, y, radius, false, Color.aquamarine)

    // Blinker
    let maxK = 1.0f
    let factor = 0.1f
    let frequency = 25.0f<1/second>
    let mutable k = 0.0f

    interface IEntity with

        member _.Update (delta: float32<second>) =
            if nextSpawn < 0.0f<second>
                then 
                    factory.Create position
                    nextSpawn <- spawnTime
                else
                    nextSpawn <- nextSpawn - delta

            if k <= maxK then k <- k + factor * frequency * delta else k <- 0.0f
            
        member _.Draw (_: float32<second>) =
            Circle(x, y, radius * k, false, Color.red) |> draw
            draw body