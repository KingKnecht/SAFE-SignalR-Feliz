namespace Shared

open System

type Todo =
    { Id : Guid
      Description : string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos : unit -> Async<Todo list>
      addTodo : Todo -> Async<Todo> }

module SignalRCom =
    [<RequireQualifiedAccess>]
    type Action =
        | IncrementCount of int
        | DecrementCount of int

    [<RequireQualifiedAccess>]
    type Response =
        | NewCount of int
        | TodoAdded of Todo

    module Endpoints =
        let [<Literal>] Root = "/socket"