namespace SampleReduxFable



module AppComponents = 
  open Fable.Core
  open Fable.Core.JsInterop
  open Fable.Helpers.React
  open Fable.Import
  open Domain
  open ReactComponents
  module R = Fable.Helpers.React

  open R.Props
    
  [<Pojo>]
  type AppProps = { Store: Redux.IStore<Todo[], TodoAction> }

  type App(p) as this =
      inherit React.Component<AppProps, MainSectionProps>(p)
      let getState() = { Todos=this.props.Store.getState(); Dispatch=this.props.Store.dispatch }
      do base.setInitState(getState())
      do this.props.Store.subscribe(getState >> this.setState)

      member this.render() =
          R.div [] [
              R.fn Header { AddTodo = AddTodo >> this.props.Store.dispatch } []
              R.com<MainSection,_,_> this.state []
          ]