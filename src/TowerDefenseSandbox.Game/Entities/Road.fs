namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open MonoGame.Extended

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type Road (draw: Shape -> unit, zindex: int) = 
    
    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (time: float32<second>) (position: RectangleF) =
            ()
            
        member _.Draw (time: float32<second>) (position: RectangleF) =
            Rectangle(position.X, position.Y, position.Width, position.Height, true, Color.grey) |> draw

