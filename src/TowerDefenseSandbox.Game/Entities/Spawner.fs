namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Game.Engine
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended

type Spawner (spriteBatch : SpriteBatch, entityProvider : IEntityProvider) =

    [<Literal>] 
    let spawnTime = 1.0f

    let radius = 15.0f
    let mutable time = spawnTime

    interface ICell with 
            
            member _.Update (gameTime : GameTime) (position : Vector2) =
                if time < 0.0f
                    then 
                        new Enemy(100, spriteBatch, position, entityProvider) |> entityProvider.RegisterEntity
                        time <- spawnTime 
                    else
                        time <- time - gameTime.GetElapsedSeconds()
            
            member _.Draw (gameTime : GameTime) (position : Vector2) =
                spriteBatch.DrawCircle(position, radius, 100, Color.Aquamarine)