namespace TowerDefenseSandbox.Game.Entities

open Microsoft.Xna.Framework

open System

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type SplashBullet (draw: Shape -> unit, center: Vector, entityProvider: IEntityProvider, target: Vector) =

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
            let (Vector (x1, y1)) = center
            let (Vector (x2, y2)) = target
            let tx = x2 - x1
            let ty = y2 - y1
            let dist = Vector.distance target center
  
            let velX = (tx/dist)*speed
            let velY = (ty/dist)*speed
            center <- Vector.init (x1 + velX) (y1 + velY)

            if isBoom then
                ttl <- ttl.Subtract(gameTime.ElapsedGameTime)

                if (ttl.TotalMilliseconds <= 0.0) then entityProvider.RemoveEntity this else ()
            else 
                if (Vector.distance center target) < radius
                    then

                        entityProvider.GetEntities()
                            |> Seq.filter (fun entity -> Vector.distance center entity.Position < boomRadius)
                            |> Seq.iter (fun entity -> 
                                match entity with 
                                | :? Enemy as enemy -> DamageEffect 30 |> enemy.ApplyEffect 
                                | _ -> ())

                        isBoom <- true
                    else 
                        ()

        member _.Draw (gameTime: GameTime) =
            let (Vector(x, y)) = center

            match isBoom with 
            | false -> Circle(x, y, radius, true, Color.red) |> draw
            | true ->  Circle(x, y, boomRadius, true, Color(byte 255, byte 0, byte 0, byte 50)) |> draw