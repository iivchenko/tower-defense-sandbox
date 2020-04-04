module TowerDefenseSandbox.Game.Maze
    
    let private getNeighbors (x, y) = 
        [
            (x - 1, y    );
            (x + 1, y    );
            (x    , y - 1);
            (x    , y + 1)
        ]

    let rec private createInternal (random: int -> int -> int) length parent maze =
        if length <= 0 
            then 
                maze
            else 

                let cells = 
                    getNeighbors parent 
                    |> List.except maze
                    |> List.filter (fun cell -> getNeighbors cell |> Seq.except (seq { yield parent }) |> Seq.except maze |> Seq.length = 3)

                match cells with 
                | [] -> 
                    maze
                | _ -> 
                    let cell = List.item (random 0 (List.length cells)) cells
                    createInternal random (length - 1) cell (cell::maze)

    let create (random: int -> int -> int) length = 
        if length <= 0
            then []
            else createInternal random (length - 1) (0, 0) [(0, 0)]