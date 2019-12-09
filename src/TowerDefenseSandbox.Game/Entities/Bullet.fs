namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Game.Engine
open Microsoft.Xna.Framework

type Bullet () =

    interface IEntity with
        
        member this.Radius = 0.0f

        member this.Position = Vector2.Zero

        member this.Update () =
            ()

        member this.Draw () =
            ()