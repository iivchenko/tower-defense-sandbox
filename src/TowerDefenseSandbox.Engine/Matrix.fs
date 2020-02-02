namespace TowerDefenseSandbox.Engine

type Matrix = 
    | Matrix of a11: float32 * a12: float32 * a21: float32 * a22: float32
    static member (*) ((Vector(x, y)), (Matrix(a11, a12, a21, a22))) = Vector(x * a11 + y * a21, x * a12 + y * a22)

module Matrix = 

    let identity () = Matrix(1.0f, 0.0f, 0.0f, 1.0f)
    let rotation angle = Matrix(cos angle, sin angle, -sin angle, cos angle)