namespace TowerDefenseSandbox.Game.Screens

type IScreenManager =
    abstract ToMainMenu: unit -> unit
    abstract ToGamePlay: unit -> unit
    abstract ToGameEdit: unit -> unit
    abstract ToGameSettings: unit -> unit
    abstract ToGameOver: unit -> unit
