namespace Fame.Graphics

open Fame

type CameraMatrix =
    | CameraMatrix of a11: float32 * a12: float32 * a13: float32 * a21: float32 * a22: float32 * a23: float32 * a31: float32 * a32: float32 * a33: float32
    static member (*) (Vector(x, y), (CameraMatrix(a11, a12, _, a21, a22, _, a31, a32, _))) = Vector(x * a11 + y * a21 + a31, x * a12 + y * a22 + a32)
    
    static member (*) ((CameraMatrix(a11, a12, a13, a21, a22, a23, a31, a32, a33)), (CameraMatrix(b11, b12, b13, b21, b22, b23, b31, b32, b33))) = 
        CameraMatrix
            (
                a11 * b11 + a12 * b21 + a13 * b31, a11 * b12 + a12 * b22 + a13 * b32, a11 * b13 + a12 * b23 + a13 * b33,
                a21 * b11 + a22 * b21 + a23 * b31, a21 * b12 + a22 * b22 + a23 * b32, a21 * b13 + a22 * b23 + a23 * b33,
                a31 * b11 + a32 * b21 + a33 * b31, a31 * b12 + a32 * b22 + a33 * b32, a31 * b13 + a32 * b23 + a33 * b33
            )
    
    static member (*) ((CameraMatrix(a11, a12, a13, a21, a22, a23, a31, a32, a33)), scalar) = 
        CameraMatrix
            (
                a11 * scalar, a12 * scalar, a13 * scalar, 
                a21 * scalar, a22 * scalar, a23 * scalar, 
                a31 * scalar, a32 * scalar, a33 * scalar
            )

module internal Camera = 

    let identity () = 
           CameraMatrix
               (
                   1.0f, 0.0f, 0.0f,
                   0.0f, 1.0f, 0.0f,
                   0.0f, 0.0f, 1.0f
               )

    let transpose (CameraMatrix(a11, a12, a13, a21, a22, a23, a31, a32, a33)) =
        CameraMatrix
            (
                a11, a21, a31, 
                a12, a22, a32, 
                a13, a23, a33
            )   

    let scale scale =
        CameraMatrix
            (
                scale, 0.0f,  0.0f,
                0.0f,  scale, 0.0f,
                0.0f,  0.0f,  1.0f
            )

    let inverse (CameraMatrix(a11, a12, a13, a21, a22, a23, a31, a32, a33)) =
        let d a11 a12 a21 a22 = a11 * a22 - a12 * a21

        let t = CameraMatrix
                    (   
                        +(d a22 a23 a32 a33), -(d a21 a23 a31 a33), +(d a21 a22 a31 a32),
                        -(d a12 a13 a32 a33), +(d a11 a13 a31 a33), -(d a11 a12 a31 a32),
                        +(d a12 a13 a22 a23), -(d a11 a13 a21 a23), +(d a11 a12 a21 a22)
                    ) |> transpose

        let d = a11 * a22 * a33 + a12 * a23 * a31 + a13 * a21 * a32 - a13 * a22 * a31 - a12 * a21 * a33 - a11 * a23 * a32

        t * (1.0f / d)

    let translate (Vector(x, y)) =
        CameraMatrix
            (
                1.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f,
                x,    y   , 1.0f
            )

type Camera(minScale: float32, maxScale: float32) = 

    let mutable matrix = Camera.identity()
    let mutable scaleMatrix = Camera.identity()
    let mutable translationMatrix = Camera.identity()
    let mutable inverse = matrix

    let mutable scale = 1.0f
    let mutable position = Vector.init 0.0f<pixel> 0.0f<pixel>

    member _.Zoom 
        with get () = scale
        and set (value) = 
            scale <- if value < minScale then minScale else if value > maxScale then maxScale else value
            scaleMatrix <- Camera.scale scale
            matrix <- Camera.identity() * scaleMatrix * translationMatrix
            inverse <- Camera.inverse matrix

    member _.Position
        with get() = position
        and set(value) =
            position <- value
            let (Vector(x, y)) = position
            translationMatrix  <- Camera.translate (Vector.init (float32 x) (float32 y))
            matrix <-  Camera.identity() * scaleMatrix * translationMatrix
            inverse <- Camera.inverse matrix

    member _.Matrix = matrix

    member _.Inverse = inverse