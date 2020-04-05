namespace TowerDefenseSandbox.Game.Scenes

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Myra.Graphics2D.UI

open Fame
open Fame.Messaging
open Fame.Scene
open System
open TowerDefenseSandbox.Game

type GamePlaySetupStartGameMessage(maze: (int * int) list) =
    member _.Maze = maze

type GamePlaySetupExitMessage() = class end

type GamePlaySetupScene (queue: IMessageQueue, content: ContentManager) =

    let h3 = content.Load<SpriteFont>("Fonts\H3")

    let exit _ = queue.Push(GamePlaySetupExitMessage())

    do

        Desktop.Widgets.Clear()

        let panel = new VerticalStackPanel()
        panel.HorizontalAlignment <- HorizontalAlignment.Center
        panel.VerticalAlignment <- VerticalAlignment.Center

        let lengthLabel = Label()
        lengthLabel.Text <- "Length: "
        lengthLabel.Font <- h3

        let lengthSpin = SpinButton()
        lengthSpin.Value <- Nullable(25.0f)
        lengthSpin.Increment <- 1.0f
        lengthSpin.Minimum <- Nullable(3.0f)
        lengthSpin.PaddingLeft <- 50

        let lengthPanel = new HorizontalSplitPane()
        lengthPanel.Widgets.Add(lengthLabel)
        lengthPanel.Widgets.Add(lengthSpin)

        panel.Widgets.Add(lengthPanel)

        let menu = new HorizontalMenu()
        menu.HorizontalAlignment <- HorizontalAlignment.Right
        menu.VerticalAlignment <- VerticalAlignment.Bottom    
        menu.LabelHorizontalAlignment <- HorizontalAlignment.Center
        menu.LabelFont <- h3
        menu.Background <- Myra.Graphics2D.Brushes.SolidBrush(new Microsoft.Xna.Framework.Color(0, 0, 0, 0)) 
        menu.PaddingBottom <- 25
        menu.PaddingRight <- 25
              
        let startGameMenuItem = new MenuItem()
        startGameMenuItem.Text <- "Start"
        startGameMenuItem.Id <- ""
        startGameMenuItem.Selected.Add((fun _ -> 
                                                let maze = Maze.create (Random.random) (lengthSpin.Value.Value |> int)
                                                queue.Push(GamePlaySetupStartGameMessage(maze))))

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