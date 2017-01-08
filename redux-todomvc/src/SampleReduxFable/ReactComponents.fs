namespace SampleReduxFable



module ReactComponents = 
  open Fable.Core
  open Fable.Core.JsInterop
  open Fable.Helpers.React
  open Fable.Import
  open Domain
  

  module R = Fable.Helpers.React
  open R.Props

  [<Pojo>]
  type TodoTextInputProps =
      { OnSave: string->unit
      ; Text: string option
      ; Placeholder: string
      ; Editing: bool
      ; NewTodo: bool }

  [<Pojo>]
  type TodoTextInputState = { Text: string }

  type TodoTextInput(props) =
      inherit React.Component<TodoTextInputProps, TodoTextInputState>(props)
      do base.setInitState({ Text = defaultArg props.Text "" })

      member this.HandleSubmit(e: React.KeyboardEvent) =
          if e.which = ENTER_KEY then
              let text = (unbox<string> e.target?value).Trim()
              this.props.OnSave(text)
              if this.props.NewTodo then
                  this.setState({ Text = "" })

      member this.HandleChange(e: React.SyntheticEvent) =
          this.setState({ Text=unbox e.target?value })

      member this.HandleBlur(e: React.SyntheticEvent) =
          if not this.props.NewTodo then
              this.props.OnSave(unbox e.target?value)

      member this.render() =
          R.input [
              ClassName(
                  classNames [
                      "edit", this.props.Editing
                      "new-todo", this.props.NewTodo
                  ])
              Type "text"
              OnBlur this.HandleBlur
              OnChange this.HandleChange
              OnKeyDown this.HandleSubmit
              AutoFocus (this.state.Text.Length > 0)
              Placeholder this.props.Placeholder
          ] []

  [<Pojo>]
  type TodoItemProps =
      { Todo: Todo
      ; EditTodo: int * string -> unit
      ; DeleteTodo: int -> unit
      ; CompleteTodo: int -> unit }

  [<Pojo>]
  type TodoItemState = { Editing: bool }

  type TodoItem(props) =
      inherit React.Component<TodoItemProps, TodoItemState>(props)
      do base.setInitState({ Editing = false })

      member this.HandleDoubleClick(_) =
          this.setState({ Editing = true })

      member this.HandleSave(id, text: string) =
          if text.Length = 0
          then this.props.DeleteTodo(id)
          else this.props.EditTodo(id, text)
          this.setState({ Editing = false })

      member this.render() =
          let element =
              if this.state.Editing
              then R.com<TodoTextInput,_,_>
                      { OnSave = fun (text: string) ->
                          this.HandleSave(this.props.Todo.Id, text)
                      ; Editing = this.state.Editing
                      ; Text = Some this.props.Todo.Text
                      ; Placeholder = ""
                      ; NewTodo = false } []
              else R.div [ClassName "view"] [
                      R.input [
                          ClassName "toggle"
                          Type "checkbox"
                          Checked this.props.Todo.Completed
                          OnChange (fun _ ->
                              this.props.CompleteTodo(this.props.Todo.Id))
                      ] []
                      R.label [OnDoubleClick this.HandleDoubleClick]
                              [R.str this.props.Todo.Text]
                      R.div [
                          ClassName "destroy"
                          OnClick (fun _ ->
                              this.props.DeleteTodo(this.props.Todo.Id))
                      ] []
                  ]
          R.li [ClassName(
                  classNames [
                      "completed", this.props.Todo.Completed
                      "editing", this.state.Editing])]
              [element]

  [<Pojo>]
  type HeaderProps = { AddTodo: string->unit }

  let Header (props: HeaderProps) =
      R.header [ClassName "header"] [
          R.h1 [] [R.str "todos"]
          R.com<TodoTextInput,_,_>
              { OnSave = fun (text: string) ->
                  if text.Length <> 0 then
                      props.AddTodo text
              ; Placeholder = "What needs to be done?"
              ; NewTodo = true
              ; Text = None
              ; Editing = false } []
      ]

  [<Pojo>]
  type FooterProps =
      { ActiveCount: int
      ; CompletedCount: int
      ; Filter: TodoFilter
      ; OnShow: TodoFilter->unit
      ; OnClearCompleted: React.MouseEvent->unit }

  let Footer =
      let filterTitles =
          dict [
              TodoFilter.ShowAll, "All"
              TodoFilter.ShowActive, "Active"
              TodoFilter.ShowCompleted, "Completed"
          ]
      let renderTodoCount activeCount =
          R.span [ClassName "todo-count"] [
              R.str(sprintf "%s item%s left"
                  (if activeCount > 0 then string activeCount else "No")
                  (if activeCount <> 1 then "s" else ""))
          ]
      let renderFilterLink filter selectedFilter onShow =
          R.a [
              ClassName (classNames ["selected", filter = selectedFilter])
              Style [unbox("cursor", "pointer")]
              OnClick (fun _ -> onShow filter)
          ] [R.str filterTitles.[filter]]
      let renderClearButton completedCount onClearCompleted =
          if completedCount > 0
          then R.button [
                  ClassName "clear-completed"
                  OnClick onClearCompleted
                ] [ R.str "Clear completed" ] |> Some
          else None
      fun (props: FooterProps) ->
          let listItems =
              [ TodoFilter.ShowAll
                TodoFilter.ShowActive
                TodoFilter.ShowCompleted ]
              |> List.map (fun filter ->
                  [renderFilterLink filter props.Filter props.OnShow]
                  |> R.li [Key (string filter)])
          R.footer [ClassName "footer"] [
              renderTodoCount props.ActiveCount
              R.ul [ClassName "filters"] listItems
              R.opt(renderClearButton props.CompletedCount props.OnClearCompleted)
          ]

  type [<Pojo>] MainSectionProps = { Todos: Todo[]; Dispatch: TodoAction->unit }
  type [<Pojo>] MainSectionState = { Filter: TodoFilter }

  type MainSection(props) =
      inherit React.Component<MainSectionProps, MainSectionState>(props)
      let todoFilters =
          dict [
              TodoFilter.ShowAll, fun _ -> true
              TodoFilter.ShowActive, fun (todo: Todo) -> not todo.Completed
              TodoFilter.ShowCompleted, fun todo -> todo.Completed
          ]
      do base.setInitState({ Filter = TodoFilter.ShowAll })

      member this.HandleClearCompleted() =
          this.props.Dispatch(ClearCompleted)

      member this.HandleShow(filter) =
          this.setState({ Filter = filter })

      member this.renderToggleAll(completedCount) =
          if this.props.Todos.Length > 0
          then R.input [
                  ClassName "toggle-all"
                  Type "checkbox"
                  Checked (completedCount = this.props.Todos.Length)
                  OnChange (fun _ -> this.props.Dispatch(CompleteAll))
                ] [] |> Some
          else None

      member this.renderFooter(completedCount) =
          if this.props.Todos.Length > 0
          then R.fn Footer
                  { ActiveCount = this.props.Todos.Length - completedCount
                  ; CompletedCount = completedCount
                  ; Filter = this.state.Filter
                  ; OnShow = fun filter ->
                      this.HandleShow filter
                  ; OnClearCompleted = fun _ ->
                      this.HandleClearCompleted() } [] |> Some
          else None

      member this.render() =
          let filteredTodos =
              this.props.Todos
              |> Array.filter todoFilters.[this.state.Filter]
              |> Array.toList
          let completedCount =
              (0, this.props.Todos) ||> Array.fold (fun count todo ->
                  if todo.Completed then count + 1 else count)
          R.section [ClassName "main"] [
              R.opt(this.renderToggleAll completedCount)
              R.ul [ClassName "todo-list"]
                  (filteredTodos
                  |> List.map (fun todo ->
                      R.com<TodoItem,_,_>
                          { Todo = todo
                          ; EditTodo = fun (id, text) ->
                              this.props.Dispatch(EditTodo(id, text))
                          ; DeleteTodo = fun id ->
                              this.props.Dispatch(DeleteTodo id)
                          ; CompleteTodo = fun id ->
                              this.props.Dispatch(CompleteTodo id) } []))
              R.opt(this.renderFooter completedCount)
          ]