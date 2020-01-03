namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Enemy (life: int, draw: Shape -> unit, center: Vector, entityProvider: IEntityProvider, path: Vector list) =

    let mutable life = life
    let mutable center = center
    let mutable path = path
    let mutable effects: Effect list = []

    let radius = 10.0f
    let speed = 1.0f

    let limit = max 0
    let direction v1 v2 = v2 - v1 |> Vector.normalize

    interface IEntity with

        member _.Position
            with get () = center
            and set (value) = center <- value

        member _.Radius = radius

        member this.Update (time: float32<second>) =

            let mutable currentSpeed = speed
            effects 
            |> List.iter (fun effect -> 
                match effect with
                | DamageEffect amount -> 
                    life <- life - amount |> limit
                    if life = 0 then entityProvider.RemoveEntity this else ()
                | SlowDownEffect (_, koefficient) -> currentSpeed <- currentSpeed * (1.0f - koefficient))
                
            effects <- 
                effects 
                |> List.filter (fun effect -> 
                           match effect with
                           | DamageEffect _ -> false
                           | SlowDownEffect (period, _) -> period > 0.0f<second>)
                |> List.map (fun effect -> 
                    match effect with
                    | SlowDownEffect (period, koefficient) -> SlowDownEffect(period - time, koefficient)
                    | e -> e)

            match path with 
            | h::tail when Vector.distance center h < radius -> path <- tail
            | h::_ ->
                center <- center + currentSpeed * (direction h center)
            | _ -> ()

        member _.Draw (time: float32<second>) =
            let (Vector(x, y)) = center
            Circle (x, y, radius, true, Color.red) |> draw

    member this.ApplyEffect (effect: TowerDefenseSandbox.Game.Entities.Effect) =

        effects <- effect::effects