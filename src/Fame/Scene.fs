﻿namespace Fame.Scene

open Microsoft.FSharp.Data.UnitSystems.SI.UnitNames

type IScene =
    abstract member Update: float32<second> -> unit
    abstract member Draw: float32<second> -> unit

type EmptyScene () =
    interface IScene with
        member _.Update (_) = ()
        member _.Draw (_) = ()

type ISceneManager =
    abstract member Scene: IScene with get, set