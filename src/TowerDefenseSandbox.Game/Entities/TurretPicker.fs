﻿namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type TurretPicker (position: Vector, width: float32, height: float32, draw: Shape -> unit, parent: IEntity option [,], entityProvider: IEntityProvider, column: int, raw: int) =

    let center (column: int) (raw: int) = Vector.init ((float32 column) * width + width / 2.0f) ((float32 raw) * height + height / 2.0f)

    member this.Click (clickPosition: Vector) = 
        let (Vector(_, y)) = position
        let (Vector(_, clickY)) = clickPosition
        let d = clickY - y

        match raw with 
        | _ when d > 0.0f && d <= height / 3.0f -> 
            let turret = Turret.CreateRegular(center column raw, draw, entityProvider) :> IEntity
            parent.[column, raw] <- Some turret
            entityProvider.RegisterEntity turret
            entityProvider.RemoveEntity this
        | _ when d > height / 3.0f && d <= height / 3.0f * 2.0f -> 
            let turret = Turret.CreateSlow(center column raw, draw, entityProvider) :> IEntity
            parent.[column, raw] <- Some turret
            entityProvider.RegisterEntity turret
            entityProvider.RemoveEntity this
        | _ ->
            let turret = Turret.CreateSplash(center column raw, draw, entityProvider) :> IEntity
            parent.[column, raw] <- Some turret
            entityProvider.RegisterEntity turret
            entityProvider.RemoveEntity this

    interface IEntity with

        member _.Update(_: float32<second>) = 
            ()
           
        member this.Draw(_: float32<second>) =
               
            let (Vector(x, y)) = position
            let height = height / 3.0f

            Rectangle(x, y + 0.0f * height, width, height, true, Color.black) |> draw
            Rectangle(x, y + 1.0f * height, width, height, true, Color.blue) |> draw
            Rectangle(x, y + 2.0f * height, width, height, true, Color.red) |> draw