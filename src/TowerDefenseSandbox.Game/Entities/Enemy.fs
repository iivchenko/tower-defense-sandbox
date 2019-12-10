namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine

type Enemy (life : int, spriteBatch : SpriteBatch, center : Vector2, entityProvider : IEntityProvider) =

    let mutable life = life
    let mutable center = center
    let radius = 10.0f

    let limit = Mathx.max 0

    interface IEntity with

        member _.Position
            with get () = center
            and set (value) = center <- value

        member _.Radius = radius

        member _.Update () =
            center.Y <- center.Y + 1.0f

        member _.Draw () =
            spriteBatch.DrawCircle(center, radius, 100, Color.Red, radius)

    member this.ApplyDamage (damage : int) = 
        life <- life - damage |> limit

        if life = 0 then entityProvider.RemoveEntity this else ()