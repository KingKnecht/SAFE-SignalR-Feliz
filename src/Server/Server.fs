
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Fable.SignalR
open Saturn
open Microsoft.Extensions.Logging

type Storage () =
    let todos = ResizeArray<_>()

    member __.GetTodos () =
        List.ofSeq todos

    member __.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok ()
        else Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new SAFE project") |> ignore
storage.AddTodo(Todo.create "Write your app") |> ignore
storage.AddTodo(Todo.create "Ship it !!!") |> ignore

let todosApi =
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
        fun todo -> async {
            match storage.AddTodo todo with
            | Ok () -> return todo
            | Error e -> return failwith e
        } }

//SignalR RPC style.
module SignalRRpc =
    open FSharp.Control.Tasks.V2

    let update (msg: SignalRCom.Action) =
        match msg with
        | SignalRCom.Action.IncrementCount i -> SignalRCom.Response.NewCount(i + 1)
        | SignalRCom.Action.DecrementCount i -> SignalRCom.Response.NewCount(i - 1)

    let invoke (msg: SignalRCom.Action) (hubContext: FableHub) =
        task { return update msg }

    let send (msg: SignalRCom.Action) (hubContext: FableHub<SignalRCom.Action,SignalRCom.Response>) =
        update msg
        |> hubContext.Clients.Caller.Send

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue todosApi
    |> Remoting.buildHttpHandler

//Some extra logging which helps if socket can't be established.
let configureLogging (logging: ILoggingBuilder) =
    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Trace) |> ignore
    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Trace) |> ignore
    logging.SetMinimumLevel(LogLevel.Trace) |> ignore

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        use_signalr (
            configure_signalr {
                endpoint SignalRCom.Endpoints.Root
                send SignalRRpc.send
                invoke SignalRRpc.invoke
            }
        )
        logging configureLogging
    }

run app
