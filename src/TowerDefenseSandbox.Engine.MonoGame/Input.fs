namespace TowerDefenseSandbox.Engine.MonoGame.Input

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework.Input

open TowerDefenseSandbox.Engine.Messaging
open TowerDefenseSandbox.Engine.Input

 type MonoGameMouseInputController(queue: IMessageQueue) =

    let mutable buttons = [ (Button.Left, ButtonState.Released); (Button.Middle, ButtonState.Released); (Button.Right, ButtonState.Released) ] |> Map.ofList
    interface IInputController with 
        member _.Update (_: float32<second>) =
            let state = Mouse.GetState()
            let buttons' = [ (Button.Left, buttons.[Button.Left], state.LeftButton); (Button.Middle, buttons.[Button.Middle], state.MiddleButton); (Button.Right, buttons.[Button.Right], state.RightButton) ]
            buttons <- buttons' |> List.map (fun (key, _, state'') -> key, state'') |> Map.ofList
            
            buttons' |> List.iter(fun (button, state', state'') -> if state' = ButtonState.Pressed && state'' = ButtonState.Released then queue.Push(MouseButtonPressedMessage(button, state.X, state.Y)) else ())

type MonoGameKeyboardInputController(keys: Key list, queue: IMessageQueue) =
   
    let map key = 
        match key with
        | Key.Esc -> Keys.Escape
        | Key.Enter -> Keys.Enter
        | _ -> Keys.None

    let asKey key = 
        match key with 
        | Keys.Escape -> Key.Esc
        | Keys.Enter -> Key.Enter
    let mutable keys = keys |> List.map map |> List.map (fun key -> (key, KeyState.Up))
        
    interface IInputController with
    
        member _.Update (_: float32<second>) =
            let keyboard = Keyboard.GetState()
            
            let tmp = keys |> List.map (fun (key, state) -> key, state, keyboard.Item(key))
            keys <- tmp |> List.map (fun (key, _, state'') -> key, state'')

            tmp |> List.iter (fun (key, state', state'') -> if state' <> state'' && state'' = KeyState.Up then queue.Push(KeyPresedMessage(asKey key)) else ())