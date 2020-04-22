namespace Fame.Input

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input

open Fame
open System
open Microsoft.Xna.Framework.Input.Touch

type IInputController =
    abstract Update: float32<second> -> unit

type AggregatedInputController (inputs: IInputController list) =
    interface IInputController with 
        member _.Update (delta: float32<second>) =
            inputs |> List.iter (fun x -> x.Update delta)

[<RequireQualifiedAccess>]
type MouseButtonState = 
    | Released
    | Pressed

type MouseState =
    {
        Position: Vector<pixel>
        LeftButton: MouseButtonState
    }

type TouchLocationState =
    | Pressed 
    | Moved
    | Released

type TouchLocation =
    { Id: int
      State: TouchLocationState
      X: float32<pixel>
      Y: float32<pixel> }

module MouseInput =
   
    let private toVector (point: Point) = Vector((float32 point.X) * 1.0f<pixel>, (float32 point.Y) * 1.0f<pixel>)
    let private toButtonState (button: ButtonState) = 
        match button with 
        | ButtonState.Released -> MouseButtonState.Released
        | ButtonState.Pressed -> MouseButtonState.Pressed
        | _ -> raise (new ArgumentException((sprintf "Unknown MonoGame mouser button state: %s" (button.ToString()))))

    let state () = 
        let state = Mouse.GetState()
        { Position = toVector state.Position; LeftButton = toButtonState state.LeftButton }

module TouchInput =
    let private toTouchLocationState state = 
           match state with 
           | Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Pressed -> TouchLocationState.Pressed
           | Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Moved -> TouchLocationState.Moved
           | Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Released -> TouchLocationState.Released
           | _ -> raise (new ArgumentException((sprintf "Unknown MonoGame touch location state: %s" (state.ToString()))))

    let state () = TouchPanel.GetState() |> Seq.map (fun touch -> { Id = touch.Id; State = (toTouchLocationState touch.State); X = touch.Position.X * 1.0f<pixel>; Y = touch.Position.Y * 1.0f<pixel> }) |> Seq.toList