namespace TowerDefenseSandbox.Game.Screens

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Microsoft.Xna.Framework.Input

open Myra.Graphics2D.UI

open System.IO
open Newtonsoft.Json

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine
open TowerDefenseSandbox.Game.Entities

type GameEditorScreen(manager: IScreenManager, draw: Shape -> unit, screenWith: int, screenHeight: int) =

    let cellWidth = 48.0f
    let cellHeight = 45.0f
    let columns = screenWith / int cellWidth
    let raws = screenHeight / int cellHeight
    let grid = Grid (draw, columns, raws, cellWidth, cellHeight)

    let mutable isEscUpPrev = true
    let mutable leftButtonPreviousState = ButtonState.Released
    let mutable rightButtonPreviousState = ButtonState.Released
    let mutable middleButtonPreviousState = ButtonState.Released

    let mutable currentEdit = 0

    let mapTo (e: ICell) = 
        match e with 
        | :? Spawner -> 0
        | :? Road -> 1
        | :? Receiver -> 2

    do
        Desktop.Widgets.Clear()

    interface IScreen with 
        member _.Update(time: float32<second>) =
            if not isEscUpPrev && Keyboard.GetState().IsKeyUp(Keys.Escape) then manager.ToMainMenu() else ()
        
            isEscUpPrev <- Keyboard.GetState().IsKeyUp(Keys.Escape)

            let state = Mouse.GetState()
            
            if middleButtonPreviousState = ButtonState.Pressed && state.MiddleButton = ButtonState.Released then
                currentEdit <- (currentEdit + 1) % 3
            else 
                ()

            middleButtonPreviousState <- state.MiddleButton

            if leftButtonPreviousState = ButtonState.Pressed && state.LeftButton = ButtonState.Released then
                let x = state.X / int cellWidth
                let y = state.Y / int cellHeight

                grid.[x, y] <- match currentEdit with
                | 0 -> Spawner (0, draw, new EnemyFactory((fun _ -> ()), new EntityProvider())) :> ICell |> Some
                | 1 -> Road (draw, 0) :> ICell |> Some
                | 2 -> Receiver (draw, new EntityProvider(), 0) :> ICell |> Some
                
            else 
                ()

            leftButtonPreviousState <- state.LeftButton

            if rightButtonPreviousState = ButtonState.Pressed && state.RightButton = ButtonState.Released then
                let x = state.X / int cellWidth
                let y = state.Y / int cellHeight

                grid.[x, y] <- None
            else 
                ()

            rightButtonPreviousState <- state.RightButton

            if Keyboard.GetState().IsKeyDown(Keys.Enter) then
                let data =
                    seq {
                        for x in [0..columns - 1] do 
                            for y in [0..raws - 1] do 
                            yield (x, y, grid.[x, y]) } 
                    |> Seq.filter (fun (_, _, i) -> Option.isSome i)
                    |> Seq.map (fun (x, y, Some i) -> (x, y, mapTo i))
                    |> Seq.toList

                File.WriteAllText("level.json", JsonConvert.SerializeObject(data));

            else
                ()

        member _.Draw(time: float32<second>) = 
            grid.Draw time