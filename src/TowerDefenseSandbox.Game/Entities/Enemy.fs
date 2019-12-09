namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine

type Enemy (life : int, spriteBatch : SpriteBatch, center : Vector2, entityProvider : IEntityProvider) as this =    

    let mutable life = life
    let mutable center = center
    let radius = 10.0f
     
    do
        entityProvider.RegisterEntity this

    let max x y =
        if x > y then x else y

    interface IEntity with

        member this.Position = center

        member this.Radius = radius

        member this.Update () =
            center.Y <- center.Y + 1.0f

        member this.Draw () =
            spriteBatch.DrawCircle(center, radius, 100, Color.Red, radius)

    member this.ApplyDamage (damage : int) = 
        life <- max 0 life - damage