namespace TowerDefenseSandbox.Engine

type Matrix = 
    | Matrix of a11: float32 * a12: float32 * a21: float32 * a22: float32

    static member (*) (Vector(x, y), (Matrix(a11, a12, a21, a22))) = 
        Vector(x * a11 + y * a21, x * a12 + y * a22)
    
    static member (*) ((Matrix(a11, a12, a21, a22)), (Matrix(b11, b12, b21, b22))) = 
        Matrix
            (
                a11 * b11 + a12 * b21, a11 * b12 + a12 * b22,
                a21 * b11 + a22 * b21, a21 * b12 + a22 * b22
            )

    static member (*) ((Matrix(a11, a12, a21, a22)), scalar) = 
        Matrix
            (
                a11 * scalar, a12 * scalar, 
                a21 * scalar, a22 * scalar
            )

module Matrix = 

    let identity () = 
        Matrix
            (
                1.0f, 0.0f,
                0.0f, 1.0f
            )

    let rotation angle = 
        Matrix
            (
                cos angle,  sin angle,
                -sin angle, cos angle
            )

    let scale (scale: float32) = 
        Matrix
            (
                scale, 0.0f,
                0.0f,  scale
            )