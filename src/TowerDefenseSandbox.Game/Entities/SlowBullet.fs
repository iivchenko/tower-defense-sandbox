namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Game.Engine
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended
open System

type SlowBullet (spriteBatch: SpriteBatch, center: Vector2, entityProvider: IEntityProvider, target: Enemy) =

    let speed = 2.0f
    let radius = 7.0f

    let mutable center = center

    interface IEntity with
        
        member _.Radius = radius

        member _.Position
            with get () = center
            and set (value) = center <- value

        member this.Update (gameTime: GameTime) =
            let entity = target :> IEntity
            let tx = entity.Position.X - center.X
            let ty = entity.Position.Y - center.Y
            let dist = Mathx.distance entity.Position center
  
            let velX = (tx/dist)*speed
            let velY = (ty/dist)*speed
            center <- Vector2(center.X + velX, center.Y + velY)

            if (Mathx.distance center entity.Position) < radius
                then
                    SlowDownEffect (TimeSpan (0, 0, 5), 0.5f) |> target.ApplyEffect
                    entityProvider.RemoveEntity this
                else 
                    ()

        member _.Draw (gameTime: GameTime) =
             spriteBatch.FillRectangle(center.X - radius, center.Y - radius, radius, radius, Color.Blue)