module TowerDefenseSandbox.Game.Scenes.GamePlaySceneHud

open Fame
open Fame.Graphics
open Microsoft.Xna.Framework.Graphics

type PlayButton = | Pause | Slow | Play | Fast

type PlayButtonsInfo = 
        { Position: Vector<pixel>
          Button: PlayButton
          Scale: float32 }

let drawPlayButtons state  = 

    let scale = state.Scale
    let transform = Matrix.scale scale
    let (Vector(x, y)) = state.Position
    let foo = state.Button
    let pause = Shape
                    ([  
                        Rectangle(x + 00.0f<pixel> * scale, y + 00.0f<pixel> * scale, 10.0f<pixel> * scale, 30.0f<pixel> * scale, false, if foo = Pause then Color.red else Color.white); 
                        Rectangle(x + 15.0f<pixel> * scale, y + 00.0f<pixel> * scale, 10.0f<pixel> * scale, 30.0f<pixel> * scale, false, if foo = Pause then Color.red else Color.white) 
                    ])

    let x = x + 35.0f<pixel> * scale
    let slow = Triangle(x, y, Vector(20.0f<pixel>, 00.0f<pixel>) * transform, Vector(20.0f<pixel>, 30.0f<pixel>) * transform, Vector(00.0f<pixel>, 15.0f<pixel>) * transform, if foo = Slow then Color.red else Color.white)

    let x = x + 35.0f<pixel> * scale
    let play = Triangle(x, y, Vector(00.0f<pixel>, 00.0f<pixel>) * transform, Vector(00.0f<pixel>, 30.0f<pixel>) * transform, Vector(20.0f<pixel>, 15.0f<pixel>) * transform, if foo = Play then Color.red else Color.white)
        
    let x = x + 35.0f<pixel> * scale
    let fast1 = Triangle(x, y, Vector(00.0f<pixel>, 00.0f<pixel>) * transform, Vector(00.0f<pixel>, 30.0f<pixel>) * transform, Vector(20.0f<pixel>, 15.0f<pixel>) * transform, if foo = Fast then Color.red else Color.white)
    let x = x + 15.0f<pixel> * scale
    let fast2 = Triangle(x, y, Vector(00.0f<pixel>, 00.0f<pixel>) * transform, Vector(00.0f<pixel>, 30.0f<pixel>) * transform, Vector(20.0f<pixel>, 15.0f<pixel>) * transform, if foo = Fast then Color.red else Color.white)
    let fast = Shape([ fast1; fast2 ])

    Shape([ pause; slow; play; fast ])

let isInPlayButtons state (Vector(x, y)) =
    let scale = state.Scale
    let (Vector(px, py)) = state.Position
    let inBound x y (x', y', width, height) = x >= x' && x <= x' + width && y >= y' && y <= y' + height

    inBound x y (px, py, 140.0f<pixel> * scale, 30.0f<pixel> * scale)

let update (Vector(x, y)) state =
    let scale = state.Scale
    let (Vector(xp, yp)) = state.Position
    let inBound x y (x', y', width, height) = x >= x' && x <= x' + width && y >= y' && y <= y' + height

    { 
        state with 
            Button = if inBound x y (xp + 0.0f<pixel>, yp + 0.0f<pixel>, 35.0f<pixel> * scale, 30.0f<pixel> * scale)
                        then Pause 
                        else if inBound x y (xp + 35.0f<pixel> * scale, yp + 00.0f<pixel> * scale, 35.0f<pixel> * scale, 30.0f<pixel> * scale) 
                            then Slow
                            else if inBound x y (xp + 70.0f<pixel> * scale, yp + 00.0f<pixel> * scale, 35.0f<pixel> * scale, 30.0f<pixel> * scale) 
                                then Play
                                else Fast
    }

let drawStatusLable screenWith (font: SpriteFont) pixels lifes = 

    let statusLable = sprintf "Pixels: %i Life: %i" pixels lifes
    let size = font.MeasureString(statusLable);
    Text((float32 screenWith) * 1.0f<pixel> - size.X * 1.0f<pixel> - 20.0f<pixel>, 20.0f<pixel>, statusLable, font, Color.white)
