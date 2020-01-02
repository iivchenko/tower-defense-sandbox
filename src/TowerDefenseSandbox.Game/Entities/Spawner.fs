namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended

type EnemyFactory (spriteBatch: SpriteBatch, entityProvider: IEntityProvider) = 
    
    let mutable _path = []

    member public this.Create (center: Vector) =
        Enemy(100, spriteBatch, center, entityProvider, _path) |> entityProvider.RegisterEntity

    member public this.UpdatePath (path: Vector list) =
        _path <- path

type Spawner (zindex: int, spriteBatch: SpriteBatch, factory: EnemyFactory) =

    [<Literal>] 
    let spawnTime = 1.0f

    let radius = 15.0f
    let mutable time = spawnTime

    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)
    let toVector2 (Vector(x, y)) = Vector2 (x, y)

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
            spriteBatch.DrawCircle(position |> center |> toVector2, radius, 100, Color.Aquamarine)