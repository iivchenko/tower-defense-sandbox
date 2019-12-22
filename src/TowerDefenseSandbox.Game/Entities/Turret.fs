namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine

type Turret (zindex : int, spriteBatch : SpriteBatch, entityProvider : IEntityProvider) =

    let viewRadius = 100.0f
    let radius = 25.0f
    let mutable reload = 0

    let center (position : RectangleF) = Vector2 (position.X + position.Width / 2.0f, position.Y + position.Height / 2.0f)

    interface ICell with

        member _.ZIndex = zindex

        member this.Update (gameTime : GameTime) (position : RectangleF) =
    
            let c = center position
            let target =
                entityProvider.GetEntities()
                |> List.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> List.filter (fun x -> (Mathx.distance c x.Position) - x.Radius < viewRadius)
                |> List.tryHead

            match target with 
            | None -> ()
            | Some x when reload > 8 ->
                Bullet(spriteBatch, c, entityProvider, x :?> Enemy) |> entityProvider.RegisterEntity
                reload <- 0
            | _ -> ()

            reload <- reload + 1

        member this.Draw (gameTime : GameTime) (position : RectangleF) =
            
            spriteBatch.DrawCircle(center position, radius, 100, Color.Blue, radius)
            spriteBatch.DrawCircle(center position, viewRadius, 100, Color.GreenYellow)