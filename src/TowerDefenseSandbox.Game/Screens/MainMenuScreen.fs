namespace TowerDefenseSandbox.Game.Screens

open Myra.Graphics2D.UI
open Microsoft.Xna.Framework

type MainMenuScreen (startGame, editGame) =

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
        member _.Update (_: GameTime) =
            ()

        member _.Draw (_: GameTime) =
            Desktop.Render ()