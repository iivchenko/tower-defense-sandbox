namespace TowerDefenseSandbox.Game.Engine

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Microsoft.Xna.Framework

type IScreen =
    abstract member Update: float32<second> -> unit
    abstract member Draw: float32<second> -> unit

type EmptyScreen () =
    interface IScreen with
        member _.Update (_) = ()
        member _.Draw (_) = ()

type ScreenManager() =
    let mutable previousScreen: IScreen option = None
    let mutable currentScreen: IScreen option = None

    member _.Screen 
        with get () = 
            match currentScreen with
            | Some screen -> screen
            | None -> EmptyScreen() :> IScreen

    member _.Next (screen: IScreen) = 
        previousScreen <- currentScreen
        currentScreen <- Some screen

    member _.Back() = 
        currentScreen <- previousScreen
        previousScreen <- None