namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Game.Engine
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended

type Spawner (zindex : int, spriteBatch : SpriteBatch, entityProvider : IEntityProvider) =

    [<Literal>] 
    let spawnTime = 1.0f

    let radius = 15.0f
    let mutable time = spawnTime

    let center (position : RectangleF) = Vector2 (position.X + position.Width / 2.0f, position.Y + position.Height / 2.0f)

    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (gameTime : GameTime) (position : RectangleF) =
            if time < 0.0f
                then 
                    new Enemy(100, spriteBatch, center position, entityProvider) |> entityProvider.RegisterEntity
                    time <- spawnTime 
                else
                    time <- time - gameTime.GetElapsedSeconds()
            
        member _.Draw (gameTime : GameTime) (position : RectangleF) =
            spriteBatch.DrawCircle(center position, radius, 100, Color.Aquamarine)