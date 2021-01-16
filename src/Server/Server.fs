
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Fable.SignalR
open Saturn
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open ClientServerShared
open Storage
open ExternalHub

let storage = Storage()

storage.AddTodo(Todo.create "Learn F# basics") |> ignore
storage.AddTodo(Todo.create "Learn F# intermediate stuff") |> ignore
storage.AddTodo(Todo.create "Learn F# hardcore stuff") |> ignore
storage.AddTodo(Todo.create "See the matrix") |> ignore

let createTodosApi (distributor : IExternalHub)=
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
        fun writeRequest -> async {
            let todo = writeRequest.Payload
            match storage.AddTodo todo with
            | Ok () ->
                        do distributor.SendAllExcept writeRequest.SignalrConnectionId (SignalrCom.Response.TodoAdded(todo))
                        return todo
            | Error e -> return failwith e
        }
    }

//Used to build the Todos-REST API with HttpContext
//to be able to inject the ExternalHub into the API.
let createTodoApiFromContext (httpContext: HttpContext) : ITodosApi =
    let distributor = httpContext.GetService<IExternalHub>()
    createTodosApi distributor

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    //FromContext instead of FromValue, due ExternalHub injection
    |> Remoting.fromContext createTodoApiFromContext
    |> Remoting.buildHttpHandler

//Some extra logging which helps if socket can't be established.
let configureLogging (logging: ILoggingBuilder) =
    logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug) |> ignore
    logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug) |> ignore
    logging.SetMinimumLevel(LogLevel.Debug) |> ignore

//Register service(s), which will be resolved by the ASP.Core DI-Container.
let configureServices (services : IServiceCollection) =
    services.AddSingleton<IExternalHub, ExternalHub>()

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        //The RPC-style Hub.
        use_signalr (
            configure_signalr {
                endpoint SignalrCom.Endpoints.Root
                send SignalrRpc.send
                invoke SignalrRpc.invoke
                with_on_connected SignalrRpc.onConnected
            }
        )
        service_config configureServices
        logging configureLogging
    }

run app
