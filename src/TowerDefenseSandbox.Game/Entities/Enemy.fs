namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame
open Fame.Graphics
open TowerDefenseSandbox.Game

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

and Enemy (info: EnemyInfo, center: Vector<pixel>, path: Vector<pixel> list, wave: int, createBody: Vector<pixel> -> float32 -> float32<pixel> -> Shape, pushMessage: EnemyMessage -> unit) as this =

    let mutable life = info.Life + info.Life * wave / 100
    let mutable center = center
    let mutable path = path
    let mutable effects: Effect list = []
    let mutable orientation = 0.0f

    let radius = 10.0f<pixel>
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

                let velocity = Behavior.seek center h currentSpeed 
                center <- center + velocity * delta

                orientation <-  Behavior.face center h
            | _ -> ()

        member _.Draw() = createBody center orientation radius

    member _.Effects with get () = effects

    member _.ApplyEffect (effect: Effect) =

        effects <- effect::effects

    static member CreateRegular(position: Vector<pixel>, path: Vector<pixel> list, wave: int, pushMessage: EnemyMessage -> unit) =

        let createBody (Vector(x, y)) orientation radius =
            let transform = Matrix.rotation orientation

            let a1 = (Vector.init  00.0f<pixel>  radius) * transform
            let a2 = (Vector.init -radius       -radius) * transform
            let a3 = (Vector.init  radius       -radius) * transform

            Triangle (x, y, a1, a2, a3, Color.red)

        Enemy(GameBalance.regularEnemyInfo, position, path, wave, createBody, pushMessage)

    static member CreateFast(position: Vector<pixel>, path: Vector<pixel> list, wave: int, pushMessage: EnemyMessage -> unit) =

        let createBody (Vector(x, y): Vector<pixel>) orientation radius = Circle (x, y, radius, false, Color.red)

        Enemy(GameBalance.fastEnemyInfo, position, path, wave, createBody, pushMessage)

    static member CreateHard(position: Vector<pixel>, path: Vector<pixel> list, wave: int, pushMessage: EnemyMessage -> unit) =
        
        let createBody (Vector(x, y): Vector<pixel>) orientation radius =
            let transform = Matrix.rotation orientation

            let a1 = (Vector.init  00.0f<pixel>   -radius)     * transform
            let a2 = (Vector.init  (radius/2.0f)  0.0f<pixel>) * transform
            let a3 = (Vector.init  00.0f<pixel>   radius)      * transform
            let a4 = (Vector.init -(radius/2.0f)  0.0f<pixel>) * transform

            Polygon (x, y, a1::a2::a3::a4::[], Color.red)

        Enemy(GameBalance.hardEnemyInfo, position, path, wave, createBody, pushMessage)