namespace TowerDefenseSandbox.Game.Engine

open Microsoft.Xna.Framework

type ICell =
    abstract member Update : GameTime -> Vector2 -> unit
    abstract member Draw : GameTime -> Vector2 -> unit

type Grid (width : int, height : int, cellWidth : float32, cellHeight : float32) = 

    let mutable grid : ICell option [,] = Array2D.init width height (fun _ _ -> Option.None)
    
    let apply action (entity : ICell option) (x : int) (y : int) (time : GameTime) = 
        match entity with 
        | Some e -> action e x y time
        | None -> ()

    let update = apply (fun entity x y time -> entity.Update time <| Vector2(float32 x * cellWidth , float32 y * cellHeight ))
    let draw = apply (fun entity x y time -> entity.Draw time <| Vector2(float32 x * cellWidth , float32 y * cellHeight))
    
    member _.Item
           with get(x, y) = grid.[x, y]
           and set (x, y) value = grid.[x, y] <- value

    member _.Update (gameTime : GameTime) =
        grid |> Array2D.iteri (fun x y e -> update e x y gameTime)

    member _.Draw (gameTime : GameTime) =
        grid |> Array2D.iteri (fun x y e -> draw e x y gameTime)