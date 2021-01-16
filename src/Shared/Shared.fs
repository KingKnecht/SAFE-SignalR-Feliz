module ClientServerShared

open System

type SignalrConnectionId = SignalrConnectionId of string

module SignalrConnectionId =
    let value (SignalrConnectionId id) = id

type Todo = { Id: Guid; Description: string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type WriteRequest<'T> =
    { SignalrConnectionId: SignalrConnectionId
      Payload: 'T }

module WriteRequest =
    let create (signalrConnectionId: SignalrConnectionId) (payload: 'T): WriteRequest<'T> =
        { SignalrConnectionId = signalrConnectionId
          Payload = payload }

type ITodosApi =
    { getTodos: unit -> Async<Todo list>
      addTodo: WriteRequest<Todo> -> Async<Todo> }

module SignalrCom =
    [<RequireQualifiedAccess>]
    type Action =
        | IncrementCount of int
        | DecrementCount of int

    [<RequireQualifiedAccess>]
    type Response =
        | NewCount of int
        | TodoAdded of Todo
        //Message sent by the server onConnect.
        //Used to send necessary updates initiated by (mutating) REST calls.
        //See SendAllExcept()
        | ConnectionId of string

    module Endpoints =
        [<Literal>]
        let Root = "/socket"
