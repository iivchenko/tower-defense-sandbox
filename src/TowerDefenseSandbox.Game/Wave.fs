namespace TowerDefenseSandbox.Game

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame

type EnemyType = 
    | Standard
    | Fast
    | Hard

type WaveAction =
    | Create of enemy: EnemyType
    | Delay of delay: float32<second>

module Wave =

    let private prices = 
        Map.empty
           .Add(EnemyType.Standard, 10<pixel>)
           .Add(EnemyType.Fast,      5<pixel>)
           .Add(EnemyType.Hard,     30<pixel>)

    let private random f =
        match f 0 5 with 
        | 0 | 1 -> EnemyType.Standard
        | 2 | 3 -> EnemyType.Fast
        | 4 -> EnemyType.Hard

    let rec private createChunk r pixel size chunk =
        match size with 
        | 0 -> 
            (chunk, pixel)
        | _ -> 
            let enemy = random r
            let price = prices.[enemy]

            if price <= pixel 
                then 
                   createChunk r (pixel - price) (size - 1) ((Create enemy)::(Delay 0.2f<second>)::chunk) // TODO: Randomize time
                else 
                    (chunk, 0<pixel>)

    let rec private createIn random pixel chunkSizeMin chunkSizeMax wave =
        match pixel with 
        | 0<pixel> -> 
            wave
        | _ -> 
            let (chunk, pixel) = createChunk random pixel (random chunkSizeMin chunkSizeMax + 1) []
            createIn random pixel chunkSizeMin chunkSizeMax wave@[Delay 1.0f<second>]@chunk

    let create random pixel chunkSizeMin chunkSizeMax = createIn random pixel chunkSizeMin chunkSizeMax []