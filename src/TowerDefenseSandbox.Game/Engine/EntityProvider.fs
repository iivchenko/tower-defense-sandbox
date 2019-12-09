namespace TowerDefenseSandbox.Game.Engine

open Microsoft.Xna.Framework

type IEntity =
    abstract member Radius : float32 with get
    abstract member Position : Vector2 with get
    abstract member Update : unit -> unit
    abstract member Draw : unit -> unit

type IEntityProvider =
    abstract member GetEntities : unit -> IEntity list
    abstract member RegisterEntity : IEntity -> unit

type EntityProvider() = 
    let mutable entities = list.Empty
    interface IEntityProvider with
        member this.GetEntities () =
            entities

        member this.RegisterEntity (entity : IEntity) = 
            entities <- entity :: entities