namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine

type Enemy (life : int, spriteBatch : SpriteBatch, center : Vector2, entityProvider : IEntityProvider, path : Vector2 list) =

    let mutable life = life
    let mutable center = center
    let mutable path = path
    let radius = 10.0f

    let limit = Mathx.max 0

    let direction (v1 : Vector2) (v2 : Vector2) = 
        let direction = (v1 - v2)
        direction.Normalize()

        direction

    interface IEntity with

        member _.Position
            with get () = center
            and set (value) = center <- value

        member _.Radius = radius

        member _.Update (gameTime : GameTime) =

            match path with 
            | h::tail when h.X = center.X && h.Y = center.Y -> path <- tail
            | h::_ -> center <- center + direction h center
            | _ -> ()

        member _.Draw (gameTime : GameTime) =
            spriteBatch.DrawCircle(center, radius, 100, Color.Red, radius)

    member this.ApplyDamage (damage : int) = 
        life <- life - damage |> limit

        if life = 0 then entityProvider.RemoveEntity this else ()