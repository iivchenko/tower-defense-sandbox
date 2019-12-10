namespace TowerDefenseSandbox.Game.Engine

open Microsoft.Xna.Framework

type IEntity =
    abstract member Radius : float32 with get
    abstract member Position : Vector2 with get, set
    abstract member Update : unit -> unit
    abstract member Draw : unit -> unit

type IEntityProvider =
    abstract member GetEntities : unit -> IEntity list
    abstract member RegisterEntity : IEntity -> unit
    abstract member RemoveEntity : IEntity -> unit

type EntityProvider() = 
    
    let mutable entities = list.Empty
    
    interface IEntityProvider with
        member this.GetEntities () =
            entities

        member this.RegisterEntity (entity : IEntity) = 
            entities <- entity :: entities

        member this.RemoveEntity(entity: IEntity): unit = 
            let rec remove entity list =
                match list with
                | [] -> []
                | head :: tail when LanguagePrimitives.PhysicalEquality entity head -> tail
                | head :: body :: tail when LanguagePrimitives.PhysicalEquality entity body -> head :: tail
                | head :: tail -> head :: remove entity tail

            entities <- remove entity entities