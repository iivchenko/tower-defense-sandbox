namespace TowerDefenseSandbox.Game.Screens

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open TowerDefenseSandbox.Game.Engine
open Microsoft.Xna.Framework.Input
open TowerDefenseSandbox.Game.Entities
open System.IO
open Newtonsoft.Json

type GameEditorScreen(spriteBatch: SpriteBatch, screenWith: int, screenHeight: int) =

    let cellWidth = 48.0f
    let cellHeight = 45.0f
    let columns = screenWith / int cellWidth
    let raws = screenHeight / int cellHeight
    let grid = Grid (spriteBatch, columns, raws, cellWidth, cellHeight)

    let mutable leftButtonPreviousState = ButtonState.Released
    let mutable rightButtonPreviousState = ButtonState.Released
    let mutable middleButtonPreviousState = ButtonState.Released

    let mutable currentEdit = 0

    let mapTo (e: ICell) = 
        match e with 
        | :? Spawner -> 0
        | :? Road -> 1
        | :? Receiver -> 2

    interface IScreen with 
        member _.Update(gameTime: GameTime) =
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
                | 0 -> Spawner (0, spriteBatch, new EnemyFactory(spriteBatch, new EntityProvider())) :> ICell |> Some
                | 1 -> Road (spriteBatch, 0) :> ICell |> Some
                | 2 -> Receiver (spriteBatch, new EntityProvider(), 0) :> ICell |> Some
                
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

        member _.Draw(gameTime: GameTime) = 
            grid.Draw gameTime