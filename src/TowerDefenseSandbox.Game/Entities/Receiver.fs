﻿namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open MonoGame.Extended

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Receiver (draw: Shape -> unit, entityProvider: IEntityProvider, zindex: int) = 

    let mutable life = 10
    let factor = life
    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)

    member _.Life with get () = life

    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (_: float32<second>) (position: RectangleF) =

            let c = center position
            let radius = position.Width / 2.0f * ((float32 life)/(float32 factor))
            let enemy =
                entityProvider.GetEntities()
                |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> Seq.filter (fun x -> (Vector.distance c x.Position) - x.Radius < radius)
                |> Seq.tryHead

            match enemy with 
            | None -> ()
            | Some e ->
                life <- life - 1
                entityProvider.RemoveEntity e
            
        member _.Draw (_: float32<second>) (position: RectangleF) =
            let radius = position.Width / 2.0f * ((float32 life)/(float32 factor))
            let (Vector(x, y)) = center position

            Circle(x, y, radius, false, Color.coral) |> draw