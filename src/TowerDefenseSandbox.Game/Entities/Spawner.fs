namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open MonoGame.Extended

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type EnemyFactory (draw: Shape -> unit, entityProvider: IEntityProvider) = 
    
    let mutable path = []

    member _.Create (center: Vector) =
        Enemy(100, draw, center, entityProvider, path) |> entityProvider.RegisterEntity

    member _.UpdatePath (newPath: Vector list) =
        path <- newPath

type Spawner (zindex: int, draw: Shape -> unit, factory: EnemyFactory) =

    [<Literal>] 
    let spawnTime = 1.0f<second>

    let radius = 15.0f
    let mutable nextSpawn = spawnTime

    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)

    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (time: float32<second>) (position: RectangleF) =
            if nextSpawn < 0.0f<second>
                then 
                    factory.Create (center position)
                    nextSpawn <- spawnTime
                else
                    nextSpawn <- nextSpawn - time
            
        member _.Draw (time: float32<second>) (position: RectangleF) =
            let (Vector(x, y)) = center position
            Circle(x, y, radius, false, Color.aquamarine) |> draw