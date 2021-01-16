module App

open Elmish
open Elmish.React

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

open Fable.Core.JsInterop
open Thoth.Json
open Browser

importSideEffects "./styles/main.scss"


// let HubDecoder =
//     Decode.succeed (Unchecked.defaultof<Index.Hub>)

// let HubEncoder ( h : Index.Hub)=
//     Encode.string "Hub encoding not possible"


// let extra =
//     Extra.empty
//     |> Extra.withCustom HubEncoder HubDecoder


// let modelDecoder =
//     Decode.Auto.generateDecoder<Index.Model>(extra = extra)

// let modelEncoder =
//     Encode.Auto.generateEncoder<Index.Hub>(extra = extra)

Program.mkProgram Index.init Index.update Index.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
//|> Program.withDebuggerCoders (fun m -> modelEncoder m.Hub.Value)  modelDecoder
//|> Program.withDebugger
#endif
|> Program.run
