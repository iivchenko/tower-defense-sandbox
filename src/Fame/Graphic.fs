namespace Fame

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework
open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended

open TowerDefenseSandbox.Engine
open System

type Color = Color of red: byte * green: byte * blue: byte * alpha: byte 

module Color =
    let white = Color (byte 255, byte 255, byte 255, byte 255)
    let red = Color (byte 255, byte 0, byte 0, byte 255)
    let grey = Color (byte 128, byte 128, byte 128, byte 255)
    let coral = Color(byte 255, byte 127, byte 80, byte 255)
    let aquamarine = Color(byte 127, byte 255, byte 212, byte 255)
    let black = Color(byte 0, byte 0, byte 0, byte 255)
    let blue = Color(byte 0, byte 0, byte 255, byte 255)

type Shape =
| Circle of x: float32<pixel> * y: float32<pixel> * radius: float32<pixel> * fill: bool * color: Color
| Rectangle of x: float32<pixel> * y: float32<pixel> * width: float32<pixel> * height: float32<pixel> * fill: bool * color: Color
| Triangle of x: float32<pixel> * y: float32<pixel> * a1: Vector<pixel> * a2: Vector<pixel> * a3: Vector<pixel> * color: Color
| Polygon of x: float32<pixel> * y: float32<pixel> * points: Vector<pixel> list * color: Color
| Text of x: float32<pixel> * y: float32<pixel> * text: string * font: SpriteFont * color: Color
| Shape of Shape list

type IDrawSystem =
    abstract member Draw: CameraMatrix -> Shape -> unit

type MonoGameGraphic (spriteBatch: SpriteBatch) =

    let toVector2 (Vector(x, y): Vector<'u>) = Vector2(float32 x, float32 y)
    let toXnaMatrix (CameraMatrix(a11, a12, a13, a21, a22, a23, a31, a32, a33)) = 
        Microsoft.Xna.Framework.Matrix
                    (
                     a11,  a12,  0.0f, 0.0f, 
                     a21,  a22,  0.0f, 0.0f, 
                     0.0f, 0.0f, 1.0f, 0.0f, 
                     a31,  a32,  0.0f, 1.0f) |> Nullable<Microsoft.Xna.Framework.Matrix>

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
        | Text(x, y, text, font, Color(r, g, b, a)) -> 
            spriteBatch.DrawString(font, text, Vector2(float32 x, float32 y),  Microsoft.Xna.Framework.Color(r, g, b, a))
        | Shape(shapes) -> shapes |> List.iter draw

    interface IDrawSystem with 

        member _.Draw (transformationMatrix: CameraMatrix) (shape: Shape) = 
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, toXnaMatrix transformationMatrix)
            draw shape
            spriteBatch.End()

module Graphic =

    let identity = Camera.identity()

    let draw (system: IDrawSystem) (transformationMatrix: CameraMatrix option) (shape: Shape) = 
        match transformationMatrix with 
        | Some(matrix) -> system.Draw matrix shape
        | _ -> system.Draw identity shape