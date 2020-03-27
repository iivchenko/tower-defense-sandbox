open TowerDefenseSandbox.Game.Windows

[<EntryPoint>]
let main _ =
    let game = new TheGame()
    game.Run()

    0