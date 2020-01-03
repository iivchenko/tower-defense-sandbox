namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open MonoGame.Extended

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type RegularTurret (zindex: int, draw: Shape -> unit, entityProvider: IEntityProvider) =

    let viewRadius = 100.0f
    let radius = 25.0f
    let mutable reload = 0

    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)

    interface ICell with

        member _.ZIndex = zindex

        member this.Update (time: float32<second>) (position: RectangleF) =
    
            let c = center position
            let target =
                entityProvider.GetEntities()
                |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> Seq.filter (fun x -> (Vector.distance c x.Position) - x.Radius < viewRadius)
                |> Seq.tryHead

            match target with 
            | None -> ()
            | Some x when reload > 8 ->
                RegularBullet(draw, c, entityProvider, x :?> Enemy) |> entityProvider.RegisterEntity
                reload <- 0
            | _ -> ()

            reload <- reload + 1

        member this.Draw (time: float32<second>) (position: RectangleF) =
            let (Vector(x, y)) = center position
            Circle(x, y, radius, true, Color.black) |> draw
            #if DEBUG
            Circle(x, y, viewRadius, false, Color.aquamarine) |> draw
            #endif