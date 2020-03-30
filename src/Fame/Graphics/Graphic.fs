namespace Fame.Graphics

open System
open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended

open Fame

type Shape =
| Circle    of x: float32<pixel> * y: float32<pixel> * radius: float32<pixel> * fill: bool * color: Color
| Rectangle of x: float32<pixel> * y: float32<pixel> * width: float32<pixel> * height: float32<pixel> * fill: bool * color: Color
| Triangle  of x: float32<pixel> * y: float32<pixel> * a1: Vector<pixel> * a2: Vector<pixel> * a3: Vector<pixel> * color: Color
| Polygon   of x: float32<pixel> * y: float32<pixel> * points: Vector<pixel> list * color: Color
| Text      of x: float32<pixel> * y: float32<pixel> * text: string * font: SpriteFont * color: Color
| Shape     of Shape list

module Graphic =

    let private toVector2 (Vector(x, y): Vector<'u>) = Microsoft.Xna.Framework.Vector2(float32 x, float32 y)
    let private toXnaMatrix (CameraMatrix(a11, a12, _, a21, a22, _, a31, a32, _)) = 
        Microsoft.Xna.Framework.Matrix
                    (
                     a11,  a12,  0.0f, 0.0f, 
                     a21,  a22,  0.0f, 0.0f, 
                     0.0f, 0.0f, 1.0f, 0.0f, 
                     a31,  a32,  0.0f, 1.0f) |> Nullable<Microsoft.Xna.Framework.Matrix>

    let rec private drawInternal (spriteBatch: SpriteBatch) (shape: Shape)  =
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
            spriteBatch.DrawPolygon(Microsoft.Xna.Framework.Vector2(float32 x, float32 y), poligon, Microsoft.Xna.Framework.Color(r, g, b, a))
        | Polygon (x, y, points, Color(r, g, b, a)) ->
            let poligon = Shapes.Polygon(List.map toVector2 points)
            spriteBatch.DrawPolygon(Microsoft.Xna.Framework.Vector2(float32 x, float32 y), poligon, Microsoft.Xna.Framework.Color(r, g, b, a))
        | Text(x, y, text, font, Color(r, g, b, a)) -> 
            spriteBatch.DrawString(font, text, Microsoft.Xna.Framework.Vector2(float32 x, float32 y),  Microsoft.Xna.Framework.Color(r, g, b, a))
        | Shape(shapes) -> shapes |> List.iter (drawInternal spriteBatch)

    let draw (spriteBatch: SpriteBatch) (transformationMatrix: CameraMatrix option) (shape: Shape) = 
        match transformationMatrix with 
        | Some(matrix) -> 
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, toXnaMatrix matrix)
            drawInternal spriteBatch shape
            spriteBatch.End()
        | _ -> 
            spriteBatch.Begin()
            drawInternal spriteBatch shape
            spriteBatch.End()