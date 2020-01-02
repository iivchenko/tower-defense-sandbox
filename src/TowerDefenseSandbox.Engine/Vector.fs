namespace TowerDefenseSandbox.Engine

type Vector = Vector of x: float32 * y: float32

module Vector =

    let init (x: float32) (y: float32) = Vector (x, y)

    let distance (Vector (x1, y1): Vector) (Vector(x2, y2): Vector) = pown (x2 - x1) 2 + pown (y2 - y1) 2 |> Operators.sqrt