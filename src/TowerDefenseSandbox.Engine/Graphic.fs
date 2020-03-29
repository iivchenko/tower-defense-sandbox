namespace TowerDefenseSandbox.Engine

open Microsoft.Xna.Framework.Graphics

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

module Graphic =

    let identity = Camera.identity()

    let draw (system: IDrawSystem) (transformationMatrix: CameraMatrix option) (shape: Shape) = 
        match transformationMatrix with 
        | Some(matrix) -> system.Draw matrix shape
        | _ -> system.Draw identity shape