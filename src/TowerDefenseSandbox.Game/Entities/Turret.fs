namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open System.Collections.Generic

type Turret (spriteBatch : SpriteBatch, texture : Texture2D, position : Vector2, enemies : IList<Enemy>) =

    let radius = 100.0f
    let center = Vector2 ((texture.Width / 2 |> float32) + position.X, (texture.Height / 2 |> float32) + position.Y)

    member this.Update () =
        //enemies |> Seq.filter (fun enemy -> enemy.Position )
        ()

    member this.Draw () =

        spriteBatch.DrawCircle(center, radius, 100, Color.GreenYellow)

        spriteBatch.Draw (texture, position, Color.White)