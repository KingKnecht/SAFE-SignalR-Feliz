module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Fable.SignalR.Elmish


type Model =
    { Todos: Todo list
      Input: string
      Counter: int
      Hub: Elmish.Hub<SignalRCom.Action, SignalRCom.Response> option }

type Msg =
    //REST
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo
    //Socket hub
    | RegisterHub of Elmish.Hub<SignalRCom.Action, SignalRCom.Response>
    //Socket (RPC)
    | SignalRMsg of SignalRCom.Response
    | IncrementCount
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
          Counter = 0 }

    let cmd =
        Cmd.batch [
            Cmd.SignalR.connect
                RegisterHub
                (fun hub ->
                    hub
                        .withUrl(SignalRCom.Endpoints.Root)
                        .withAutomaticReconnect()
                        .configureLogging(Fable.SignalR.LogLevel.Debug)
                        .onMessage SignalRMsg)
            Cmd.OfAsync.perform todosApi.getTodos () GotTodos
        ]

    model, cmd

let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd =
            Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | AddedTodo todo ->
        { model with
              Todos = model.Todos @ [ todo ] },
        Cmd.none
    | RegisterHub hub -> { model with Hub = Some hub }, Cmd.none
    | SignalRMsg response ->
        match response with
        | SignalRCom.Response.NewCount count -> { model with Counter = count }, Cmd.none
    | IncrementCount ->
        let cmds =
            seq { 0 .. 9999 }
            |> Seq.map (fun i -> Cmd.SignalR.send model.Hub (SignalRCom.Action.IncrementCount(model.Counter + i)))
            |> Cmd.batch
        //model, Cmd.SignalR.send model.Hub (SignalRCom.Action.IncrementCount model.Counter)
        model, cmds

    | DecrementCount -> model, Cmd.SignalR.send model.Hub (SignalRCom.Action.DecrementCount model.Counter)

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
                            ]
                        ]

                        Bulma.control.p [
                            Bulma.button.a [
                                prop.classes [ Bcss.IsPrimary ]
                                prop.onClick (fun _ -> dispatch DecrementCount)
                                prop.text "Decrement"
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
                            Bulma.columns [
                                prop.children [
                                    Bulma.column [
                                        prop.classes [ Bcss.IsHalf ]
                                        prop.children [
                                            Bulma.title [
                                                prop.style [ style.textAlign.center ]
                                                prop.classes [ Bcss.IsSize1 ]
                                                prop.children [
                                                    Html.text "REST + Socket"
                                                ]
                                            ]
                                            containerBoxTodos model dispatch
                                        ]
                                    ]

                                    Bulma.column [
                                        prop.classes [ Bcss.IsHalf ]
                                        prop.children [
                                            Bulma.title [
                                                prop.style [ style.textAlign.center ]
                                                prop.classes [ Bcss.IsSize1 ]
                                                prop.children [ Html.text "Socket" ]
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
