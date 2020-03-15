namespace TowerDefenseSandbox.Game.Scenes

open TowerDefenseSandbox.Game.Engine

type SceneManager() =

    member val public Scene : IScene = EmptyScene() :> IScene with get, set