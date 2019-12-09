namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine

type Turret (spriteBatch : SpriteBatch, center : Vector2, entityProvider : IEntityProvider) as this =

    let viewRadius = 100.0f
    let radius = 25.0f
    let mutable iCanSee = false
    
    do
        entityProvider.RegisterEntity this

    let sqr x = x * x
    let sqrt (x : float32) = System.Math.Sqrt(x |> float) |> float32
    let distance (entity1 : IEntity) (entity2 : IEntity) = (((entity1.Position.X - entity2.Position.X |> sqr) + (entity1.Position.Y - entity2.Position.Y |> sqr)) |> sqrt)

    interface IEntity with

        member this.Position = center

        member this.Radius = radius
    
        member this.Update () =

            iCanSee <-
                entityProvider.GetEntities()
                |> List.filter (fun x -> x <> (this :> IEntity))
                |> List.exists (fun x -> distance (this :> IEntity) x - x.Radius < viewRadius)

        member this.Draw () =
            spriteBatch.DrawCircle(center, radius, 100, (if iCanSee then Color.Blue else Color.Brown), radius)
            spriteBatch.DrawCircle(center, viewRadius, 100, Color.GreenYellow)