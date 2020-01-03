namespace TowerDefenseSandbox.Game.Entities

open Microsoft.Xna.Framework

open System

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type SlowBullet (draw: Shape -> unit, center: Vector, entityProvider: IEntityProvider, target: Enemy) =

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
            let (Vector(x1, y1)) = center
            let (Vector(x2, y2)) = entity.Position
            let tx = x2 - x1
            let ty = y2 - y1
            let dist = Vector.distance entity.Position center
  
            let velX = (tx/dist)*speed
            let velY = (ty/dist)*speed
            center <- Vector.init (x1 + velX) (y1 + velY)

            if (Vector.distance center entity.Position) < radius
                then
                    SlowDownEffect (TimeSpan (0, 0, 5), 0.5f) |> target.ApplyEffect
                    entityProvider.RemoveEntity this
                else 
                    ()

        member _.Draw (gameTime: GameTime) =
             let (Vector(x, y)) = center
             Rectangle(x - radius, y - radius, radius, radius, true, Color.blue) |> draw