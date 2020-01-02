namespace TowerDefenseSandbox.Engine.UnitTests

open TowerDefenseSandbox.Engine

open NUnit.Framework
open FsUnit

[<TestFixture>]
module VectorTests =

    [<TestCase(0.0f, 0.0f)>]
    [<TestCase(1.0f, 0.0f)>]
    [<TestCase(0.0f, 1.0f)>]
    [<TestCase(1.0f, 1.0f)>]
    [<TestCase(-1.0f, 1.0f)>]
    [<TestCase(1.0f, -1.0f)>]
    [<TestCase(10.0f, 10.0f)>]
    let ``init: test cases.``(x: float32, y: float32) = 

        // Arrange + Act
        let (Vector (x', y')) = Vector.init x y

        // Assert
        x' |> should equal x
        y' |> should equal y

    [<TestCase(0.0f, 0.0f, 0.0f, 0.0f, 0.0f)>]
    [<TestCase(1.0f, 1.0f, 2.0f, 1.0f, 1.0f)>]
    [<TestCase(1.0f, 1.0f, 1.0f, 2.0f, 1.0f)>]
    [<TestCase(1.0f, 1.0f, 0.0f, 1.0f, 1.0f)>]
    [<TestCase(1.0f, 1.0f, 1.0f, 0.0f, 1.0f)>]
    let ``distance: test cases.`` (x1: float32, y1: float32, x2: float32, y2: float32, expectedDistance: float32) =

        // Arrange
        let vector1 = Vector.init x1 y1
        let vector2 = Vector.init x2 y2

        // Act
        let actualDistance = Vector.distance vector1 vector2

        // Assert
        actualDistance |> should equal expectedDistance

    [<TestCase(3.0f, 4.0f, 5.0f)>]
    let ``length: test cases.`` (x, y, expectedLength) =
        
        // Arrange + Act
        let actualLength = Vector.init x y |> Vector.length

        actualLength |> should equal expectedLength

    [<TestCase(3.0f, 4.0f, 0.6f, 0.8f)>]
    let ``normalize: test cases.`` (x, y, expectedX, expectedY) =
    
        // Arrange
        let vector = Vector.init x y
        let expectedVector = Vector.init expectedX expectedY

        // Act
        let actualVector = Vector.normalize vector

        // Assert
        actualVector |> should equal expectedVector

