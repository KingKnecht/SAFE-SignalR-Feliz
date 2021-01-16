//Used for SignalR RPC style e.g. Counter +/-

module SignalrRpc

open FSharp.Control.Tasks.V2
open ClientServerShared
open Fable.SignalR

let update (msg: SignalrCom.Action) =
    match msg with
    | SignalrCom.Action.IncrementCount i -> SignalrCom.Response.NewCount(i + 1)
    | SignalrCom.Action.DecrementCount i -> SignalrCom.Response.NewCount(i - 1)

let invoke (msg: SignalrCom.Action) (hubContext: FableHub) =
    task { return update msg }

let send (msg: SignalrCom.Action) (hubContext: FableHub<SignalrCom.Action,SignalrCom.Response>) =
    update msg
    |> hubContext.Clients.All.Send

let onConnected (x :  FableHub<'ClientApi,SignalrCom.Response>) =
    x.Clients.Caller.Send (SignalrCom.Response.ConnectionId x.Context.ConnectionId) |> ignore
    task {()}