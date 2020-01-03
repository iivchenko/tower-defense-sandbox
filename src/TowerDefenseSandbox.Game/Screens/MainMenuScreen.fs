namespace TowerDefenseSandbox.Game.Screens

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Myra.Graphics2D.UI
open Microsoft.Xna.Framework.Input

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type MainMenuScreen (manager: ScreenManager, exit: unit -> unit, draw: Shape -> unit, screenWith: int, screenHeight: int) =

    let mutable isEscUpPrev = true

    let startGame _ = manager.Next(GamePlayScreen(manager, draw, screenWith, screenHeight))
    let editGame _ = manager.Next(GameEditorScreen(manager, draw, screenWith, screenHeight))

    do
        let panel = new VerticalStackPanel()
        panel.HorizontalAlignment <- HorizontalAlignment.Center
        panel.VerticalAlignment <- VerticalAlignment.Center
        
        let newGameButton = new TextButton()
        newGameButton.Text <- "New Game"
        newGameButton.Id <- ""
        newGameButton.Click.Add(startGame)

        let createLevelButton = new TextButton()
        createLevelButton.Text <- "Create Level"
        createLevelButton.Id <- ""
        createLevelButton.Click.Add(editGame)

        panel.Widgets.Add(newGameButton)
        panel.Widgets.Add(createLevelButton)

        Desktop.Widgets.Add(panel)

    interface IScreen with

       member _.Update (_: float32<second>) =
            if not isEscUpPrev && Keyboard.GetState().IsKeyUp(Keys.Escape) then exit() else ()

            isEscUpPrev <- Keyboard.GetState().IsKeyUp(Keys.Escape)

        member _.Draw (time: float32<second>) =
            Desktop.Render ()