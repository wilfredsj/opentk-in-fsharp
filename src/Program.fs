open OpenTK

module Indirection =  
  let constructor : int -> int -> GameWindow = 
    let case = 5
    match case with
    | 1 -> 
      fun w h -> upcast new ColourPrimitive.ColourPrimitiveWindow(h, w, "Sandbox") 
    | 2 -> 
      fun w h -> upcast new TexturePrimitive.TexturePrimitiveWindow(h, w, "Sandbox") 
    | 3 -> 
      fun w h -> upcast new ColourVBO.ColourVBOWindow(h, w, "Sandbox") 
    | 4 -> 
      fun w h -> upcast new TextureVBO.TextureVBOWindow(h, w, "Sandbox") 
    | 5 -> 
      fun w h -> upcast new VBOText.VBOTextWindow(h, w, "Sandbox", "Hello         World!") 
    | _ -> 
      fun w h -> upcast new ColourPrimitive.ColourPrimitiveWindow(h, w, "Sandbox") 

  [<EntryPoint>]
  let makeChart args =
    let window = constructor 800 600
    do window.Run(30.0)
    1