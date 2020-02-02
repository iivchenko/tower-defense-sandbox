namespace TowerDefenseSandbox.Engine

type Vector = 
    | Vector of x: float32 * y: float32
    static member (+) (Vector (x1, y1), Vector (x2, y2)) = Vector (x2 + x1, y2 + y1)
    static member (-) (Vector (x1, y1), Vector (x2, y2)) = Vector (x2 - x1, y2 - y1)
    static member (*) (Vector (x, y), scalar) = Vector (x * scalar, y * scalar)
    static member (*) (scalar, Vector (x, y)) = Vector (x * scalar, y * scalar)
    static member (/) (Vector (x, y), scalar) = Vector (x / scalar, y / scalar)

module Vector =

    let init (x) (y) = Vector (x, y)
    let unwrap (Vector(x, y)) = (x, y)
    let distance (Vector (x1, y1): Vector) (Vector(x2, y2): Vector) = pown (x2 - x1) 2 + pown (y2 - y1) 2 |> sqrt
    let length (Vector(x, y)) = pown x 2 + pown y 2 |> sqrt
    let normalize (v: Vector) = v / length v