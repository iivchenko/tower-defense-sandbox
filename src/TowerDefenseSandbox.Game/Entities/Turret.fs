namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine

type Turret (spriteBatch : SpriteBatch, entityProvider : IEntityProvider) =

    let viewRadius = 100.0f
    let radius = 25.0f
    let mutable reload = 0

    interface ICell with

        member this.Update (gameTime : GameTime) (position : Vector2) =
    
            let target =
                entityProvider.GetEntities()
                |> List.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> List.filter (fun x -> (Mathx.distance position x.Position) - x.Radius < viewRadius)
                |> List.tryHead

            match target with 
            | None -> ()
            | Some x when reload > 8 ->
                Bullet(spriteBatch, position, entityProvider, x :?> Enemy) |> entityProvider.RegisterEntity
                reload <- 0
            | _ -> ()

            reload <- reload + 1

        member this.Draw (gameTime : GameTime) (position : Vector2) =
            spriteBatch.DrawCircle(position, radius, 100, Color.Blue, radius)
            spriteBatch.DrawCircle(position, viewRadius, 100, Color.GreenYellow)