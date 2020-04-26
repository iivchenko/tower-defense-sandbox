namespace TowerDefenseSandbox.Game

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame

type WaveAction =
    | Create of enemy: EnemyType
    | Delay of delay: float32<second>

module Wave =

    let private random f =
        match f 0 6 with 
        | 0 | 1 | 2  -> EnemyType.Standard
        | 3 | 4 -> EnemyType.Fast
        | 5 -> EnemyType.Hard

    let rec private createChunk r pixel size chunk =
        match size with 
        | 0 -> 
            (chunk, pixel)
        | _ -> 
            let enemy = random r
            let price = GameBalance.enemyPrices.[enemy]

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
            let delay = (float32 <| random 1 6) * 1.0f<second>
            createIn random pixel chunkSizeMin chunkSizeMax chunk@[Delay delay]@wave

    let create random pixel chunkSizeMin chunkSizeMax = createIn random pixel chunkSizeMin chunkSizeMax []