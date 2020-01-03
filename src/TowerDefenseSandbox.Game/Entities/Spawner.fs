namespace TowerDefenseSandbox.Game.Entities

open Microsoft.Xna.Framework
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
    let spawnTime = 1.0f

    let radius = 15.0f
    let mutable time = spawnTime

    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)

    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (gameTime: GameTime) (position: RectangleF) =
            if time < 0.0f
                then 
                    factory.Create (center position)
                    time <- spawnTime
                else
                    time <- time - gameTime.GetElapsedSeconds()
            
        member _.Draw (gameTime: GameTime) (position: RectangleF) =
            let (Vector(x, y)) = center position
            Circle(x, y, radius, false, Color.aquamarine) |> draw