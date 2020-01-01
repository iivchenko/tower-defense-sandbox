module TowerDefenseSandbox.Engine.Point

type Point = Point of x: float32 * y: float32

let init (x: float32) (y: float32) = Point (x, y)

let distance (Point (x1, y1): Point) (Point(x2, y2): Point) = pown (x2 - x1) 2 + pown (y2 - y1) 2 |> Operators.sqrt