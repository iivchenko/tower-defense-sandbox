namespace TowerDefenseSandbox.Engine.MonoGame

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended

open TowerDefenseSandbox.Engine

type MonoGameGraphic (spriteBatch: SpriteBatch) =

    let toVector2 (Vector(x, y)) = Vector2(x, y)

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
            | Triangle (x, y, a1, a2, a3, Color(r, g, b, a)) ->
                let poligon = Shapes.Polygon([| toVector2 a1; toVector2 a2; toVector2 a3|])
                spriteBatch.DrawPolygon(Vector2(x, y), poligon, Microsoft.Xna.Framework.Color(r, g, b, a))
            | Polygon (x, y, points, Color(r, g, b, a)) ->
                let poligon = Shapes.Polygon(List.map toVector2 points)
                spriteBatch.DrawPolygon(Vector2(x, y), poligon, Microsoft.Xna.Framework.Color(r, g, b, a))
            | _ -> ()