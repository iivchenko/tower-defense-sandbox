namespace TowerDefenseSandbox.Game.Engine

open Microsoft.Xna.Framework
open MonoGame.Extended
open Microsoft.Xna.Framework.Graphics

type ICell =
    abstract member ZIndex : int with get
    abstract member Update : GameTime -> RectangleF -> unit
    abstract member Draw : GameTime -> RectangleF -> unit

type Grid (spriteBatch : SpriteBatch, width : int, height : int, cellWidth : float32, cellHeight : float32) = 

    let mutable grid : ICell option [,] = Array2D.init width height (fun _ _ -> Option.None)
    
    let apply action (entity : ICell option) (x : int) (y : int) (time : GameTime) = 
        match entity with 
        | Some e -> action e x y time
        | None -> ()

    let update = apply (fun entity x y time -> entity.Update time <| RectangleF(float32 x * cellWidth , float32 y * cellHeight, cellWidth, cellHeight))
    let draw = apply (fun entity x y time -> entity.Draw time <| RectangleF(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight))

    let isSome = 
        function
        | Some _ -> true
        | _ -> false 

    let flaten array2D = 
        seq { for x in [0..(Array2D.length1 array2D) - 1] do 
                    for y in [0..(Array2D.length2 array2D) - 1] do 
                        yield array2D.[x, y] }
    
    member _.Item
           with get(x, y) = grid.[x, y]
           and set (x, y) value = grid.[x, y] <- value

    member _.Update (gameTime : GameTime) =
        grid |> Array2D.iteri (fun x y e -> update e x y gameTime)

    member _.Draw (gameTime : GameTime) =
        grid 
            |> Array2D.mapi (fun x y e -> x, y, e) 
            |> flaten 
            |> Seq.filter (fun (_, _, e) -> isSome e)
            |> Seq.sortBy (fun (_, _, Some e) -> e.ZIndex) 
            |> Seq.iter (fun (x, y, e) -> draw e x y gameTime)

        for x in [0..(Array2D.length1 grid) - 1] do 
            for y in [0..(Array2D.length2 grid) - 1] do 
                spriteBatch.DrawRectangle(RectangleF(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight), Color.Black)