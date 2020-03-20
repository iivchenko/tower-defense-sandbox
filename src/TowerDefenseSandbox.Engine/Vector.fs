namespace TowerDefenseSandbox.Engine


type Vector<[<Measure>] 'u> = 
    | Vector of x: float32<'u> * y: float32<'u>
    static member (+) (Vector (x1, y1), Vector (x2, y2)) = Vector (x1 + x2, y1 + y2)
    static member (-) (Vector (x1, y1), Vector (x2, y2)) = Vector (x1 - x2, y1 - y2)
    static member (*) (Vector (x, y), scalar) = Vector (x * scalar, y * scalar)
    static member (*) (scalar, Vector (x, y)) = Vector (x * scalar, y * scalar)
    static member (/) (Vector (x, y), scalar) = Vector (x / scalar, y / scalar)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Vector =

    let init (x) (y) = Vector (x, y)
    let unwrap (Vector(x, y)) = (x, y)
    let distance (Vector (x1, y1)) (Vector(x2, y2)) = 
        let xd = x2 - x1
        let yd = y2 - y1
        (xd * xd) + (yd * yd) |> sqrt

    let length (Vector(x, y)) = (x * x) + (y * y) |> sqrt

    let normalize v = v / length v
    let direction (v1: Vector<_>) (v2: Vector<_>) = v2 - v1 |> normalize