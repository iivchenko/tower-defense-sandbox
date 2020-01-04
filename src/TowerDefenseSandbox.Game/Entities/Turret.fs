namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open MonoGame.Extended

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

[<Measure>] type pixel

type Boom (draw: Shape -> unit, center: Vector, radius: float32, entityProvider: IEntityProvider) =
    
    let delta = radius / 3.0f

    let mutable center = center
    let mutable ttl = 0.3f<second>
    let mutable radius = 0.0f

    interface IEntity with
       
        member _.Radius = radius

        member _.Position
            with get () = center
            and set (value) = center <- value

        member this.Update (time: float32<second>) =

            if (ttl <= 0.0f<second>) 
                then entityProvider.RemoveEntity this
                else 
                    entityProvider.GetEntities()
                        |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                        |> Seq.filter (fun entity -> Vector.distance center entity.Position < radius)
                        |> Seq.cast<Enemy>
                        |> Seq.iter (fun enemy -> DamageEffect 3 |> enemy.ApplyEffect)

            ttl <- ttl - time
            radius <- radius + (delta / 0.1f<second>) * time

        member _.Draw (time: float32<second>) =
            let (Vector(x, y)) = center            
            Circle(x, y, radius, true, Color(byte 255, byte 0, byte 0, byte 50)) |> draw

type Bullet(draw: Vector -> unit, entityProvider: IEntityProvider, center: Vector, speed: float32<pixel/second>, getTargetPosition: unit -> Vector, apply: Bullet -> unit) =

    let radius = 2.5f
    let mutable center = center

    interface IEntity with
       
        member _.Radius = radius

        member _.Position
            with get () = center
            and set (value) = center <- value

        member this.Update (time: float32<second>) =
            let v = getTargetPosition()
            let velocity = (center - v |> Vector.normalize) * float32 (speed * time)
            
            center <- center + velocity

            if (Vector.distance center v) < radius
                then
                    apply(this)
                    entityProvider.RemoveEntity this
                else 
                    ()

        member _.Draw (time: float32<second>) =
            draw center

type Turret (
            zindex: int, 
            color: Color, 
            viewRadius: float32,
            reload: float32<second>, 
            select: Enemy seq -> Enemy option, 
            fire: Vector -> Enemy -> unit,
            draw: Shape -> unit,
            entityProvider: IEntityProvider) =

    let radius = 25.0f
    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)

    let mutable nextReload = reload

    interface ICell with

        member _.ZIndex = zindex

        member _.Update(time: float32<second>) (position: RectangleF) = 
            
            let c = center position
            let enemies = 
                entityProvider.GetEntities()
                    |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                    |> Seq.filter (fun x -> (Vector.distance c x.Position) - x.Radius < viewRadius)
                    |> Seq.cast<Enemy>

            match select enemies with 
            | None -> ()
            | Some enemy when nextReload < 0.0f<second> ->
                fire c enemy
                nextReload <- reload
            | _ -> ()

            nextReload <- nextReload - time

        member _.Draw(time: float32<second>) (position: RectangleF) =

            let (Vector(x, y)) = center position
            Circle(x, y, radius, true, color) |> draw

            #if DEBUG
            Circle(x, y, viewRadius, false, Color.aquamarine) |> draw
            #endif

    static member private Create(zindex: int, color: Color, viewRadius: float32, reload: float32<second>, select: Enemy seq -> Enemy option, fire: Vector -> Enemy -> unit, draw: Shape -> unit, entityProvider: IEntityProvider) =  
        Turret (zindex, color, viewRadius, reload, select, fire, draw, entityProvider)

    static member CreateRegular(draw: Shape -> unit, entityProvider: IEntityProvider) =
        let select enemies = enemies |> Seq.tryHead

        let fire position (enemy: Enemy) = 
            let draw (Vector(x, y)) = Circle(x, y, 2.5f, false, Color.black) |> draw
            Bullet(draw, entityProvider, position, 250.0f<pixel/second>, (fun _ -> (enemy :> IEntity).Position), (fun _ -> DamageEffect 15 |> enemy.ApplyEffect)) |> entityProvider.RegisterEntity

        Turret.Create (1, Color.black, 100.0f, 0.2f<second>, select, fire, draw, entityProvider)

    static member CreateSlow(draw: Shape -> unit, entityProvider: IEntityProvider) =
        let select (enemies: Enemy seq) =
            enemies
            |> Seq.sortBy (fun x -> x.Effects)
            |> Seq.tryHead

        let fire position (enemy: Enemy)= 
            let draw (Vector(x, y)) = Rectangle(x - 7.0f, y - 7.0f, 7.0f, 7.0f, true, Color.blue) |> draw
            Bullet(draw, entityProvider, position, 100.0f<pixel/second>, (fun _ -> (enemy :> IEntity).Position), (fun _ -> SlowDownEffect (5.0f<second>, 0.5f) |> enemy.ApplyEffect)) |> entityProvider.RegisterEntity

        Turret.Create (1, Color.blue, 100.0f, 0.7f<second>, select, fire, draw, entityProvider)

    static member CreateSplash(draw: Shape -> unit, entityProvider: IEntityProvider) =
        let select enemies = enemies |> Seq.tryHead

        let fire position (enemy: Enemy) = 
            let target = (enemy :> IEntity).Position
            let drawBullet (Vector(x, y)) = Circle(x, y, 2.5f, false, Color.red) |> draw
            Bullet(drawBullet, entityProvider, position, 250.0f<pixel/second>, (fun _ -> target), (fun bullet -> Boom(draw, (bullet :> IEntity).Position, 100.0f, entityProvider) |> entityProvider.RegisterEntity)) |> entityProvider.RegisterEntity

        Turret.Create (1, Color.red, 100.0f, 1.0f<second>, select, fire, draw, entityProvider)