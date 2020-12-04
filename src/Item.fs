module Item

open Types
open Browser.Dom
open Feliz
open Fable.React
open Hacn.Core
open Hacn.Operations
open Browser.Types
open Focus
open Once

type ItemEvent = 
| Toggled
| StartEdit
| Delete

type ItemEditEvent = 
| EditBlured
| EditKey of string
| EditChange of string

let valueFromRef (ref: IRefValue<option<HTMLElement>>) = 
  match ref.current with
  | Some(element) -> 
    let inputElement = box element :?> HTMLInputElement
    inputElement.value
  | None -> failwith "Ref not set"

let Item : ItemProps -> ReactElement =
  react {
    let! props = Props
    let sendEvent event = Call (fun _ -> props.SendEvent event)
    let! ref = Ref None
    let! _, start = State ()
    let! itemEvent = RenderCapture(
      fun capture -> 
        Html.li [
          if props.Todo.Completed then
            prop.className "completed"
          prop.children [
            Html.div [
              prop.className "view"
              prop.children [
                Html.input [
                  prop.className "toggle"
                  prop.type' "checkbox"
                  prop.defaultChecked false
                  prop.isChecked props.Todo.Completed
                  prop.onChange (fun (_: bool) -> capture Toggled)
                ]
                Html.label [
                  prop.onDoubleClick (fun _ -> capture StartEdit)
                  prop.text props.Todo.Title
                ]
                Html.button [
                  prop.className "destroy"
                  prop.onClick (fun _ -> capture Delete)
                ]
              ]
            ]
          ]
        ]
      )

    match itemEvent with 
    | Toggled -> do! sendEvent (ToggleTodo props.Todo.Id)
    | Delete -> do! sendEvent (ClearTodo props.Todo.Id)
    | StartEdit ->
      let! editText, setEditState = State props.Todo.Title

      do! Once (Focus ref)

      let! itemEditEvent = RenderCapture (fun capture -> 
          Html.li [
            prop.className "editing"
            prop.children [
              Html.input [
                prop.ref ref
                prop.className "edit"
                prop.value editText
                prop.type' "text"
                prop.onBlur (fun _ -> capture EditBlured)
                prop.onKeyDown (fun keyEvent -> capture (EditKey keyEvent.key))
                prop.onChange (
                  fun (keyEvent: Browser.Types.Event) -> 
                    let inputElement = box keyEvent.target :?> HTMLInputElement
                    capture (EditChange inputElement.value)
                  )
              ]
            ]
          ]
      )

      let currentText = valueFromRef ref

      match itemEditEvent with 
      | EditBlured -> do! sendEvent (SaveTodo(props.Todo.Id, currentText))
      | EditKey(key) ->
        match key with 
        | "Enter" -> do! sendEvent (SaveTodo(props.Todo.Id, currentText))
        | "Escape" -> do! start ()
        | _ -> ()
      | EditChange(value) -> do! setEditState value
  }