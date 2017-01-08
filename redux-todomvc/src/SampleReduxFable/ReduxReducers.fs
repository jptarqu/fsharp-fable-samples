
namespace SampleReduxFable


module ReduxReducers = 
  open Fable.Core
  open Fable.Core.JsInterop
  open Fable.Helpers.React
  open Fable.Import
  open Domain
  open ReactComponents
(**
## Reducer

The reducer is a single function (which can be composed of other smaller function)
with the responsibility of updating the state in reaction to the actions
dispatched to the Redux store. F# union types and pattern matching makes
it really easy to identify and extract the data from the received actions
in a type-safe manner. The compiler will even warn us if we forget to handle
any of the possible `TodoAction` cases.
*)
  let newId state = 
    (-1, state)
    ||> Array.fold (fun id todo -> max id todo.Id)
    |> (+) 1
  let reducer (state: Todo[]) = function
      | AddTodo text ->
          let id = newId state
          state
          |> Array.append [|{Id=id; Completed=false; Text=text} ; {Id=id+1; Completed=false; Text= "But really" + text}|]
      | DeleteTodo id ->
          state
          |> Array.filter(fun todo -> todo.Id <> id)
      | EditTodo(id, text) ->
          state
          |> Array.map(fun todo ->
              if todo.Id = id
              then { todo with Text=text }
              else todo)
      | CompleteTodo id ->
          state
          |> Array.map(fun todo ->
              if todo.Id = id
              then { todo with Completed=not todo.Completed }
              else todo)
      | CompleteAll ->
          let areAllMarked =
              state |> Array.forall(fun todo -> todo.Completed)
          state
          |> Array.map(fun todo -> { todo with Completed=not areAllMarked})
      | ClearCompleted ->
          state
          |> Array.filter(fun todo -> not todo.Completed)
