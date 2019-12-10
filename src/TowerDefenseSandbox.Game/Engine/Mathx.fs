module TowerDefenseSandbox.Game.Engine.Mathx

open Microsoft.Xna.Framework

let max x y = if x > y then x else y

let public sqr x = x * x

let public sqrt (x : float32) = x |> float |> System.Math.Sqrt |> float32

let public distance (v1 : Vector2) (v2 : Vector2) = ((v1.X - v2.X |> sqr) + (v1.Y - v2.Y |> sqr)) |> sqrt

