namespace TowerDefenseSandbox.Game.Entities

open TowerDefenseSandbox.Game.Engine
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open MonoGame.Extended

type Road (spriteBatch : SpriteBatch, zindex : int) = 
    
    interface ICell with 

        member _.ZIndex = zindex
            
        member _.Update (gameTime : GameTime) (position : RectangleF) =
            ()
            
        member _.Draw (gameTime : GameTime) (position : RectangleF) =
            spriteBatch.FillRectangle(position, Color.Gray)

