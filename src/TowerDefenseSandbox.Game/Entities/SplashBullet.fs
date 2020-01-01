namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Game.Engine
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended
open System

type SplashBullet (spriteBatch: SpriteBatch, center: Vector2, entityProvider: IEntityProvider, target: Vector2) =

    let speed = 2.0f
    let radius = 5.0f
    let boomRadius = 15.0f * radius
    let mutable isBoom = false
    let mutable ttl = TimeSpan (0, 0, 0, 0, 300)
    let mutable center = center

    interface IEntity with
        
        member _.Radius = radius

        member _.Position
            with get () = center
            and set (value) = center <- value

        member this.Update (gameTime: GameTime) =
            let tx = target.X - center.X
            let ty = target.Y - center.Y
            let dist = Mathx.distance target center
  
            let velX = (tx/dist)*speed
            let velY = (ty/dist)*speed
            center <- Vector2(center.X + velX, center.Y + velY)

            if isBoom then
                ttl <- ttl.Subtract(gameTime.ElapsedGameTime)

                if (ttl.TotalMilliseconds <= 0.0) then entityProvider.RemoveEntity this else ()
            else 
                if (Mathx.distance center target) < radius
                    then

                        entityProvider.GetEntities()
                            |> Seq.filter (fun entity -> Mathx.distance center entity.Position < boomRadius)
                            |> Seq.iter (fun entity -> 
                                match entity with 
                                | :? Enemy as enemy -> DamageEffect 30 |> enemy.ApplyEffect 
                                | _ -> ())

                        isBoom <- true
                    else 
                        ()

        member _.Draw (gameTime: GameTime) =
             match isBoom with 
             | false -> spriteBatch.DrawCircle(center, radius, 100, Color.Red, radius)
             | true ->  spriteBatch.DrawCircle(center, radius, 100, Color.Red, boomRadius)