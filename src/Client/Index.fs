module Index

open ClientServerShared
open Elmish
open Fable.Remoting.Client
open Fable.SignalR.Elmish
open Browser
open Fable.SignalR


type Hub = Elmish.Hub<SignalrCom.Action, SignalrCom.Response>

type Model =
    { Todos: Todo list
      Input: string
      Counter: int
      Hub: Hub option
      SignalrConnectionId: ClientServerShared.SignalrConnectionId option }

type Msg =
    //REST
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    //Socket hub
    | RegisterHub of Elmish.Hub<SignalrCom.Action, SignalrCom.Response>
    | GotSignalRConnectionId of string option
    | GetConnectionId
    //Socket (RPC)
    | SignalRMsg of SignalrCom.Response
    | IncrementCount
    | IncrementAsBatch of int
    | DecrementCount

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init (): Model * Cmd<Msg> =
    let model =
        { Todos = []
          Input = ""
          Hub = None
          Counter = 0
          SignalrConnectionId = None }

    let cmd =
        Cmd.batch [
            //Connect to SignalR
            Cmd.SignalR.connect
                RegisterHub
                (fun hub ->
                    hub
                        .withUrl(SignalrCom.Endpoints.Root)
                        .withAutomaticReconnect()
                        .configureLogging(Fable.SignalR.LogLevel.Debug)
                        .onMessage SignalRMsg)

            //get some data via REST
            Cmd.OfAsync.perform todosApi.getTodos () GotTodos
        ]

    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        match model.SignalrConnectionId with
        | None -> model, Cmd.none
        | Some id ->
            let writeRequest =
                Todo.create model.Input |> WriteRequest.create id

            { model with Input = "" }, Cmd.OfAsync.perform todosApi.addTodo writeRequest AddedTodo
    | AddedTodo todo ->
        { model with
              Todos = model.Todos @ [ todo ] },
        Cmd.none
    | RegisterHub hub -> { model with Hub = Some hub }, Cmd.none
    | SignalRMsg response ->
        match response with
        | SignalrCom.Response.NewCount count -> { model with Counter = count }, Cmd.none
        | SignalrCom.Response.TodoAdded todo ->
            { model with
                  Todos = model.Todos @ [ todo ] },
            Cmd.none
        | SignalrCom.Response.ConnectionId id ->
            console.log (printfn "SignalRCom.Response.ConnectionId: %s" id)

            { model with
                  SignalrConnectionId = Some(SignalrConnectionId id) },
            Cmd.none

    | IncrementCount -> model, Cmd.SignalR.send model.Hub (SignalrCom.Action.IncrementCount model.Counter)
    | IncrementAsBatch amount ->
        let cmds =
            seq { 1 .. (amount - 1) }
            |> Seq.map (fun i -> Cmd.SignalR.send model.Hub (SignalrCom.Action.IncrementCount(model.Counter + i)))
            |> Cmd.batch

        model, cmds
    | DecrementCount -> model, Cmd.SignalR.send model.Hub (SignalrCom.Action.DecrementCount model.Counter)
    | GotSignalRConnectionId connectionIdOpt ->
        console.log (printfn "GotSignalRConnectionID: %A" (Option.map SignalrConnectionId connectionIdOpt))

        { model with
              SignalrConnectionId = Option.map SignalrConnectionId connectionIdOpt },
        Cmd.none
    | GetConnectionId -> model, Cmd.SignalR.connectionId model.Hub GotSignalRConnectionId

open Feliz
open Feliz.Bulma
open Zanaptak.TypedCssClasses

type Bcss = CssClasses<"https://cdn.jsdelivr.net/npm/bulma@0.9.0/css/bulma.min.css", Naming.PascalCase>
type FA = CssClasses<"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.14.0/css/all.min.css", Naming.PascalCase>

