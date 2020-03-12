﻿namespace TowerDefenseSandbox.Game.Scenes

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open Myra.Graphics2D.UI

open TowerDefenseSandbox.Game.Engine

type GameVictoryScreen (manager: IScreenManager, content: ContentManager) =

    let mutable isEscUpPrev = true

    let h1 = content.Load<SpriteFont>("Fonts\H1")
    let h3 = content.Load<SpriteFont>("Fonts\H3")

    let restartGame _ = manager.ToGamePlay()
    let exitGame _ = manager.ToMainMenu()

    do
        Desktop.Widgets.Clear()

        let panel = new VerticalStackPanel()
        panel.HorizontalAlignment <- HorizontalAlignment.Center
        panel.VerticalAlignment <- VerticalAlignment.Center

        let victoryLabel = new Label()
        victoryLabel.Text <- "Victory"
        victoryLabel.Font <- h1
        victoryLabel.HorizontalAlignment <- HorizontalAlignment.Center
        victoryLabel.VerticalAlignment <- VerticalAlignment.Top
        victoryLabel.PaddingBottom <- 100

        let menu = new VerticalMenu()
        menu.HorizontalAlignment <- HorizontalAlignment.Center
        menu.VerticalAlignment <- VerticalAlignment.Center    
        menu.LabelHorizontalAlignment <- HorizontalAlignment.Center
        menu.LabelFont <- h3
        menu.Background <- Myra.Graphics2D.Brushes.SolidBrush(new Microsoft.Xna.Framework.Color(0, 0, 0, 0)) 
              
        let restartGameMenuItem = new MenuItem()
        restartGameMenuItem.Text <- "Restart"
        restartGameMenuItem.Id <- ""
        restartGameMenuItem.Selected.Add(restartGame)

        let exitGameMenuItem = new MenuItem()
        exitGameMenuItem.Text <- "Exit"
        exitGameMenuItem.Id <- ""
        exitGameMenuItem.Selected.Add(exitGame)

        menu.Items.Add(restartGameMenuItem)
        menu.Items.Add(exitGameMenuItem)

        panel.Widgets.Add(victoryLabel)
        panel.Widgets.Add(menu)

        Desktop.Widgets.Add(panel)

    interface IScreen with

       member _.Update (_: float32<second>) =
          ()

        member _.Draw (time: float32<second>) =
            Desktop.Render ()