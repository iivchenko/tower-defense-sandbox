﻿namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame
open Fame.Graphics
open TowerDefenseSandbox.Game

type Boom (center: Vector<pixel>, radius: float32<pixel>, entityProvider: IEntityProvider) =
    
    let k = radius / 3.0f
    let (Vector(x, y)) = center

    let mutable ttl = 0.3f<second>
    let mutable radius = 0.0f<pixel>

    interface IEntity with

        member this.Update (delta: float32<second>) =

            if (ttl <= 0.0f<second>) 
                then entityProvider.RemoveEntity this
                else 
                    entityProvider.GetEntities()
                        |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                        |> Seq.cast<Enemy>
                        |> Seq.filter (fun entity -> Vector.distance center entity.Position < radius)
                        |> Seq.iter (fun enemy -> DamageEffect 5 |> enemy.ApplyEffect)

            ttl <- ttl - delta
            radius <- radius + (k / 0.1f<second>) * delta

        member _.Draw() = Circle(x, y, radius, true, Color(byte 255, byte 0, byte 0, byte 50))

type Bullet(draw: Vector<pixel> -> Shape, entityProvider: IEntityProvider, center: Vector<pixel>, speed: float32<pixel/second>, getTarget: unit -> Enemy, apply: Bullet -> unit) =

    let radius = 2.5f<pixel>
    let mutable center = center

    member _.Position
        with get () = center

    interface IEntity with
    
        member this.Update (delta: float32<second>) =
            let target = getTarget()
            let velocity = Behavior.seek center target.Position speed
            
            center <- center + velocity * delta

            if (Vector.distance center target.Position) < target.Radius
                then
                    apply(this)
                    entityProvider.RemoveEntity this
                else 
                    ()

        member _.Draw() = draw center

type TurretCreatedMessage(pixels: int) =
    
    member _.Pixels = pixels

type Turret (
            info: TurretInfo,
            position: Vector<pixel>,
            color: Color,
            select: Enemy seq -> Enemy option, 
            fire: Vector<pixel> -> Enemy -> unit,
            entityProvider: IEntityProvider) =

    let radius = 25.0f<pixel>
    let position = position
    let (Vector(x, y)) = position
    let body = Circle(x, y, radius, true, color)
    let viewRadius = info.ViewRadius
    let reload = info.Reload

    let view = Circle(x, y, viewRadius, false, Color.aquamarine)

    let mutable nextReload = reload

    interface IEntity with

        member _.Update(delta: float32<second>) = 

            let enemies = 
                entityProvider.GetEntities()
                    |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                    |> Seq.cast<Enemy>
                    |> Seq.filter (fun x -> (Vector.distance position x.Position) - x.Radius < viewRadius)

            match select enemies with 
            | None -> ()
            | Some enemy when nextReload < 0.0f<second> ->
                fire position enemy
                nextReload <- reload
            | _ -> ()

            nextReload <- nextReload - delta

        member _.Draw() = Shape(body::view::[])

    static member private Create(info: TurretInfo, position: Vector<pixel>, color: Color, select: Enemy seq -> Enemy option, fire: Vector<pixel> -> Enemy -> unit, entityProvider: IEntityProvider) =  
        Turret (info, position, color, select, fire, entityProvider)

    static member CreateRegular(position: Vector<pixel>, entityProvider: IEntityProvider) =
        let select enemies = enemies |> Seq.tryHead

        let fire position (enemy: Enemy) = 
            let draw (Vector(x, y)) = Circle(x, y, 2.5f<pixel>, false, Color.black)
            Bullet(draw, entityProvider, position, 400.0f<pixel/second>, (fun _ -> enemy), (fun _ -> DamageEffect 7 |> enemy.ApplyEffect)) |> entityProvider.RegisterEntity
        
        Turret.Create (GameBalance.regularTurretInfo, position, Color.black, select, fire, entityProvider)

    static member CreateSlow(position: Vector<pixel>, entityProvider: IEntityProvider) =
        let select (enemies: Enemy seq) =
            enemies
            |> Seq.sortBy (fun x -> x.Effects)
            |> Seq.filter (fun x -> x.Effects |> List.exists (fun y -> match y with | SlowDownEffect _ -> true | _ -> false) |> not)
            |> Seq.tryHead

        let fire position (enemy: Enemy)= 
            let draw (Vector(x, y)) = Rectangle(x - 7.0f<pixel>, y - 7.0f<pixel>, 7.0f<pixel>, 7.0f<pixel>, true, Color.blue)
            Bullet(draw, entityProvider, position, 300.0f<pixel/second>, (fun _ -> enemy), (fun _ -> SlowDownEffect (5.0f<second>, 0.5f) |> enemy.ApplyEffect)) |> entityProvider.RegisterEntity

        Turret.Create (GameBalance.slowTurretInfo, position, Color.blue, select, fire, entityProvider)

    static member CreateSplash(position: Vector<pixel>, entityProvider: IEntityProvider) =
        let select enemies = enemies |> Seq.tryHead

        let fire position (enemy: Enemy) = 
            let drawBullet (Vector(x, y)) = Circle(x, y, 2.5f<pixel>, false, Color.red)
            Bullet(drawBullet, entityProvider, position, 250.0f<pixel/second>, (fun _ -> enemy), (fun bullet -> Boom(bullet.Position, 100.0f<pixel>, entityProvider) |> entityProvider.RegisterEntity)) |> entityProvider.RegisterEntity

        Turret.Create (GameBalance.splashTurretInfo, position, Color.red, select, fire, entityProvider)