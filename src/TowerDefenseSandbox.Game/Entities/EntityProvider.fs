namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open System.Collections.Generic
open Fame.Graphics

type IEntity =
    abstract member Update: float32<second> -> unit
    abstract member Draw: unit -> Shape

type IEntityProvider =
    abstract member Update: float32<second> -> unit
    abstract member GetEntities: unit -> IEntity seq
    abstract member RegisterEntity: IEntity -> unit
    abstract member RemoveEntity: IEntity -> unit
    abstract member Flush: unit -> unit

type EntityProvider() = 
    let addEntities = List<IEntity>()
    let removeEntities = List<IEntity>()
    let entities = List<IEntity>()
    
    interface IEntityProvider with

        member _.Update(delta: float32<second>) = 
            entities |> Seq.iter (fun x -> x.Update delta)
        
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