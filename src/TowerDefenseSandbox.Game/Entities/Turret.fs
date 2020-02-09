﻿namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

[<Measure>] type pixel

type Boom (draw: Shape -> unit, center: Vector, radius: float32, entityProvider: IEntityProvider) =
    
    let k = radius / 3.0f
    let (Vector(x, y)) = center

    let mutable ttl = 0.3f<second>
    let mutable radius = 0.0f

    interface IEntity with

        member this.Update (delta: float32<second>) =

            if (ttl <= 0.0f<second>) 
                then entityProvider.RemoveEntity this
                else 
                    entityProvider.GetEntities()
                        |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                        |> Seq.cast<Enemy>
                        |> Seq.filter (fun entity -> Vector.distance center entity.Position < radius)
                        |> Seq.iter (fun enemy -> DamageEffect 3 |> enemy.ApplyEffect)

            ttl <- ttl - delta
            radius <- radius + (k / 0.1f<second>) * delta

        member _.Draw (time: float32<second>) =

            Circle(x, y, radius, true, Color(byte 255, byte 0, byte 0, byte 50)) |> draw

type Bullet(draw: Vector -> unit, entityProvider: IEntityProvider, center: Vector, speed: float32<pixel/second>, getTargetPosition: unit -> Vector, apply: Bullet -> unit) =

    let radius = 2.5f
    let mutable center = center

    member _.Position
        with get () = center

    interface IEntity with
    
        member this.Update (delta: float32<second>) =
            let v = getTargetPosition()
            let velocity = (Vector.direction center v) * float32 (speed * delta)
            
            center <- center + velocity

            if (Vector.distance center v) < radius
                then
                    apply(this)
                    entityProvider.RemoveEntity this
                else 
                    ()

        member _.Draw (_: float32<second>) =
            draw center

type Turret (
            center: Vector,
            color: Color, 
            viewRadius: float32,
            reload: float32<second>, 
            select: Enemy seq -> Enemy option, 
            fire: Vector -> Enemy -> unit,
            draw: Shape -> unit,
            entityProvider: IEntityProvider) =

    let radius = 25.0f
    let (Vector(x, y)) = center
    let body = Circle(x, y, radius, true, color)

    #if DEBUG
    let view = Circle(x, y, viewRadius, false, Color.aquamarine)
    #endif

    let mutable nextReload = reload

    interface IEntity with

        member _.Update(time: float32<second>) = 

            let enemies = 
                entityProvider.GetEntities()
                    |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                    |> Seq.cast<Enemy>
                    |> Seq.filter (fun x -> (Vector.distance center x.Position) - x.Radius < viewRadius)

            match select enemies with 
            | None -> ()
            | Some enemy when nextReload < 0.0f<second> ->
                fire center enemy
                nextReload <- reload
            | _ -> ()

            nextReload <- nextReload - time

        member _.Draw(_: float32<second>) =

            draw body

            #if DEBUG
            draw view
            #endif

    static member private Create(center: Vector, color: Color, viewRadius: float32, reload: float32<second>, select: Enemy seq -> Enemy option, fire: Vector -> Enemy -> unit, draw: Shape -> unit, entityProvider: IEntityProvider) =  
        Turret (center, color, viewRadius, reload, select, fire, draw, entityProvider)

    static member CreateRegular(center: Vector, draw: Shape -> unit, entityProvider: IEntityProvider) =
        let select enemies = enemies |> Seq.tryHead

        let fire position (enemy: Enemy) = 
            let draw (Vector(x, y)) = Circle(x, y, 2.5f, false, Color.black) |> draw
            Bullet(draw, entityProvider, position, 250.0f<pixel/second>, (fun _ -> enemy.Position), (fun _ -> DamageEffect 15 |> enemy.ApplyEffect)) |> entityProvider.RegisterEntity

        Turret.Create (center, Color.black, 100.0f, 0.2f<second>, select, fire, draw, entityProvider)

    static member CreateSlow(center: Vector, draw: Shape -> unit, entityProvider: IEntityProvider) =
        let select (enemies: Enemy seq) =
            enemies
            |> Seq.sortBy (fun x -> x.Effects)
            |> Seq.tryHead

        let fire position (enemy: Enemy)= 
            let draw (Vector(x, y)) = Rectangle(x - 7.0f, y - 7.0f, 7.0f, 7.0f, true, Color.blue) |> draw
            Bullet(draw, entityProvider, position, 100.0f<pixel/second>, (fun _ -> enemy.Position), (fun _ -> SlowDownEffect (5.0f<second>, 0.5f) |> enemy.ApplyEffect)) |> entityProvider.RegisterEntity

        Turret.Create (center, Color.blue, 100.0f, 0.7f<second>, select, fire, draw, entityProvider)

    static member CreateSplash(center: Vector, draw: Shape -> unit, entityProvider: IEntityProvider) =
        let select enemies = enemies |> Seq.tryHead

        let fire position (enemy: Enemy) = 
            let target = enemy.Position
            let drawBullet (Vector(x, y)) = Circle(x, y, 2.5f, false, Color.red) |> draw
            Bullet(drawBullet, entityProvider, position, 250.0f<pixel/second>, (fun _ -> target), (fun bullet -> Boom(draw, bullet.Position, 100.0f, entityProvider) |> entityProvider.RegisterEntity)) |> entityProvider.RegisterEntity

        Turret.Create (center, Color.red, 100.0f, 1.0f<second>, select, fire, draw, entityProvider)