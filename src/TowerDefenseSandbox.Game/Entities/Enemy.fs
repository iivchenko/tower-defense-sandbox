namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine
open System

type Enemy (life: int, spriteBatch: SpriteBatch, center: Vector2, entityProvider: IEntityProvider, path: Vector2 list) =

    let mutable life = life
    let mutable center = center
    let mutable path = path
    let radius = 10.0f
    let mutable effects: TowerDefenseSandbox.Game.Entities.Effect list = []
    let speed = 1.0f

    let limit = Mathx.max 0

    let direction (v1: Vector2) (v2: Vector2) = 
        let direction = (v1 - v2)
        direction.Normalize()

        direction

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
            | h::tail when Mathx.distance center h < radius -> path <- tail
            | h::_ ->
                center <- center + currentSpeed * (direction h center)
            | _ -> ()

        member _.Draw (gameTime: GameTime) =
            spriteBatch.DrawCircle(center, radius, 100, Color.Red, radius)

    member this.ApplyEffect (effect: TowerDefenseSandbox.Game.Entities.Effect) =

        effects <- effect::effects