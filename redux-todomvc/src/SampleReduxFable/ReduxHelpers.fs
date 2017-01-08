namespace SampleReduxFable

open System
open Fable.Core
open Fable.Core.JsInterop

module Redux =
    open System
    open Fable.Import
    open Fable.Core
    open Fable.Core.JsInterop

    type IStore<'TState, 'TAction> =
        abstract dispatch: 'TAction->unit
        abstract subscribe: (unit->unit)->unit
        abstract getState: unit->'TState

    let private createStore_: JsFunc = import "createStore" "redux"

    let createStore (reducer: 'TState->'TAction->'TState) (initState: 'TState): IStore<'TState, 'TAction> =
        // Check if the action is a lifecycle event dispatched by Redux before applying the reducer
        let jsReducer = JsFunc2(fun state action ->
            match !!action?``type``: obj with
            | :?string as s when s.StartsWith "@@" -> state
            | _ -> reducer state action)
        match !!Browser.window?devToolsExtension: JsFunc with
        | null -> !!createStore_.Invoke(jsReducer, initState)
        | ext -> !!createStore_.Invoke(jsReducer, initState, ext.Invoke())
