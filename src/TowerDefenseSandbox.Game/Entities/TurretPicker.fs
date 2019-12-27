namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Game.Engine
open Microsoft.Xna.Framework
open MonoGame.Extended
open Microsoft.Xna.Framework.Graphics

type TurretPicker (zindex : int, spriteBatch : SpriteBatch, parent : Grid, entityProvider : IEntityProvider, x : int, y : int) =

    member this.Click (cellPosition : RectangleF, clickPosition : Vector2) = 
        let d = clickPosition.Y - cellPosition.Position.Y

        match y with 
        | _ when d > 0.0f && d <= cellPosition.Size.Height / 3.0f -> parent.[x, y] <- RegularTurret (1, spriteBatch, entityProvider) :> ICell |> Some
        | _ when d > cellPosition.Size.Height / 3.0f && d <= cellPosition.Size.Height / 3.0f * 2.0f -> parent.[x, y] <- SlowTurret (1, spriteBatch, entityProvider) :> ICell |> Some
        | _ -> parent.[x, y] <- SplashTurret (1, spriteBatch, entityProvider) :> ICell |> Some

    interface ICell with

        member _.ZIndex = zindex 

        member _.Update(gameTime : GameTime) (position : RectangleF) = 
            ()
        
        member _.Draw(gameTime : GameTime) (position : RectangleF) =

            let size = Size2(position.Size.Width, position.Size.Height / 3.0f)
            let rec1 = RectangleF(position.Position, size)
            let rec2 = RectangleF(rec1.Position.X, rec1.Position.Y + size.Height, size.Width, size.Height)
            let rec3 = RectangleF(rec2.Position.X, rec2.Position.Y + size.Height, size.Width, size.Height)
            spriteBatch.FillRectangle(rec1, Color.Black)
            spriteBatch.FillRectangle(rec2, Color.Blue)
            spriteBatch.FillRectangle(rec3, Color.Red)

        
