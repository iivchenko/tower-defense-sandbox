namespace TowerDefenseSandbox.Engine

type Color = Color of red: byte * green: byte * blue: byte * alpha: byte 

module Color =
    let red = Color (byte 255, byte 0, byte 0, byte 255)
    let grey = Color (byte 128, byte 128, byte 128, byte 255)
    let coral = Color(byte 255, byte 127, byte 80, byte 255)
    let aquamarine = Color(byte 127, byte 255, byte 212, byte 255)
    let black = Color(byte 0, byte 0, byte 0, byte 255)
    let blue = Color(byte 0, byte 0, byte 255, byte 255)

type Shape =
| Circle of x: float32 * y: float32 * radius: float32 * fill: bool * color: Color
| Rectangle of x: float32 * y: float32 * width: float32 * height: float32 * fill: bool * color: Color
| Triangle of x: float32 * y: float32 * a1: Vector * a2: Vector * a3: Vector * color: Color
| Polygon of x: float32 * y: float32 * points: Vector list * color: Color
| Shape of Shape list

type IDrawSystem =
    abstract member Draw: Shape -> unit

module Graphic =
    let draw (system: IDrawSystem) (shape: Shape) = system.Draw (shape)