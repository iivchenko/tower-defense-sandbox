namespace TowerDefenseSandbox.Game.Engine

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open MonoGame.Extended

open TowerDefenseSandbox.Engine

type ICell =
    abstract member ZIndex: int with get
    abstract member Update: float32<second> -> RectangleF -> unit
    abstract member Draw: float32<second> -> RectangleF -> unit

type Grid (draw: Shape -> unit, width: int, height: int, cellWidth: float32, cellHeight: float32) = 

    let mutable grid: ICell option [,] = Array2D.init width height (fun _ _ -> Option.None)
    
    let apply action (entity: ICell option) (x: int) (y: int) (time: float32<second>) = 
        match entity with 
        | Some e -> action e x y time
        | None -> ()

    let update = apply (fun entity x y time -> entity.Update time <| RectangleF(float32 x * cellWidth , float32 y * cellHeight, cellWidth, cellHeight))
    let drawAll = apply (fun entity x y time -> entity.Draw time <| RectangleF(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight))

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

    member _.Update (time: float32<second>) =
        grid |> Array2D.iteri (fun x y e -> update e x y time)

    member _.Draw (time: float32<second>) =
        grid 
            |> Array2D.mapi (fun x y e -> x, y, e) 
            |> flaten 
            |> Seq.filter (fun (_, _, e) -> isSome e)
            |> Seq.sortBy (fun (_, _, Some e) -> e.ZIndex) 
            |> Seq.iter (fun (x, y, e) -> drawAll e x y time)

        for x in [0..(Array2D.length1 grid) - 1] do 
            for y in [0..(Array2D.length2 grid) - 1] do 
                Rectangle(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight, false, Color.black) |> draw