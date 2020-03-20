namespace TowerDefenseSandbox.Game.Scenes

open System.IO
open Newtonsoft.Json
open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Myra.Graphics2D.UI

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Engine.Messaging
open TowerDefenseSandbox.Engine.Input
open TowerDefenseSandbox.Engine.Scene
open TowerDefenseSandbox.Game.Engine
open TowerDefenseSandbox.Game.Entities

type ExitGameEditorMessage() = class end

type SaveGameEditMessage() = class end

type UpdateEditMessage() = class end

type PlaceEntityMessage(x: int, y: int) = 
    member _.X = x
    member _.Y = y

type RemoveEntityMessage(x: int, y: int) = 
    member _.X = x
    member _.Y = y

type SaveGameEditMessageHandler (saveGame: unit -> unit) = 
    
    interface IMessageHandler<SaveGameEditMessage> with 
        member _.Handle(_: SaveGameEditMessage) =
            saveGame()

type PlaceEntityMessageHandler(placeEntity: int -> int -> unit) =
    interface IMessageHandler<PlaceEntityMessage> with

        member _.Handle(message: PlaceEntityMessage)=
            placeEntity message.X message.Y

type RemoveEntityMessageHandler(removeEntity: int -> int -> unit) =
    interface IMessageHandler<RemoveEntityMessage> with

        member _.Handle(message: RemoveEntityMessage)=
            removeEntity message.X message.Y

type UpdateEditMessageHandler(updateEdit: unit -> unit) =
    interface IMessageHandler<UpdateEditMessage> with

        member _.Handle(_: UpdateEditMessage) =
            updateEdit()

type GameEditorScene(input: IInputController, register: IMessageHandlerRegister, draw: Shape -> unit, screenWith: int, screenHeight: int) =

    let cellWidth = 48.0f<pixel>
    let cellHeight = 45.0f<pixel>
    let columns = screenWith / int cellWidth
    let raws = screenHeight / int cellHeight
    let grid = Array2D.create columns raws None

    let mutable currentEdit = 0

    let mapTo (e: IEntity) = 
        match e with 
        | :? Spawner -> 0
        | :? Road -> 1
        | :? Receiver -> 2

    let center (column: int) (raw: int) = Vector.init ((float32 column) * cellWidth + cellWidth / 2.0f) ((float32 raw) * cellHeight + cellHeight / 2.0f)

    let saveGame () = 
        let data =
            seq {
                for x in [0..columns - 1] do 
                    for y in [0..raws - 1] do 
                    yield (x, y, grid.[x, y]) } 
            |> Seq.filter (fun (_, _, i) -> Option.isSome i)
            |> Seq.map (fun (x, y, Some i) -> (x, y, mapTo i))
            |> Seq.toList

        File.WriteAllText("level.json", JsonConvert.SerializeObject(data));

    let placeEntity x y = 
        let column = x / int cellWidth
        let raw = y / int cellHeight
        grid.[column, raw] <- match currentEdit with
        | 0 -> Spawner (center column raw, draw, new EnemyFactory((fun _ -> ()), (fun _ -> ())), (fun _ -> ()), new EntityProvider()) :> IEntity |> Some
        | 1 -> Road (Vector.init (float32 column * cellWidth) (float32 raw * cellHeight), cellWidth, cellHeight, draw) :> IEntity |> Some
        | 2 -> Receiver (center column raw, draw, new EntityProvider()) :> IEntity |> Some

    let updateEdit() = currentEdit <- (currentEdit + 1) % 3

    let removeEntity x y = 
        let column = x / int cellWidth
        let raw = y / int cellHeight

        grid.[column, raw] <- None

    do
        Desktop.Widgets.Clear()

        register.Register(SaveGameEditMessageHandler(saveGame))
        register.Register(PlaceEntityMessageHandler(placeEntity))
        register.Register(RemoveEntityMessageHandler(removeEntity))
        register.Register(UpdateEditMessageHandler(updateEdit))

    interface IScene with 
        member _.Update(time: float32<second>) =
            
            input.Update(time)

        member _.Draw(time: float32<second>) = 
            for x in [0..columns - 1] do 
                for y in [0..raws - 1] do 
                    match grid.[x, y] with 
                    | Some entity -> entity.Draw time
                    | _ -> ()

            for x in [0 .. columns - 1] do
                for y in [0 .. raws - 1] do
                    Rectangle(float32 x * cellWidth, float32 y * cellHeight, cellWidth, cellHeight, false, Color.black ) |> draw