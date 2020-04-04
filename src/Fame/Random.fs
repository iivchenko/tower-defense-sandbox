module Fame.Random

let mutable private r = System.Random()

let init seed =
    match seed with  
    | None -> r <- System.Random()
    | Some value -> r <- System.Random (value)

let random min max = r.Next(min, max)

