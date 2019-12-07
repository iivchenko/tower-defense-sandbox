namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended

type Turret (spriteBatch : SpriteBatch, center : Vector2, enemy : Enemy) =

    let viewRadius = 100.0f
    let radius = 25.0f
    let mutable iCanSee = false

    let sqr x = x * x
    let sqrt (x : float32) = System.Math.Sqrt(x |> float) |> float32
    
    member this.Update () =
        let distance = (((center.X - enemy.Position.X |> sqr) + (center.Y - enemy.Position.Y |> sqr)) |> sqrt) - enemy.Radius

        iCanSee <- distance < viewRadius 

    member this.Draw () =
        spriteBatch.DrawCircle(center, radius, 100, (if iCanSee then Color.Blue else Color.Brown), radius)
        spriteBatch.DrawCircle(center, viewRadius, 100, Color.GreenYellow)