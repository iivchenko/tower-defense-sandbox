namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame
open Fame.Graphics

type Receiver (position: Vector<pixel>, entityProvider: IEntityProvider, lifes: int) = 

    let (Vector(x, y)) = position

    let mutable life = lifes
    let maxLife = lifes

    let maxRadius = 25.0f<pixel>
    let mutable radius = maxRadius
    let mutable body = Circle(x, y, radius, false, Color.coral) 

    member _.Life with get () = life

    interface IEntity with
            
        member _.Update (_: float32<second>) =
        
            let enemy =
                entityProvider.GetEntities()
                |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> Seq.cast<Enemy>
                |> Seq.filter (fun x -> (Vector.distance position x.Position) - x.Radius < radius)
                |> Seq.tryHead

            match enemy with 
            | None -> ()
            | Some e ->
                life <- life - 1
                entityProvider.RemoveEntity e
                radius <- maxRadius * ((float32 life)/(float32 maxLife))
                body <- Circle(x, y, radius, false, Color.coral) 

        member _.Draw() = body