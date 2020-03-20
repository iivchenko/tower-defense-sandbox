namespace TowerDefenseSandbox.Engine.MonoGame.Graphic

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended

open TowerDefenseSandbox.Engine

type MonoGameGraphic (spriteBatch: SpriteBatch) =

    let toVector2 (Vector(x, y): Vector<'u>) = Vector2(float32 x, float32 y)

    let rec draw (shape: Shape)  =
        match shape with 
        | Circle (x, y, radius, fill, Color(r, g, b, a)) -> 
            if fill 
                then spriteBatch.DrawCircle(float32 x, float32 y, float32 radius, 100, Microsoft.Xna.Framework.Color(r, g, b, a), float32 radius)
                else spriteBatch.DrawCircle(float32 x, float32 y, float32 radius, 100, Microsoft.Xna.Framework.Color(r, g, b, a))
        | Rectangle (x, y, width, height, fill, Color(r, g, b, a)) -> 
            if fill 
                then spriteBatch.FillRectangle(float32 x, float32 y, float32 width, float32 height, Microsoft.Xna.Framework.Color(r, g, b, a))
                else spriteBatch.DrawRectangle(float32 x, float32 y, float32 width, float32 height, Microsoft.Xna.Framework.Color(r, g, b, a))
        | Triangle (x, y, a1, a2, a3, Color(r, g, b, a)) ->
            let poligon = Shapes.Polygon([| toVector2 a1; toVector2 a2; toVector2 a3|])
            spriteBatch.DrawPolygon(Vector2(float32 x, float32 y), poligon, Microsoft.Xna.Framework.Color(r, g, b, a))
        | Polygon (x, y, points, Color(r, g, b, a)) ->
            let poligon = Shapes.Polygon(List.map toVector2 points)
            spriteBatch.DrawPolygon(Vector2(float32 x, float32 y), poligon, Microsoft.Xna.Framework.Color(r, g, b, a))
        | Shape(shapes) -> shapes |> List.iter draw
        | _ -> ()

    interface IDrawSystem with 

        member _.Draw (shape: Shape) = draw shape
           