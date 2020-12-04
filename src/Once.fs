module Once

open Browser.Dom
open Hacn.Types

type OnceState = {
  UnderlyingState: obj option
}

let Once wrappedOperation = 
  Perform({ 
    PreProcess = fun _ -> None;
    GetResult = fun capture operationState -> 
      match operationState with
      | Some(_) -> 
        PerformContinue({Element = None; Effect = None; LayoutEffect = None}, ())
      | None -> 
        match wrappedOperation with
        | Perform({GetResult = getResult}) ->
          let response = getResult capture operationState
          match response with 
          | PerformContinue({ Element = underlyingElement; Effect = underlyingEffectOpt; LayoutEffect = underlyingLayoutEffect }, returnValue) -> 
            let wrapUnderlyingEffect rerender =
              let wrapRerender stateUpdater =
                rerender (fun rerenderState -> 
                  let underlyingState = stateUpdater rerenderState
                  Some ({UnderlyingState = underlyingState} :> obj)
                ) 
                
              match underlyingEffectOpt with
              | Some(underlyingEffect) ->
                underlyingEffect wrapRerender
              | None -> 
                rerender (fun _ -> Some ({UnderlyingState = None} :> obj))
                None

            PerformContinue(
              { 
                Element = underlyingElement
                Effect = Some(wrapUnderlyingEffect)
                LayoutEffect = underlyingLayoutEffect
              },
              returnValue
            )
          | _ -> response
        | _ -> failwith "Operation must be Perform"
  })