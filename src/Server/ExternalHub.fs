module ExternalHub

open ClientServerShared
open Fable.SignalR

//A hub which is available outside the webserver world
//Used e.g. to send updates after mutable REST API calls.
type IExternalHub =
    abstract SendAll : SignalrCom.Response -> unit
    abstract SendAllExcept : ClientServerShared.SignalrConnectionId -> SignalrCom.Response -> unit

type ExternalHub (hub: FableHubCaller<SignalrCom.Action,SignalrCom.Response>) =
    let hub = hub
    interface IExternalHub with
        member __.SendAll msg = hub.Clients.All.Send msg |> ignore
        member __.SendAllExcept id msg =
            hub.Clients.AllExcept([(SignalrConnectionId.value id)]).Send msg |> ignore
