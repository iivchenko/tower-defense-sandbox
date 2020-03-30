module TowerDefenseSandbox.Engine.MonoGame.File

open Microsoft.Xna.Framework

let read file = TitleContainer.OpenStream(file)

