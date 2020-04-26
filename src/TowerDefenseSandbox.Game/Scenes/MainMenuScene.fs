namespace TowerDefenseSandbox.Game.Scenes

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Myra.Graphics2D.UI
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open Fame.Messaging
open Fame.Scene

type MainMenuStartGameMessage() = class end
type ExitApplicationMessage() = class end

type MainMenuScene (queue: IMessageQueue, content: ContentManager) =

    let version = "version: 0.4.0"

    let startGame _ = queue.Push(MainMenuStartGameMessage())
    let exitGame _ = queue.Push(ExitApplicationMessage())

    let h1 = content.Load<SpriteFont>("Fonts\H1")
    let h3 = content.Load<SpriteFont>("Fonts\H3")

    do
        Desktop.Widgets.Clear()

        let panel = new VerticalStackPanel()
        panel.HorizontalAlignment <- HorizontalAlignment.Center
        panel.VerticalAlignment <- VerticalAlignment.Center

        let label = new Label()
        label.Text <- "The Tower Defence"
        label.Font <- h1
        label.HorizontalAlignment <- HorizontalAlignment.Center
        label.VerticalAlignment <- VerticalAlignment.Top
        label.PaddingBottom <- 100

        let menu = new VerticalMenu()
        menu.HorizontalAlignment <- HorizontalAlignment.Center
        menu.VerticalAlignment <- VerticalAlignment.Center    
        menu.LabelHorizontalAlignment <- HorizontalAlignment.Center
        menu.LabelFont <- h3
        menu.Background <- Myra.Graphics2D.Brushes.SolidBrush(new Microsoft.Xna.Framework.Color(0, 0, 0, 0)) 
        
        let newGameMenuItem = new MenuItem()
        newGameMenuItem.Text <- "New Game"
        newGameMenuItem.Id <- ""
        newGameMenuItem.Selected.Add(startGame)

        let exitMenuItem = new MenuItem()
        exitMenuItem.Text <- "Exit"
        exitMenuItem.Id <- ""
        exitMenuItem.Selected.Add(exitGame)

        menu.Items.Add(newGameMenuItem)
        menu.Items.Add(exitMenuItem)

        let versionLabel = new Label()
        versionLabel.Text <- version
        versionLabel.HorizontalAlignment <- HorizontalAlignment.Right
        versionLabel.VerticalAlignment <- VerticalAlignment.Bottom
        versionLabel.PaddingRight <- 25
        versionLabel.PaddingBottom <- 25

        panel.Widgets.Add(label)
        panel.Widgets.Add(menu)

        Desktop.Widgets.Add(panel)
        Desktop.Widgets.Add(versionLabel)

    interface IScene with
    
        member _.Update (_: float32<second>) = ()
    
        member _.Draw (_: float32<second>) = Desktop.Render ()