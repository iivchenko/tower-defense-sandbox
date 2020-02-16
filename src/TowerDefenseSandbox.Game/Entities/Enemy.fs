namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type EnemyInfo =
    {Life: int
     Center: Vector
     Speed: float32
     Path: Vector list
     Pixels: int}

type IEnemyMessage = interface end

type EnemyCreatedMessage (enemy: Enemy) =
    
    member _.Enemy = enemy

    interface IEnemyMessage

and EnemyKilledMessage (pixels: int, enemy: Enemy) =
    
    member _.Pixels = pixels
    member _.Enemy = enemy

    interface IEnemyMessage

and EnemyMessage = 
    | EnemyCreatedMessage of EnemyCreatedMessage
    | EnemyKilledMessage of EnemyKilledMessage

and Enemy (info: EnemyInfo, createBody: Vector -> float32 -> float32 -> Shape, draw: Shape -> unit, pushMessage: EnemyMessage -> unit) as this =

    let mutable life = info.Life
    let mutable center = info.Center
    let mutable path = info.Path
    let mutable effects: Effect list = []
    let mutable orientation = 0.0f

    let radius = 10.0f
    let speed = info.Speed

    let pixels = info.Pixels

    let limit = max 0

    do
        pushMessage (EnemyCreatedMessage(new EnemyCreatedMessage(this)))

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
                    if life = 0 then (pushMessage (EnemyKilledMessage(new EnemyKilledMessage (pixels, this)))) else ()
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

            createBody center orientation radius |> draw

    member _.Effects with get () = effects

    member _.ApplyEffect (effect: Effect) =

        effects <- effect::effects

    static member CreateRegular(position: Vector, path: Vector list, draw: Shape -> unit, pushMessage: EnemyMessage -> unit) =

        let createBody (Vector(x, y)) orientation radius =
            let transform = Matrix.rotation orientation

            let a1 = (Vector.init  00.0f   radius) * transform
            let a2 = (Vector.init -radius -radius) * transform
            let a3 = (Vector.init  radius -radius) * transform

            Triangle (x, y, a1, a2, a3, Color.red)

        let info = { Life = 200; Speed = 1.0f; Center = position; Path = path; Pixels = 10 }

        Enemy(info, createBody, draw, pushMessage)

    static member CreateFast(position: Vector, path: Vector list, draw: Shape -> unit, pushMessage: EnemyMessage -> unit) =

        let createBody (Vector(x, y)) orientation radius =

            Circle (x, y, radius, false, Color.red)

        let info = { Life = 100; Speed = 2.0f; Center = position; Path = path; Pixels = 5 }

        Enemy(info, createBody, draw, pushMessage)

    static member CreateHard(position: Vector, path: Vector list, draw: Shape -> unit, pushMessage: EnemyMessage -> unit) =
        
        let createBody (Vector(x, y)) orientation radius =
            let transform = Matrix.rotation orientation

            let a1 = (Vector.init  00.0f         -radius) * transform
            let a2 = (Vector.init  (radius/2.0f)  0.0f)   * transform
            let a3 = (Vector.init  00.0f          radius) * transform
            let a4 = (Vector.init -(radius/2.0f)  0.0f)   * transform

            Polygon (x, y, a1::a2::a3::a4::[], Color.red)

        let info = { Life = 1000; Speed = 0.5f; Center = position; Path = path; Pixels = 15 }

        Enemy(info,createBody,  draw, pushMessage)