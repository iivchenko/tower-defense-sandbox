namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Enemy (life: int, draw: Shape -> unit, center: Vector, entityProvider: IEntityProvider, path: Vector list) =

    let mutable life = life
    let mutable center = center
    let mutable path = path
    let mutable effects: Effect list = []
    let mutable orientation = 0.0f

    let radius = 10.0f
    let speed = 1.0f

    let limit = max 0
    let coordinates = [|
                        Vector.init  00.0f   radius;
                        Vector.init -radius -radius;
                        Vector.init  radius -radius;
                      |]

    member _.Radius = radius

    member _.Position
        with get () = center

    interface IEntity with

        member this.Update (delta: float32<second>) =

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
                    | SlowDownEffect (period, koefficient) -> SlowDownEffect(period - delta, koefficient)
                    | e -> e)

            match path with 
            | target::tail when Vector.distance center target < radius -> 
                path <- tail
            | h::_ ->
                let direction = Vector.direction center h
                let (x, y) = Vector.unwrap direction

                orientation <- atan2 -x y
                center <- center + currentSpeed * direction
            | _ -> ()

        member _.Draw (_: float32<second>) =
            
            let (Vector(x, y)) = center

            let transform = Matrix.rotation orientation

            let a1 = coordinates.[0] * transform
            let a2 = coordinates.[1] * transform
            let a3 = coordinates.[2] * transform

            Triangle (x, y, a1, a2, a3, Color.red) |> draw

    member _.Effects with get () = effects

    member this.ApplyEffect (effect: Effect) =

        effects <- effect::effects