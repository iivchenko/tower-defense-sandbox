namespace TowerDefenseSandbox.Game.Scenes

type IScreenManager =
    abstract ToMainMenu: unit -> unit
    abstract ToGamePlay: unit -> unit
    abstract ToGameEdit: unit -> unit
    abstract ToGameSettings: unit -> unit
    abstract ToGameVictory: unit -> unit
    abstract ToGameOver: unit -> unit
