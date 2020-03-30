namespace Fame.Input

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

open Fame.Messaging
open Microsoft.Xna.Framework.Input

[<RequireQualifiedAccess>]
type Key =
    | Esc
    | Enter
    | None

 type KeyPresedMessage (key: Key) = 

    member _.Key = key

type KeyboardController(keys: Key list, queue: IMessageQueue) =
  
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