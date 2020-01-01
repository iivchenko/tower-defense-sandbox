namespace TowerDefenseSandbox.Engine.UnitTests

open TowerDefenseSandbox.Engine

open NUnit.Framework
open FsUnit

[<TestFixture>]
module PointTests =

    [<TestCase(0.0f, 0.0f)>]
    [<TestCase(1.0f, 0.0f)>]
    [<TestCase(0.0f, 1.0f)>]
    [<TestCase(1.0f, 1.0f)>]
    [<TestCase(-1.0f, 1.0f)>]
    [<TestCase(1.0f, -1.0f)>]
    [<TestCase(10.0f, 10.0f)>]
    let ``init: test cases.``(x: float32, y: float32) = 

        // Arrange + Act
        let (Point.Point (x', y')) = Point.init x y

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
        let point1 = Point.init x1 y1
        let point2 = Point.init x2 y2

        // Act
        let actualDistance = Point.distance point1 point2

        // Assert
        actualDistance |> should equal expectedDistance