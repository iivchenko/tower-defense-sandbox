namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework

type Enemy (life : int, spriteBatch : SpriteBatch, texture : Texture2D, position : Vector2) =

    let mutable life = life
    let mutable position = position

    let max x y =
        if x > y then x else y

    member this.ApplyDamage (damage : int) = 
        life <- max 0 life - damage

    member this.Update () =
        position.Y <- position.Y + 1.0f

    member this.Draw () =
        spriteBatch.Draw (texture, position, Color.White)