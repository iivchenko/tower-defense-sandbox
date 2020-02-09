namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Receiver (position: Vector, draw: Shape -> unit, entityProvider: IEntityProvider) = 

    let mutable life = 10
    let factor = life

    let radius = 25.0f

    member _.Life with get () = life

    interface IEntity with

        member _.Radius = radius

        member _.Position 
            with get () = position
            and set(value: Vector) = ()
            
        member _.Update (_: float32<second>) =

            let radius = radius / 2.0f * ((float32 life)/(float32 factor))
            let enemy =
                entityProvider.GetEntities()
                |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> Seq.filter (fun x -> (Vector.distance position x.Position) - x.Radius < radius)
                |> Seq.tryHead

            match enemy with 
            | None -> ()
            | Some e ->
                life <- life - 1
                entityProvider.RemoveEntity e
            
        member _.Draw (_: float32<second>) =

            let (Vector(x, y)) = position
            let radius = radius * ((float32 life)/(float32 factor))

            Circle(x, y, radius, false, Color.coral) |> draw