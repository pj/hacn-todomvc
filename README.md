# Introduction

TodoMVC written in [Hacn](https://github.com/pj/hacn). Hacn is a DSL for writting react components written in [F#/Fable](https://fable.io). It uses [computation expressions](https://fsharpforfunandprofit.com/series/computation-expressions.html) to provide a way of writing components in a sequential way, similar to how async works but with the ability to go back to an earlier step.

# Example

A Hacn component is a series of operations written using the F# computation expression syntax. You can think of the `let! props = ...` syntax as working in a similar way to the javascript async syntax: `const props = await ...`.

```fsharp
type HeaderProps = { AddTodo: Action -> unit }

let Header : HeaderProps -> ReactElement = 
  react {
    // Changes to props cause rendering to restart where the Props operation is
    // used.
    let! props = Props

    // The Ref operation simply wraps the useRef hook.
    let! ref = Ref None

    // Rendering happens as a side effect of the sequence of operations and can
    // capture values into the sequence.
    let! key = RenderCapture (
      fun capture ->
        // Elements are rendered using the Feliz library.
        Html.header [
          prop.children [
            Html.input [
              prop.ref ref
              prop.onKeyDown (fun keyEvent -> capture keyEvent.key)
              prop.type' "text"
            ]
          ]
        ]
    )

    // Rather than a callback we can handle events as part of the sequence of operations and then return another operation or render something different.
    if key = "Enter" then
      match ref.current with
      | Some(element) -> 
        let inputElement = box element :?> HTMLInputElement
        // The Call can operation is used to call functions passed into props.
        do! Call (fun () -> 
          props.AddTodo (AddTodo(inputElement.value))
          inputElement.value <- ""
        )
      | None -> failwith "Ref not set"
  }
```