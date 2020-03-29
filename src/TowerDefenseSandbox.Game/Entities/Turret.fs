namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine

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

type Bullet(draw: Vector<pixel> -> Shape, entityProvider: IEntityProvider, center: Vector<pixel>, speed: float32<pixel/second>, getTargetPosition: unit -> Vector<pixel>, apply: Bullet -> unit) =

    let radius = 2.5f<pixel>
    let mutable center = center

    member _.Position
        with get () = center

    interface IEntity with
    
        member this.Update (delta: float32<second>) =
            let target = getTargetPosition()
            let velocity = Behavior.seek center target speed
            
            center <- center + velocity * delta

            if (Vector.distance center target) < radius
                then
                    apply(this)
                    entityProvider.RemoveEntity this
                else 
                    ()

        member _.Draw() = draw center

type TurretCreatedMessage(pixels: int) =
    
    member _.Pixels = pixels

type TurretInfo =
    {Position: Vector<pixel>
     Color: Color
     ViewRadius: float32<pixel>
     Reload: float32<second>
     Pixels: int}

type Turret (
            info: TurretInfo,
            select: Enemy seq -> Enemy option, 
            fire: Vector<pixel> -> Enemy -> unit,
            pushMessge: TurretCreatedMessage -> unit,
            entityProvider: IEntityProvider) =

    let radius = 25.0f<pixel>
    let position = info.Position
    let (Vector(x, y)) = info.Position
    let body = Circle(x, y, radius, true, info.Color)
    let pixels = info.Pixels
    let viewRadius = info.ViewRadius
    let reload = info.Reload

    let view = Circle(x, y, viewRadius, false, Color.aquamarine)

    let mutable nextReload = reload

    do 
        pushMessge (TurretCreatedMessage pixels)

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

    static member private Create(info: TurretInfo, select: Enemy seq -> Enemy option, fire: Vector<pixel> -> Enemy -> unit, pushMessage: TurretCreatedMessage -> unit, entityProvider: IEntityProvider) =  
        Turret (info, select, fire, pushMessage, entityProvider)

    static member CreateRegular(position: Vector<pixel>, pushMessage: TurretCreatedMessage -> unit, entityProvider: IEntityProvider) =
        let select enemies = enemies |> Seq.tryHead

        let fire position (enemy: Enemy) = 
            let draw (Vector(x, y)) = Circle(x, y, 2.5f<pixel>, false, Color.black)
            Bullet(draw, entityProvider, position, 250.0f<pixel/second>, (fun _ -> enemy.Position), (fun _ -> DamageEffect 15 |> enemy.ApplyEffect)) |> entityProvider.RegisterEntity
        
        let info = {Position = position; Color = Color.black; ViewRadius = 100.0f<pixel>; Reload = 0.2f<second>; Pixels = 75}
        Turret.Create (info, select, fire, pushMessage, entityProvider)

    static member CreateSlow(position: Vector<pixel>, pushMessage: TurretCreatedMessage -> unit, entityProvider: IEntityProvider) =
        let select (enemies: Enemy seq) =
            enemies
            |> Seq.sortBy (fun x -> x.Effects)
            |> Seq.tryHead

        let fire position (enemy: Enemy)= 
            let draw (Vector(x, y)) = Rectangle(x - 7.0f<pixel>, y - 7.0f<pixel>, 7.0f<pixel>, 7.0f<pixel>, true, Color.blue)
            Bullet(draw, entityProvider, position, 150.0f<pixel/second>, (fun _ -> enemy.Position), (fun _ -> SlowDownEffect (5.0f<second>, 0.5f) |> enemy.ApplyEffect)) |> entityProvider.RegisterEntity

        let info = {Position = position; Color = Color.blue; ViewRadius = 100.0f<pixel>; Reload = 0.7f<second>; Pixels = 100}
        Turret.Create (info, select, fire, pushMessage, entityProvider)

    static member CreateSplash(position: Vector<pixel>, pushMessage: TurretCreatedMessage -> unit, entityProvider: IEntityProvider) =
        let select enemies = enemies |> Seq.tryHead

        let fire position (enemy: Enemy) = 
            let target = enemy.Position
            let drawBullet (Vector(x, y)) = Circle(x, y, 2.5f<pixel>, false, Color.red)
            Bullet(drawBullet, entityProvider, position, 250.0f<pixel/second>, (fun _ -> target), (fun bullet -> Boom(bullet.Position, 100.0f<pixel>, entityProvider) |> entityProvider.RegisterEntity)) |> entityProvider.RegisterEntity

        let info = {Position = position; Color = Color.red; ViewRadius = 100.0f<pixel>; Reload = 1.0f<second>; Pixels = 120}
        Turret.Create (info, select, fire, pushMessage, entityProvider)