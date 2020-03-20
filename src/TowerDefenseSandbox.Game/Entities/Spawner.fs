namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type EnemyFactory (draw: Shape -> unit, pushMessage: EnemyMessage -> unit) = 
    
    let mutable path = []

    member _.CreateRegular (center: Vector<pixel>) =
        Enemy.CreateRegular (center, path, draw, pushMessage)

    member _.CreateFast (center: Vector<pixel>) =
        Enemy.CreateFast (center, path, draw, pushMessage)

    member _.CreateHard (center: Vector<pixel>) =
        Enemy.CreateHard (center, path, draw, pushMessage)

    member _.UpdatePath (newPath: Vector<pixel> list) =
        path <- newPath

type EnemyType = 
    | Standard
    | Fast
    | Hard

type WaveChunk = WaveChunk of enemy: EnemyType * time: float32<second>
type Wave = Wave of WaveChunk list

module Wave =

    let rec next waves = 
        match waves with
        | (Wave(chunks))::nextWaves -> 
            match chunks with 
            | (WaveChunk(enemies, time))::nextChunks -> 
                let wave = Wave(nextChunks)
                Some (enemies, time, wave::nextWaves)
            | [] -> next nextWaves
        | [] -> None

    let calculateTime wave = 
        let rec calculate chunks acc =
            match chunks with 
            | [] -> acc
            | (WaveChunk(_, time))::tail -> calculate tail (acc + time)
            
        calculate wave 0.0f<second>

type WavesOverMessage () = class end

type Spawner (position: Vector<pixel>, factory: EnemyFactory, pushMessage: WavesOverMessage -> unit, entityProvider: IEntityProvider) =

    [<Literal>] 
    let spawnTime = 1.0f<second>

    let radius = 15.0f<pixel>
    let mutable nextSpawn = spawnTime
    let (Vector(x, y)) = position
    let body = Circle(x, y, radius, false, Color.aquamarine)

    // Blinker
    let maxK = 1.0f
    let factor = 0.1f
    let frequency = 25.0f<1/second>
    let mutable k = 0.0f

    let mutable waves = [
        Wave([
            WaveChunk(Standard, 5.0f<second>)
        ]);
        Wave([
            WaveChunk(Standard, 2.0f<second>);
            WaveChunk(Standard, 5.0f<second>)
        ]);
        Wave([
            WaveChunk(Standard, 0.5f<second>);
            WaveChunk(Standard, 0.5f<second>);
            WaveChunk(Standard, 5.0f<second>);
            WaveChunk(Standard, 0.5f<second>);
            WaveChunk(Standard, 0.5f<second>);
            WaveChunk(Standard, 10.0f<second>)
        ]);
        Wave([
            WaveChunk(Standard, 0.5f<second>);
            WaveChunk(Standard, 0.5f<second>);
            WaveChunk(Standard, 0.5f<second>);
            WaveChunk(Standard, 0.5f<second>);
            WaveChunk(Standard, 15.0f<second>);
        ]);
        Wave([
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 7.0f<second>);
        ]);
        Wave([
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 7.0f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 7.0f<second>);
        ]);
        Wave([
            WaveChunk(Standard, 1.0f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Hard, 1.0f<second>);
            WaveChunk(Standard, 10.3f<second>);
        ]);
        Wave([
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 10.0f<second>);
        ]);
        Wave([
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Hard, 1.0f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Hard, 1.0f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 10.0f<second>);
        ]);
        Wave([
            WaveChunk(Hard, 0.5f<second>);
            WaveChunk(Hard, 0.5f<second>);
            WaveChunk(Hard, 0.5f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Fast, 0.1f<second>);
            WaveChunk(Hard, 0.5f<second>);
            WaveChunk(Hard, 0.5f<second>);
            WaveChunk(Hard, 0.5f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
            WaveChunk(Standard, 0.3f<second>);
        ]);
    ]

    let rec spawn enemy =
        match enemy with 
        | Standard -> factory.CreateRegular position |> ignore
        | Fast -> factory.CreateFast position |> ignore
        | Hard -> factory.CreateHard position |> ignore
        
    interface IEntity with

        member _.Update (delta: float32<second>) =
            if nextSpawn < 0.0f<second>
                then
                    match Wave.next waves with 
                    | Some (enemy, time, nextWaves) ->
                        waves <- nextWaves
                        nextSpawn <- time
                        spawn enemy
                    | None when Seq.where (fun x -> x.GetType() = typeof<Enemy>) (entityProvider.GetEntities()) |> Seq.fold (fun acc _-> acc + 1) 0 > 0 -> 
                        ()
                    | None -> 
                        pushMessage (WavesOverMessage())
                else
                    nextSpawn <- nextSpawn - delta

            if k <= maxK then k <- k + factor * frequency * delta else k <- 0.0f
            
        member _.Draw() = Shape(Circle(x, y, radius * k, false, Color.red)::body::[])