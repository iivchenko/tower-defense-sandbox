namespace TowerDefenseSandbox.Game.Entities

open Microsoft.Xna.Framework
open MonoGame.Extended

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Road (draw: Shape -> unit, zindex: int) = 
    
    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (gameTime: GameTime) (position: RectangleF) =
            ()
            
        member _.Draw (gameTime: GameTime) (position: RectangleF) =
            Rectangle(position.X, position.Y, position.Width, position.Height, true, Color.grey) |> draw

