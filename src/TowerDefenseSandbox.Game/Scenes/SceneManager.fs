namespace TowerDefenseSandbox.Game.Scenes

open TowerDefenseSandbox.Game.Engine

type SceneManager() =

    let mutable screen = EmptyScreen() :> IScreen

    let mutable createMainScreen = (fun () -> EmptyScreen() :> IScreen)
    let mutable createGamePlayScreen = (fun () -> EmptyScreen() :> IScreen)
    let mutable createGameEditScreen = (fun () -> EmptyScreen() :> IScreen)
    let mutable createGameSettingsScreen = (fun () -> EmptyScreen() :> IScreen)
    let mutable createGameVictoryScreen = (fun () -> EmptyScreen() :> IScreen)
    let mutable createGameOverScreen = (fun () -> EmptyScreen() :> IScreen)

    member _.Screen with get() = screen and set value = screen <-value

    member val Scene : IScene = EmptyScene() :> IScene with get, set

    member _.SetMainMenu factory = createMainScreen <- factory
    member _.SetGamePlay factory = createGamePlayScreen <- factory
    member _.SetGameEdit factory = createGameEditScreen <- factory
    member _.SetGameSettings factory = createGameSettingsScreen <- factory
    member _.SetGameVictory factory = createGameVictoryScreen <- factory
    member _.SetGameOver factory = createGameOverScreen <- factory

    interface IScreenManager with
    
        member _.ToMainMenu () = screen <- createMainScreen()
        member _.ToGamePlay () = screen <- createGamePlayScreen()
        member _.ToGameEdit () = screen <- createGameEditScreen()
        member _.ToGameSettings () = screen <- createGameSettingsScreen()
        member _.ToGameVictory () = screen <- createGameVictoryScreen()
        member _.ToGameOver () = screen <- createGameOverScreen()

