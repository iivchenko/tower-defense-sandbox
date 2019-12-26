namespace TowerDefenseSandbox.Game.Screens

open Microsoft.Xna.Framework

type IScreen =
    abstract member Update : GameTime -> unit
    abstract member Draw : GameTime -> unit

