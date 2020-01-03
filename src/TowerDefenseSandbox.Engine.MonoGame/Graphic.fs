namespace TowerDefenseSandbox.Engine.MonoGame

open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended

open TowerDefenseSandbox.Engine

type MonoGameGraphic (spriteBatch: SpriteBatch) =

    interface IDrawSystem with 

        member _.Draw (shape: Shape) =
            match shape with 
            | Circle (x, y, radius, fill, Color(r, g, b, a)) -> 
                if fill 
                    then spriteBatch.DrawCircle(x, y, radius, 100, Microsoft.Xna.Framework.Color(r, g, b, a), radius)
                    else spriteBatch.DrawCircle(x, y, radius, 100, Microsoft.Xna.Framework.Color(r, g, b, a))
            | Rectangle (x, y, width, height, fill, Color(r, g, b, a)) -> 
                if fill 
                    then spriteBatch.FillRectangle(x, y, width, height, Microsoft.Xna.Framework.Color(r, g, b, a))
                    else spriteBatch.DrawRectangle(x, y, width, height, Microsoft.Xna.Framework.Color(r, g, b, a))
            | _ -> ()