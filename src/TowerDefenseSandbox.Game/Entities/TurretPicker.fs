namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame
open Fame.Graphics

open TowerDefenseSandbox.Game

type TurretPicker (
                    position: Vector<pixel>, 
                    width: float32<pixel>, 
                    height: float32<pixel>, 
                    raw: int) =

    let center (column: int) (raw: int) = Vector.init ((float32 column) * width + width / 2.0f) ((float32 raw) * height + height / 2.0f)

    member this.Click (clickPosition: Vector<pixel>) = 
        let (Vector(_, y)) = position
        let (Vector(_, clickY)) = clickPosition
        let d = clickY - y

        match raw with 
        | _ when d > 0.0f<pixel> && d <= height / 3.0f -> TurretType.Regular
        | _ when d > height / 3.0f && d <= height / 3.0f * 2.0f -> TurretType.Slow
        | _ -> TurretType.Splash

    interface IEntity with

        member _.Update(_: float32<second>) = 
            ()
           
        member _.Draw() = 
            let (Vector(x, y)) = position
            let height = height / 3.0f
            [
                Rectangle(x, y + 0.0f * height, width, height, true, Color.black); 
                Rectangle(x, y + 1.0f * height, width, height, true, Color.blue);
                Rectangle(x, y + 2.0f * height, width, height, true, Color.red)
            ] |> Shape