let navBrand =
    Bulma.navbarBrand.div [
        prop.children [
            Bulma.navbarItem.a [
                navbarItem.isExpanded
                prop.href "https://safe-stack.github.io/"
                prop.classes [
                    Bcss.IsPrimary
                    Bcss.IsActive
                ]
                prop.children [
                    Html.img [
                        prop.src "/favicon.png"
                        prop.alt "Logo"
                    ]
                ]
            ]
        ]
    ]

let containerBoxTodos (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [

        Bulma.content [
            Html.orderedList [
                for todo in model.Todos do
                    Html.listItem [
                        Html.text todo.Description
                    ]
            ]
        ]
        Bulma.field.div [
            prop.classes [ Bcss.IsGrouped ]
            prop.children [
                Bulma.control.p [
                    prop.classes [ Bcss.IsExpanded ]
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Input
                            prop.placeholder "What needs to be done?"
                            prop.onChange (SetInput >> dispatch)
                        ]
                    ]
                ]

                Bulma.control.p [
                    Bulma.button.a [
                        prop.classes [ Bcss.IsPrimary ]
                        prop.disabled (Todo.isValid model.Input |> not)
                        prop.onClick (fun _ -> dispatch AddTodo)
                        prop.text "Add"
                    ]
                ]
            ]
        ]
    ]

