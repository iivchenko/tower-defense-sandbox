namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended

type Enemy (life: int, spriteBatch: SpriteBatch, center: Vector, entityProvider: IEntityProvider, path: Vector list) =

    let mutable life = life
    let mutable center = center
    let mutable path = path
    let radius = 10.0f
    let mutable effects: TowerDefenseSandbox.Game.Entities.Effect list = []
    let speed = 1.0f

    let limit = max 0

    let direction (v1: Vector) (v2: Vector) = v2 - v1 |> Vector.normalize
    let toVector2 (Vector(x, y)) = Vector2 (x, y)

    interface IEntity with

        member _.Position
            with get () = center
            and set (value) = center <- value

        member _.Radius = radius

        member this.Update (gameTime: GameTime) =

            let mutable currentSpeed = speed
            effects 
            |> List.iter (fun effect -> 
                match effect with
                | DamageEffect amount -> 
                    life <- life - amount |> limit
                    if life = 0 then entityProvider.RemoveEntity this else ()
                | SlowDownEffect (_, koefficient) -> currentSpeed <- currentSpeed * ( 1.0f - koefficient))
                
            effects <- 
                effects 
                |> List.filter (fun effect -> 
                           match effect with
                           | DamageEffect _ -> false
                           | SlowDownEffect (period, _) -> period.TotalMilliseconds > 0.0)
                |> List.map (fun effect -> 
                    match effect with
                    | SlowDownEffect (period, koefficient) -> SlowDownEffect(period.Subtract(gameTime.ElapsedGameTime), koefficient)
                    | e -> e)

            match path with 
            | h::tail when Vector.distance center h < radius -> path <- tail
            | h::_ ->
                center <- center + currentSpeed * (direction h center)
            | _ -> ()

        member _.Draw (gameTime: GameTime) =
            spriteBatch.DrawCircle(toVector2 center, radius, 100, Color.Red, radius)

    member this.ApplyEffect (effect: TowerDefenseSandbox.Game.Entities.Effect) =

        effects <- effect::effects