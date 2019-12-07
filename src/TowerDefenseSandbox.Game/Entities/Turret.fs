namespace TowerDefenseSandbox.Game.Entities
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open MonoGame.Extended
open System.Collections.Generic

type Turret (spriteBatch : SpriteBatch, center : Vector2, enemies : IList<Enemy>) =

    let viewRadius = 100.0f
    let radius = 25.0f

    member this.Update () =
        //enemies |> Seq.filter (fun enemy -> enemy.Position )
        ()

    member this.Draw () =
        spriteBatch.DrawCircle(center, radius, 100, Color.Blue, radius)
        spriteBatch.DrawCircle(center, viewRadius, 100, Color.GreenYellow)