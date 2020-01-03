namespace TowerDefenseSandbox.Game.Engine

open System.Collections.Generic
open Microsoft.Xna.Framework

open TowerDefenseSandbox.Engine

type IEntity =
    abstract member Radius: float32 with get
    abstract member Position: Vector with get, set
    abstract member Update: GameTime -> unit
    abstract member Draw: GameTime -> unit

type IEntityProvider =
    abstract member GetEntities: unit -> IEntity seq
    abstract member RegisterEntity: IEntity -> unit
    abstract member RemoveEntity: IEntity -> unit
    abstract member Flush: unit -> unit

type EntityProvider() = 
    let addEntities = List<IEntity>()
    let removeEntities = List<IEntity>()
    let entities = List<IEntity>()
    
    interface IEntityProvider with
        
        member _.GetEntities () =
            entities :> IEnumerable<IEntity>

        member _.RegisterEntity (entity: IEntity) = 
            addEntities.Add(entity)

        member _.RemoveEntity(entity: IEntity): unit = 
            removeEntities.Add(entity)

        member _.Flush() =
            removeEntities |> Seq.iter (fun x -> entities.Remove(x) |> ignore)
            addEntities |> Seq.iter (fun x -> entities.Add(x) |> ignore)

            removeEntities.Clear()
            addEntities.Clear()