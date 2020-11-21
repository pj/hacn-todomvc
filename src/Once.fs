module Once

open Browser.Dom
open Hacn.Types

type OnceState = {
  UnderlyingState: obj option
}

let Once wrappedOperation = 
  Perform({ 
    OperationType = NotCore;
    PreProcess = fun _ -> None;
    GetResult = fun capture operationState -> 
      match operationState with
      | Some(_) -> 
        InvokeContinue(None, None, None, ())
      | None -> 
        match wrappedOperation with
        | Perform({GetResult = getResult}) ->
          let response = getResult capture operationState
          match response with 
          | InvokeContinue(underlyingElement, underlyingEffectOpt, underlyingLayoutEffect, returnValue) -> 
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

            InvokeContinue(
              underlyingElement, 
              Some(wrapUnderlyingEffect), 
              underlyingLayoutEffect, 
              returnValue
            )
          | _ -> response
        | _ -> failwith "Operation must be Perform"
  })