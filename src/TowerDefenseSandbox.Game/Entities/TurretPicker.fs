namespace TowerDefenseSandbox.Game.Entities

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open MonoGame.Extended

open TowerDefenseSandbox.Engine
open TowerDefenseSandbox.Game.Engine

type TurretPicker (zindex: int, draw: Shape -> unit, parent: Grid, entityProvider: IEntityProvider, x: int, y: int) =

    member this.Click (cellPosition: RectangleF, clickPosition: Vector) = 
        let (Vector(_, clickY)) = clickPosition
        let d = clickY - cellPosition.Position.Y

        match y with 
        | _ when d > 0.0f && d <= cellPosition.Size.Height / 3.0f -> 
            parent.[x, y] <- Turret.CreateRegular(draw, entityProvider) :> ICell |> Some
        | _ when d > cellPosition.Size.Height / 3.0f && d <= cellPosition.Size.Height / 3.0f * 2.0f -> 
            parent.[x, y] <- Turret.CreateSlow(draw, entityProvider) :> ICell |> Some
        | _ ->
             parent.[x, y] <- Turret.CreateSplash(draw, entityProvider) :> ICell |> Some 

    interface ICell with

        member _.ZIndex = zindex 

        member _.Update(time: float32<second>) (position: RectangleF) = 
            ()
        
        member _.Draw(time: float32<second>) (position: RectangleF) =
            
            let x = position.X
            let y = position.Y
            let width = position.Size.Width
            let height = position.Size.Height / 3.0f

            Rectangle(x, y + 0.0f * height, width, height, true, Color.black) |> draw
            Rectangle(x, y + 1.0f * height, width, height, true, Color.blue) |> draw
            Rectangle(x, y + 2.0f * height, width, height, true, Color.red) |> draw
