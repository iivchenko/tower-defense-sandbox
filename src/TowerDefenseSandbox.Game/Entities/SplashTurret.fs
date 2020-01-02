namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open TowerDefenseSandbox.Game.Engine
open TowerDefenseSandbox.Engine

type SplashTurret (zindex: int, spriteBatch: SpriteBatch, entityProvider: IEntityProvider) =

    let viewRadius = 100.0f
    let radius = 25.0f
    let mutable reload = 0

    let center (position: RectangleF) = Vector.init (position.X + position.Width / 2.0f) (position.Y + position.Height / 2.0f)
    let toVector2 (Vector(x, y)) = Vector2 (x, y)

    interface ICell with

        member _.ZIndex = zindex

        member this.Update (gameTime: GameTime) (position: RectangleF) =
    
            let c = center position
            let target =
                entityProvider.GetEntities()
                |> Seq.filter (fun x -> x.GetType() = typeof<Enemy>)
                |> Seq.filter (fun x -> (Vector.distance c x.Position) - x.Radius < viewRadius)
                |> Seq.tryHead

            match target with 
            | None -> ()
            | Some x when reload > 70 ->
                SplashBullet(spriteBatch, c, entityProvider, x.Position) |> entityProvider.RegisterEntity
                reload <- 0
            | _ -> ()

            reload <- reload + 1

        member this.Draw (gameTime: GameTime) (position: RectangleF) =
            
            spriteBatch.DrawCircle(position |> center |> toVector2, radius, 100, Color.Red, radius)

            #if DEBUG
            spriteBatch.DrawCircle(position |> center |> toVector2, viewRadius, 100, Color.GreenYellow)
            #endif