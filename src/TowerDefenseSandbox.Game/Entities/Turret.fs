namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine

type Turret (spriteBatch : SpriteBatch, center : Vector2, entityProvider : IEntityProvider) =

    let viewRadius = 100.0f
    let radius = 25.0f
    let mutable reload = 0

    interface IEntity with

        member val Position = center with get, set

        member this.Radius = radius
    
        member this.Update () =

            let target =
                entityProvider.GetEntities()
                |> List.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> List.filter (fun x -> (Mathx.distance center x.Position) - x.Radius < viewRadius)
                |> List.tryHead

            match target with 
            | None -> ()
            | Some x when reload > 10 ->
                Bullet(spriteBatch, center, entityProvider, x :?> Enemy) |> entityProvider.RegisterEntity
                reload <- 0
            | _ -> ()

            reload <- reload + 1

        member this.Draw () =
            spriteBatch.DrawCircle(center, radius, 100, Color.Blue, radius)
            spriteBatch.DrawCircle(center, viewRadius, 100, Color.GreenYellow)