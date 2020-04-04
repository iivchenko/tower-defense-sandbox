namespace TowerDefenseSandbox.Game.Scenes

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Myra.Graphics2D.UI

open Fame.Messaging
open Fame.Scene
open System

type GamePlaySetupStartGameMessage() = class end
type GamePlaySetupExitMessage() = class end

type GamePlaySetupScene (queue: IMessageQueue, content: ContentManager) =

    let h3 = content.Load<SpriteFont>("Fonts\H3")

    let start _ = queue.Push(GamePlaySetupStartGameMessage())
    let exit _ = queue.Push(GamePlaySetupExitMessage())

    do
        Desktop.Widgets.Clear()

        let panel = new VerticalStackPanel()
        panel.HorizontalAlignment <- HorizontalAlignment.Center
        panel.VerticalAlignment <- VerticalAlignment.Center

        let lengthLabel = Label()
        lengthLabel.Text <- "Length: "

        let lengthSpin = SpinButton()
        lengthSpin.Value <- Nullable(3.0f)
        lengthSpin.Increment <- 1.0f
        lengthSpin.Minimum <- Nullable(3.0f)

        let lengthPanel = new HorizontalSplitPane()
        lengthPanel.Widgets.Add(lengthLabel)
        lengthPanel.Widgets.Add(lengthSpin)

        let curvingLabel = Label()
        curvingLabel.Text <- "Curving (%): "
        curvingLabel.PaddingRight <- 45

        let curvingSpin = SpinButton()
        curvingSpin.Value <- Nullable(25.0f)
        curvingSpin.Increment <- 1.0f
        curvingSpin.Minimum <- Nullable(0.0f)
        curvingSpin.Maximum <- Nullable(100.0f)

        let curvingPanel = new HorizontalSplitPane()
        curvingPanel.Widgets.Add(curvingLabel)
        curvingPanel.Widgets.Add(curvingSpin)

        panel.Widgets.Add(lengthPanel)
        panel.Widgets.Add(curvingPanel)

        let menu = new HorizontalMenu()
        menu.HorizontalAlignment <- HorizontalAlignment.Right
        menu.VerticalAlignment <- VerticalAlignment.Bottom    
        menu.LabelHorizontalAlignment <- HorizontalAlignment.Center
        menu.LabelFont <- h3
        menu.Background <- Myra.Graphics2D.Brushes.SolidBrush(new Microsoft.Xna.Framework.Color(0, 0, 0, 0)) 
        menu.PaddingBottom <- 25
        menu.PaddingRight <- 25
              
        let startGameMenuItem = new MenuItem()
        startGameMenuItem.Text <- "Restart"
        startGameMenuItem.Id <- ""
        startGameMenuItem.Selected.Add(start)

        let exitGameMenuItem = new MenuItem()
        exitGameMenuItem.Text <- "Exit"
        exitGameMenuItem.Id <- ""
        exitGameMenuItem.Selected.Add(exit)

        menu.Items.Add(startGameMenuItem)
        menu.Items.Add(exitGameMenuItem)

        Desktop.Widgets.Add(panel)
        Desktop.Widgets.Add(menu)

    interface IScene with

       member _.Update (_: float32<second>) =
          ()

        member _.Draw (_: float32<second>) =
            Desktop.Render ()