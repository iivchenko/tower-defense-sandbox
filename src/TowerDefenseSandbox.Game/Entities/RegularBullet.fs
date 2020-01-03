﻿namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type RegularBullet (draw: Shape -> unit, center: Vector, entityProvider: IEntityProvider, target: Enemy) =

    let speed = 5.0f
    let radius = 2.5f

    let mutable center = center

    interface IEntity with
        
        member _.Radius = radius

        member _.Position
            with get () = center
            and set (value) = center <- value

        member this.Update (time: float32<second>) =
            let entity = target :> IEntity
            let (Vector(x1, y1)) = center
            let (Vector(x2, y2)) = entity.Position
            let tx = x2 - x1
            let ty = y2 - y1
            let dist = Vector.distance entity.Position center
  
            let velX = (tx/dist)*speed
            let velY = (ty/dist)*speed
            center <- Vector.init (x1 + velX) (y1 + velY)

            if (Vector.distance center entity.Position) <= radius 
                then
                    target.ApplyEffect (DamageEffect 15)
                    entityProvider.RemoveEntity this
                else 
                    ()

        member _.Draw (time: float32<second>) =
            let (Vector(x, y)) = center
            Circle(x, y, radius, true, Color.black) |> draw