let containerBoxCounter (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [

        Bulma.field.div [
            prop.children [
                Bulma.content [
                    Bulma.title [
                        title.is1
                        prop.style [ style.textAlign.center ]
                        prop.text model.Counter
                    ]
                ]

                Bulma.field.div [
                    prop.classes [
                        Bcss.IsGrouped
                        Bcss.IsGroupedCentered
                    ]
                    prop.children [
                        Bulma.control.p [
                            Bulma.button.a [
                                prop.classes [ Bcss.IsPrimary ]
                                prop.onClick (fun _ -> dispatch IncrementCount)
                                prop.text "Increment"
                                prop.children [
                                    Html.i [
                                        prop.classes [ FA.Fa; FA.FaPlus ]
                                    ]
                                ]
                            ]
                        ]

                        Bulma.control.p [
                            Bulma.button.a [
                                prop.classes [ Bcss.IsPrimary ]
                                prop.onClick (fun _ -> dispatch DecrementCount)
                                prop.text "Decrement"
                                prop.children [
                                    Html.i [
                                        prop.classes [ FA.Fa; FA.FaMinus ]
                                    ]
                                ]
                            ]
                        ]

                        Bulma.control.p [
                            Bulma.button.a [
                                prop.classes [ Bcss.IsPrimary ]
                                let amount = 500
                                prop.onClick (fun _ -> dispatch (IncrementAsBatch amount))
                                prop.text "IncrementAmount"

                                prop.children [
                                    Html.span [
                                        prop.classes [ Bcss.Icon; Bcss.IsSmall ]
                                        prop.children [
                                            Html.i [
                                                prop.classes [ FA.Fa; FA.FaTimes ]
                                            ]
                                        ]
                                    ]
                                    Html.span [ Html.text amount ]
                                ]
                            ]
                        ]
                    ]
                ]

            ]

        ]

    ]

let containerBoxCounterReact (model: Model) (dispatch: Msg -> unit) =
    let count, setCount = React.useState 0

    let hub =
        React.useSignalR<SignalrCom.Action, SignalrCom.Response>
            (fun hub ->
                hub
                    .withUrl(SignalrCom.Endpoints.Root)
                    .withAutomaticReconnect()
                    .configureLogging(Fable.SignalR.LogLevel.Debug)
                    .onMessage
                <| function
                | SignalrCom.Response.NewCount i -> setCount i
                | _ -> () |> ignore)

    Bulma.box [
        Bulma.field.div [
            prop.children [
                Bulma.content [
                    Bulma.title [
                        title.is1
                        prop.style [ style.textAlign.center ]
                        prop.text model.Counter
                    ]
                ]

                Bulma.field.div [
                    prop.classes [
                        Bcss.IsGrouped
                        Bcss.IsGroupedCentered
                    ]
                    prop.children [
                        Bulma.control.p [
                            Bulma.button.a [
                                prop.classes [ Bcss.IsPrimary ]
                                prop.onClick
                                <| fun _ -> hub.current.sendNow (SignalrCom.Action.IncrementCount count)
                                prop.text "Increment"
                                prop.children [
                                    Html.i [
                                        prop.classes [ FA.Fa; FA.FaPlus ]
                                    ]
                                ]
                            ]
                        ]

                        Bulma.control.p [
                            Bulma.button.a [
                                prop.classes [ Bcss.IsPrimary ]
                                prop.onClick
                                <| fun _ -> hub.current.sendNow (SignalrCom.Action.DecrementCount count)
                                prop.text "Decrement"
                                prop.children [
                                    Html.i [
                                        prop.classes [ FA.Fa; FA.FaMinus ]
                                    ]
                                ]
                            ]
                        ]

                        Bulma.control.p [
                            Bulma.button.a [
                                prop.classes [ Bcss.IsPrimary ]
                                let amount = 500

                                prop.onClick
                                <| fun _ -> hub.current.sendNow (SignalrCom.Action.IncrementCount amount)

                                prop.text "IncrementAmount"

                                prop.children [
                                    Html.span [
                                        prop.classes [ Bcss.Icon; Bcss.IsSmall ]
                                        prop.children [
                                            Html.i [
                                                prop.classes [ FA.Fa; FA.FaTimes ]
                                            ]
                                        ]
                                    ]
                                    Html.span [ Html.text amount ]
                                ]
                            ]
                        ]
                    ]
                ]

            ]

        ]

    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        prop.style [
            style.backgroundSize.cover
            style.backgroundRepeat.noRepeat
            style.backgroundPosition.fixedNoScroll
        ]
        prop.classes [
            Bcss.IsFullheight
            "bg-gradient"
        ]
        prop.children [
            Bulma.heroHead [
                Bulma.navbar [
                    prop.classes [ Bcss.IsPrimary ]
                    prop.children [
                        Bulma.container [ navBrand ]
                    ]
                ]
            ]

            Bulma.heroBody [
                prop.children [
                    Bulma.container [
                        prop.classes [ Bcss.IsFullwidth ]
                        prop.children [
                            // Bulma.button.a [
                            //     prop.text "GetConnId"
                            //     prop.onClick (fun _ -> dispatch GetConnectionId)
                            // ]
                            Bulma.columns [
                                prop.children [
                                    Bulma.column [
                                        prop.classes [ Bcss.IsOneThird ]
                                        prop.children [
                                            Bulma.title [
                                                prop.style [
                                                    style.textAlign.center
                                                    style.color.white
                                                ]
                                                prop.classes [ Bcss.IsSize2 ]
                                                prop.children [
                                                    Html.text "Elmish REST + Socket"
                                                ]
                                            ]
                                            containerBoxTodos model dispatch
                                        ]
                                    ]

                                    Bulma.column [
                                        prop.classes [ Bcss.IsOneThird ]
                                        prop.children [
                                            Bulma.title [
                                                prop.style [
                                                    style.textAlign.center
                                                    style.color.white
                                                ]
                                                prop.classes [ Bcss.IsSize1 ]
                                                prop.children [
                                                    Html.text "Elmish socket"
                                                ]
                                            ]
                                            containerBoxCounter model dispatch
                                        ]
                                    ]

                                    Bulma.column [
                                        prop.classes [ Bcss.IsOneThird ]
                                        prop.children [
                                            Bulma.title [
                                                prop.style [
                                                    style.textAlign.center
                                                    style.color.white
                                                ]
                                                prop.classes [ Bcss.IsSize1 ]
                                                prop.children [
                                                    Html.text "Feliz/React socket"
                                                ]
                                            ]
                                            containerBoxCounter model dispatch
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
