namespace TowerDefenseSandbox.Game.Scenes

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open Fame
open Fame.Input
open Fame.Messaging
open Fame.Scene
open TowerDefenseSandbox.Game
open Fame.Graphics

type GamePlaySetupStartGameMessage(maze: (int * int) list) =
    member _.Maze = maze

type GamePlaySetupExitMessage() = class end

type GamePlaySetupScene (queue: IMessageQueue, content: ContentManager, screenWidth: float32<pixel>, screenHeight: float32<pixel>, draw: CameraMatrix option -> Shape -> unit) =

    let h1 = content.Load<SpriteFont>("Fonts\H1")
    let h3 = content.Load<SpriteFont>("Fonts\H3")

    let mutable left = MouseButtonState.Released
    let mutable mazeLength = 25

    let size (font: SpriteFont) (text: string) = font.MeasureString(text)
    let sizeH3 = size h3
    let sizeH1 = size h1
    let button (Vector(x, y)) text = Text(x, y, text, h3, Color.white)
    let intersect (Vector(x, y)) shape = 
        match shape with 
        | Text(sx, sy, text, font, _) ->
            let ss = size font text
            x > sx && x < sx + ss.X * 1.0f<pixel> && y > sy && y < sy + ss.Y * 1.0f<pixel>
        | _ -> false            

    let backButtonUi = 
        let backText = "Back"       
        let backSize = size h3 backText       
        let backX  = screenWidth  - backSize.X  * 1.0f<pixel> - 15.0f<pixel>
        let backY  = screenHeight - backSize.Y  * 1.0f<pixel> - 100.0f<pixel>
       
        button (Vector(backX,  backY)) backText

    let startButtonUi =
        let startText = "Start"
        let startSize = size h3 startText
        let (Text(backX, backY, _, _, _)) = backButtonUi
        let startX = backX - startSize.X * 1.0f<pixel> - 30.0f<pixel>
        let startY = backY
        
        button (Vector(startX, startY)) startText

    let navigationUi = Shape(backButtonUi::startButtonUi::[])

    let mazeLengthLabelUi () = 
        let text = sprintf "Length: %i" mazeLength
        let textSize = sizeH3 text
        let x = (screenWidth / 2.0f)  - textSize.X  * 1.0f<pixel>
        let y = (screenHeight / 2.0f) - textSize.Y  * 1.0f<pixel>

        button (Vector(x, y)) text

    let mazeLengthIncButtonUi () =
        let text = "+"
        let (Text(mx, my, mt, mf, _)) = mazeLengthLabelUi ()
        let ms = mf.MeasureString(mt)
        let x = mx + ms.X * 1.0f<pixel> + 10.0f<pixel>
        let y = my      

        button (Vector(x, y)) text

    let mazeLengthDecButtonUi () = 
        let text = "-"
        let textSize = sizeH1 text
        let (Text(mx, my, _, _, _)) = mazeLengthLabelUi ()
        let x = mx - textSize.X * 1.0f<pixel> - 10.0f<pixel>
        let y = my      

        button (Vector(x, y)) text

    let ui () = Shape(navigationUi::mazeLengthLabelUi()::[])

    interface IScene with

        member _.Update (_: float32<second>) =
            match MouseInput.state () with 
            | { Position = position; LeftButton = state } when state = MouseButtonState.Released && left = MouseButtonState.Pressed -> 
                left <- state
                if (intersect position startButtonUi)
                then 
                    let maze = Maze.create (Random.random) mazeLength
                    queue.Push(GamePlaySetupStartGameMessage(maze))
                elif (intersect position backButtonUi)
                    then 
                        queue.Push(GamePlaySetupExitMessage())
                elif (intersect position (mazeLengthIncButtonUi()))
                    then
                        mazeLength <- mazeLength + 1
                elif (intersect position (mazeLengthDecButtonUi())) 
                    then 
                        mazeLength <- if mazeLength = 3 then 3 else mazeLength - 1
                else
                    ()

            | { LeftButton = state } -> 
                left <- state
            | _ -> ()

            match TouchInput.state() with 
            | { Id = _; State = state; X = x; Y = y }::[] when state = TouchLocationState.Released ->
                let position = Vector.init x y
                if (intersect position startButtonUi)
                then 
                    let maze = Maze.create (Random.random) mazeLength
                    queue.Push(GamePlaySetupStartGameMessage(maze))
                elif (intersect position backButtonUi)
                    then 
                        queue.Push(GamePlaySetupExitMessage())
                elif (intersect position (mazeLengthIncButtonUi()))
                    then
                        mazeLength <- mazeLength + 1
                elif (intersect position (mazeLengthDecButtonUi())) 
                    then 
                        mazeLength <- if mazeLength = 3 then 3 else mazeLength - 1
                else
                    ()
            | _ -> ()

        member _.Draw (_: float32<second>) =
            draw None ((Shape(ui()::mazeLengthIncButtonUi()::mazeLengthDecButtonUi()::[])))