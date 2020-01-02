namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended

type Receiver (spriteBatch: SpriteBatch, entityProvider: IEntityProvider, zindex: int) = 
    
    let mutable life = 1.0f

    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)
    let toVector2 (Vector(x, y)) = Vector2 (x, y)

    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (_: GameTime) (position: RectangleF) =
            let c = center position
            let radius = position.Width / 2.0f * life
            let m = Set.empty
            let enemy =
                entityProvider.GetEntities()
                |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> Seq.filter (fun x -> (Vector.distance c x.Position) - x.Radius < radius)
                |> Seq.tryHead

            match enemy with 
            | None -> ()
            | Some e ->
                life <- life - 0.1f
                entityProvider.RemoveEntity e

            if life <= 0.0f then raise (System.Exception("Game Over")) else ()
            
        member _.Draw (_: GameTime) (position: RectangleF) =
            let radius = position.Width / 2.0f * life
            
            spriteBatch.DrawCircle(position |> center |> toVector2, radius, 100, Color.Coral)