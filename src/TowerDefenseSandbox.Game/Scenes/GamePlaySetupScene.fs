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

type GameDifficult =
    | Easy
    | Normal
    | Hard

type GamePlaySetupStartGameMessage(maze: (int * int) list, waves: int, lifes: int) =
    member _.Maze = maze
    member _.Waves = waves
    member _.Lifes = lifes

type GamePlaySetupExitMessage() = class end

type GamePlaySetupScene (queue: IMessageQueue, content: ContentManager, screenWidth: float32<pixel>, screenHeight: float32<pixel>, draw: CameraMatrix option -> Shape -> unit) =

    let h1 = content.Load<SpriteFont>("Fonts\H1")
    let h3 = content.Load<SpriteFont>("Fonts\H3")

    let mutable left = MouseButtonState.Released
    let mutable mazeLength = 25
    let mutable waves = 30
    let mutable difficult = GameDifficult.Normal
    let mutable lifes = 10

    let placeholderSize = h3.MeasureString "MMMMMMMMMMMM"
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
        let x = (screenWidth / 2.0f) - (textSize.X / 2.0f) * 1.0f<pixel>
        let y = (screenHeight / 2.0f) - 2.0f<pixel> * placeholderSize.Y 

        button (Vector(x, y)) text

    let mazeLengthIncButtonUi =
        let text = "+"
        let x = (screenWidth / 2.0f) + (placeholderSize.X / 2.0f) * 1.0f<pixel>
        let y = (screenHeight / 2.0f) - 2.0f<pixel> * placeholderSize.Y 

        button (Vector(x, y)) text

    let mazeLengthDecButtonUi = 
        let text = "-"
        let x = (screenWidth / 2.0f) - (placeholderSize.X / 2.0f) * 1.0f<pixel>
        let y = (screenHeight / 2.0f) - 2.0f<pixel> * placeholderSize.Y 

        button (Vector(x, y)) text

    let (wavesDecButtonUi, wavesLabelUi, wavesIncButtonUi) = 
        let wavesText = fun () -> sprintf "Waves: %i" waves
        let wavesTextSize = wavesText >> sizeH3 
        let wavesX = fun () -> (screenWidth / 2.0f)  - (wavesTextSize().X / 2.0f) * 1.0f<pixel>
        let wavesY = fun () -> (screenHeight / 2.0f) - 1.0f<pixel> * placeholderSize.Y 

        let wavesLabelUi = fun () -> button (Vector(wavesX(), wavesY())) (wavesText())

        let wavesDecButtonUi =
            let text = "-"
            let x = (screenWidth / 2.0f) - (placeholderSize.X / 2.0f) * 1.0f<pixel>
            let y = (screenHeight / 2.0f) - 1.0f<pixel> * placeholderSize.Y 

            button (Vector(x, y)) text

        let wavesIncButtonUi =
            let text = "+"
            let x = (screenWidth / 2.0f) + (placeholderSize.X / 2.0f) * 1.0f<pixel>
            let y = (screenHeight / 2.0f) - 1.0f<pixel> * placeholderSize.Y 

            button (Vector(x, y)) text

        (wavesDecButtonUi, wavesLabelUi, wavesIncButtonUi)
    
    let waveControllerUi () = Shape(wavesDecButtonUi::wavesLabelUi()::wavesIncButtonUi::[]) 

    let (difficultDecButtonUi, difficultLabelUi, difficultIncButtonUi) = 
        let difficultText = fun () -> 
            match difficult with
            | Easy ->  "Easy" 
            | Normal -> "Normal"
            | Hard -> "Hard"

        let difficultTextSize = difficultText >> sizeH3 
        let difficultX = fun () -> (screenWidth / 2.0f)  - (difficultTextSize().X / 2.0f) * 1.0f<pixel>
        let difficultY = fun () -> (screenHeight / 2.0f) + 1.0f<pixel> * placeholderSize.Y 

        let difficultLabelUi = fun () -> button (Vector(difficultX(), difficultY())) (difficultText())

        let difficultDecButtonUi =
            let text = "-"
            let x = (screenWidth / 2.0f) - (placeholderSize.X / 2.0f) * 1.0f<pixel>
            let y = (screenHeight / 2.0f) + 1.0f<pixel> * placeholderSize.Y 

            button (Vector(x, y)) text

        let difficultIncButtonUi =
            let text = "+"
            let x = (screenWidth / 2.0f) + (placeholderSize.X / 2.0f) * 1.0f<pixel>
            let y = (screenHeight / 2.0f) + 1.0f<pixel> * placeholderSize.Y 

            button (Vector(x, y)) text

        (difficultDecButtonUi, difficultLabelUi, difficultIncButtonUi)
       
    let difficultControllerUi () = Shape(difficultDecButtonUi::difficultLabelUi()::difficultIncButtonUi::[]) 

    let (lifeDecButtonUi, lifeLabelUi, lifeIncButtonUi) = 
        let text = fun () -> sprintf "Lifes: %i" lifes
        let textSize = text >> sizeH3 
        let x = fun () -> (screenWidth / 2.0f)  - (textSize().X / 2.0f) * 1.0f<pixel>
        let y = fun () -> (screenHeight / 2.0f) + 2.0f<pixel> * placeholderSize.Y 

        let lifeLabelUi = fun () -> button (Vector(x(), y())) (text())

        let lifeDecButtonUi =
            let text = "-"
            let x = (screenWidth / 2.0f) - (placeholderSize.X / 2.0f) * 1.0f<pixel>
            let y = (screenHeight / 2.0f) + 2.0f<pixel> * placeholderSize.Y 

            button (Vector(x, y)) text

        let lifeIncButtonUi =
            let text = "+"
            let x = (screenWidth / 2.0f) + (placeholderSize.X / 2.0f) * 1.0f<pixel>
            let y = (screenHeight / 2.0f) + 2.0f<pixel> * placeholderSize.Y 

            button (Vector(x, y)) text

        (lifeDecButtonUi, lifeLabelUi, lifeIncButtonUi)
    
    let lifeControllerUi () = Shape(lifeDecButtonUi::lifeLabelUi()::lifeIncButtonUi::[]) 

    let ui () = Shape(navigationUi::mazeLengthLabelUi()::lifeControllerUi()::[])

    interface IScene with

        member _.Update (_: float32<second>) =
            match MouseInput.state () with 
            | { Position = position; LeftButton = state } when state = MouseButtonState.Released && left = MouseButtonState.Pressed -> 
                left <- state
                if (intersect position startButtonUi)
                then 
                    let maze = Maze.create (Random.random) mazeLength
                    queue.Push(GamePlaySetupStartGameMessage(maze, waves, lifes))
                elif (intersect position backButtonUi)
                    then 
                        queue.Push(GamePlaySetupExitMessage())
                elif (intersect position mazeLengthIncButtonUi)
                    then
                        mazeLength <- if mazeLength = 999 then 999 else mazeLength + 1
                elif (intersect position mazeLengthDecButtonUi) 
                    then 
                        mazeLength <- if mazeLength = 3 then 3 else mazeLength - 1
                elif (intersect position wavesDecButtonUi) 
                    then 
                        waves <- if waves = 15 then 15 else waves - 1
                elif (intersect position wavesIncButtonUi) 
                    then 
                        waves <- if waves = 999 then 999 else waves + 1
                elif (intersect position difficultDecButtonUi) 
                    then 
                        difficult <- match difficult with 
                                     | Easy -> Easy
                                     | Normal -> Easy
                                     | Hard -> Normal
                elif (intersect position difficultIncButtonUi) 
                    then 
                       difficult <- match difficult with 
                                    | Easy -> Normal
                                    | Normal -> Hard
                                    | Hard -> Hard
                elif (intersect position lifeDecButtonUi) 
                    then 
                        lifes <- if lifes = 1 then 1 else lifes - 1
                elif (intersect position lifeIncButtonUi) 
                    then 
                        lifes <- if lifes = 999 then 999 else lifes + 1
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
                    queue.Push(GamePlaySetupStartGameMessage(maze, waves, lifes))
                elif (intersect position backButtonUi)
                    then 
                        queue.Push(GamePlaySetupExitMessage())
                elif (intersect position mazeLengthIncButtonUi)
                    then
                        mazeLength <- if mazeLength = 999 then 999 else mazeLength + 1
                elif (intersect position mazeLengthDecButtonUi) 
                    then 
                        mazeLength <- if mazeLength = 3 then 3 else mazeLength - 1
                elif (intersect position wavesDecButtonUi) 
                    then 
                        waves <- if waves = 15 then 15 else waves - 1
                elif (intersect position wavesIncButtonUi) 
                    then 
                        waves <- if waves = 999 then 999 else waves + 1
                elif (intersect position difficultDecButtonUi) 
                    then 
                        difficult <- match difficult with 
                                    | Easy -> Easy
                                    | Normal -> Easy
                                    | Hard -> Normal
                elif (intersect position difficultIncButtonUi) 
                    then 
                        difficult <- match difficult with 
                                    | Easy -> Normal
                                    | Normal -> Hard
                                    | Hard -> Hard
                elif (intersect position lifeDecButtonUi) 
                    then 
                        lifes <- if lifes = 1 then 1 else lifes - 1
                elif (intersect position lifeIncButtonUi) 
                    then 
                        lifes <- if lifes = 999 then 999 else lifes + 1
                else
                    ()
            | _ -> ()

        member _.Draw (_: float32<second>) =
            draw None ((Shape(ui()::mazeLengthIncButtonUi::mazeLengthDecButtonUi::waveControllerUi()::difficultControllerUi()::[])))