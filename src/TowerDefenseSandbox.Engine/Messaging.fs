namespace TowerDefenseSandbox.Engine.Messaging

open System
open System.Collections.Generic

type IMessageQueue =
    abstract Push<'TMessage> : 'TMessage -> unit

type IMessageHandler = interface end

type IMessageHandler<'TMessage> =
    inherit IMessageHandler
    abstract Handle: 'TMessage -> unit

type IMessageHandlerRegister =
    abstract Register<'TMessage> : IMessageHandler<'TMessage> -> unit

type MessageBus () =

    let handlers = Dictionary<Type, IMessageHandler list>()
    
    interface IMessageQueue with 
        
        member _.Push<'TMessage> (message: 'TMessage) =

            match handlers.TryGetValue(typeof<'TMessage>) with 
            | (true, handlers) -> 
                handlers |> List.iter (fun x -> (x :?> IMessageHandler<'TMessage>).Handle message)
            | _ -> ()

    interface IMessageHandlerRegister with
        
        member _.Register<'TMessage> (handler: IMessageHandler<'TMessage>) =
            if handlers.ContainsKey(typeof<'TMessage>) |> not
                then
                    handlers.Add(typeof<'TMessage>, [])
                else ()

            handlers.[typeof<'TMessage>] <- (handler :> IMessageHandler) :: handlers.[typeof<'TMessage>]