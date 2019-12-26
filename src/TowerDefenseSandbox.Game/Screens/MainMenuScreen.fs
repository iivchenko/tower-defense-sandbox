namespace TowerDefenseSandbox.Game.Screens

open Myra.Graphics2D.UI
open Microsoft.Xna.Framework

type MainMenuScreen (startGame) =

    do
        let panel = new VerticalStackPanel()
        panel.HorizontalAlignment <- HorizontalAlignment.Center
        panel.VerticalAlignment <- VerticalAlignment.Center
        
        let newGameButton = new TextButton()
        newGameButton.Text <- "New Game"
        newGameButton.Id <- "";

        newGameButton.Click.Add(startGame)

        let createLevelButton = new TextButton()
        createLevelButton.Text <- "Create Level"
        createLevelButton.Id <- "";

        panel.Widgets.Add(newGameButton)
        panel.Widgets.Add(createLevelButton)

        Desktop.Widgets.Add(panel)

    interface IScreen with 
        member this.Update (gameTime : GameTime) =
            ()

        member this.Draw (gameTime : GameTime) =
            Desktop.Render ()