namespace TowerDefenseSandbox.Game.Scenes

open TowerDefenseSandbox.Game.Engine

type SceneManager() =

    let mutable createMainScreen = (fun () -> EmptyScene() :> IScene)
    let mutable createGamePlayScreen = (fun () -> EmptyScene() :> IScene)
    let mutable createGameEditScreen = (fun () -> EmptyScene() :> IScene)
    let mutable createGameSettingsScreen =(fun () -> EmptyScene() :> IScene)
    let mutable createGameVictoryScreen = (fun () -> EmptyScene() :> IScene)
    let mutable createGameOverScreen = (fun () -> EmptyScene() :> IScene)

    member val Scene : IScene = EmptyScene() :> IScene with get, set

    member _.SetMainMenu factory = createMainScreen <- factory
    member _.SetGamePlay factory = createGamePlayScreen <- factory
    member _.SetGameEdit factory = createGameEditScreen <- factory
    member _.SetGameSettings factory = createGameSettingsScreen <- factory
    member _.SetGameVictory factory = createGameVictoryScreen <- factory
    member _.SetGameOver factory = createGameOverScreen <- factory

    interface IScreenManager with
    
        member this.ToMainMenu () = this.Scene <- createMainScreen()
        member this.ToGamePlay () = this.Scene <- createGamePlayScreen()
        member this.ToGameEdit () = this.Scene <- createGameEditScreen()
        member this.ToGameSettings () = this.Scene <- createGameSettingsScreen()
        member this.ToGameVictory () = this.Scene <- createGameVictoryScreen()
        member this.ToGameOver () = this.Scene <- createGameOverScreen()

