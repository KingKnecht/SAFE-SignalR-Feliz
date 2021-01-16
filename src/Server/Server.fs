
open Shared
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Fable.SignalR
open Saturn
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http

//REST -> Socket updates
type IDistributor =
    abstract Send : SignalRCom.Response -> unit

type Distributor (hub: FableHubCaller<SignalRCom.Action,SignalRCom.Response>) =
    let hub = hub
    interface IDistributor with
        member this.Send msg = hub.Clients.All.Send msg |> ignore

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

let createTodosApi (distributor : IDistributor)=
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
        fun todo -> async {
            match storage.AddTodo todo with
            | Ok () ->
                        do distributor.Send (SignalRCom.Response.TodoAdded(todo))
                        return todo
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
        |> hubContext.Clients.All.Send

let createTodoApiFromContext (httpContext: HttpContext) : Shared.ITodosApi =
    let distributor = httpContext.GetService<IDistributor>()
    createTodosApi distributor

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromContext createTodoApiFromContext
    |> Remoting.buildHttpHandler

//Some extra logging which helps if socket can't be established.
let configureLogging (logging: ILoggingBuilder) =
    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug) |> ignore
    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug) |> ignore
    logging.SetMinimumLevel(LogLevel.Debug) |> ignore

let configureServices (services : IServiceCollection) =
    services.AddSingleton<IDistributor, Distributor>()

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
        service_config configureServices
        logging configureLogging
    }

run app
