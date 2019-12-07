namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended

type Enemy (life : int, spriteBatch : SpriteBatch, center : Vector2) =

    let mutable life = life
    let mutable center = center
    let radius = 10.0f

    let max x y =
        if x > y then x else y

    member this.Position = center

    member this.ApplyDamage (damage : int) = 
        life <- max 0 life - damage

    member this.Update () =
        center.Y <- center.Y + 1.0f

    member this.Draw () =
        spriteBatch.DrawCircle(center, radius, 100, Color.Red, radius)