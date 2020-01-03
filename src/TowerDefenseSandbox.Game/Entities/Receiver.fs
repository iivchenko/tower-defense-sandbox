namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open MonoGame.Extended

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Receiver (draw: Shape -> unit, entityProvider: IEntityProvider, zindex: int) = 
    
    let mutable life = 1.0f

    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)

    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (_: float32<second>) (position: RectangleF) =
            let c = center position
            let radius = position.Width / 2.0f * life
            let enemy =
                entityProvider.GetEntities()
                |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> Seq.filter (fun x -> (Vector.distance c x.Position) - x.Radius < radius)
                |> Seq.tryHead

            match enemy with 
            | None -> ()
            | Some e ->
                life <- life - 0.1f
                entityProvider.RemoveEntity e

            if life <= 0.0f then raise (System.Exception("Game Over")) else ()
            
        member _.Draw (_: float32<second>) (position: RectangleF) =
            let radius = position.Width / 2.0f * life
            let (Vector(x, y)) = center position

            Circle(x, y, radius, false, Color.coral) |> draw