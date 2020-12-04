module Focus

open Browser.Dom
open Hacn.Types
open Fable.React
open Browser.Types

let Focus (ref: IRefValue<option<HTMLElement>>) = 
  Perform({ 
    PreProcess = fun _ -> None;
    GetResult = fun _ __ -> 
      let focusLayoutEffect _ =
        match ref.current with
        | Some(element) -> 
          let inputElement = box element :?> HTMLInputElement
          inputElement.setSelectionRange (0, inputElement.value.Length)
          inputElement.focus ()
        | None -> failwith "Ref not set"
        None
        
      PerformContinue({Element = None; Effect = None; LayoutEffect = Some(focusLayoutEffect)}, ())
  })