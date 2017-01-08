namespace SampleReduxFable

module AppStart =
  open Fable.Core
  open Fable.Import
  open Fable.Core.JsInterop
  open Domain
  open AppComponents

  module R = Fable.Helpers.React
  let start() =
      let store =
          { Text="Use Fable + React + Redux"; Completed=false; Id=0}
          |> Array.singleton
          |> Redux.createStore ReduxReducers.reducer

      ReactDom.render(
          R.com<App,_,_> { Store=store } [],
          Browser.document.getElementsByClassName("todoapp").[0]
      ) |> ignore

  do start()