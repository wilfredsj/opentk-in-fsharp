module ColourVBO

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input
open System
open System.Drawing

type ColourVBOWindow(w, h, title) =
  inherit GameWindow(w, h, GraphicsMode.Default, title)

  let vertexCoords = [| 1.0f; 1.0f; 4.0f; -1.0f; 1.0f; 4.0f; -1.0f; -1.0f; 4.0f; 1.0f; -1.0f; 4.0f |] 
  let colourCoords = [| 0.8f; 0.3f; 0.4f; 0.3f; 0.3f; 0.8f; 0.8f; 0.9f; 0.4f; 0.8f; 0.3f; 0.4f |] 
  let vertexIndices = [| 0u; 1u; 2u; 3u |]
  let mutable vertexBuffer : int option = None
  let mutable colourBuffer : int option = None
  let mutable vertexIndexBuffer : int option = None

  // Bind buffer, do 'something' with the buffer, unbind the buffer
  // 'fn' is BufferTarget -> unit
  // Perhaps 'fn' should be () -> unit
  // Since the later functions end up having an unused 'bt' argument
  let execInBuffer (fn : BufferTarget -> unit) bufferType (bufferNum : int) =
    GL.BindBuffer(bufferType, bufferNum)
    fn bufferType
    GL.BindBuffer(bufferType, 0)
  
  let bufferGlData () = 
    vertexBuffer <- GL.GenBuffer() |> Some
    colourBuffer <- GL.GenBuffer() |> Some
    vertexIndexBuffer <- GL.GenBuffer() |> Some

    let n = vertexCoords |> Array.length
    let m = vertexIndices |> Array.length

    // Various copy functions from our data structures to GL buffers
    // All intended to be wrapped in the 'execInBuffer' function
    let bufferVertices bt = GL.BufferData(bt, sizeof<OpenTK.Vector3> * n, vertexCoords, BufferUsageHint.StreamRead)
    let bufferIndices bt = GL.BufferData(bt, sizeof<uint32> * m, vertexIndices, BufferUsageHint.StreamRead)
    let bufferColours bt = GL.BufferData(bt, sizeof<OpenTK.Vector3> * n, colourCoords, BufferUsageHint.StreamRead)

    vertexBuffer 
    |> Option.iter(execInBuffer bufferVertices BufferTarget.ArrayBuffer)

    vertexIndexBuffer 
    |> Option.iter(execInBuffer bufferIndices BufferTarget.ElementArrayBuffer)

    colourBuffer 
    |> Option.iter(execInBuffer bufferColours BufferTarget.ArrayBuffer)
  
  override o.OnLoad e =
    base.OnLoad e
    
    bufferGlData ()

    GL.ClearColor(0.10f, 0.75f, 0.30f, 0.0f)

    //Use for pixel depth comparing before storing in the depth buffer
    GL.Enable(EnableCap.DepthTest)
    
  override o.OnResize e =
    base.OnResize e
    GL.Viewport(base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width, base.ClientRectangle.Height)
    let mutable projection = Matrix4.CreatePerspectiveFieldOfView(float32 (Math.PI / 4.), float32 base.Width / float32 base.Height, 1.f, 200.f)
    GL.MatrixMode(MatrixMode.Projection)
    GL.LoadMatrix(&projection)

  override o.OnRenderFrame e =
    base.OnRenderFrame e

    GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
    let mutable modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY)
    GL.MatrixMode(MatrixMode.Modelview)
    GL.LoadMatrix(ref modelview)
    
    GL.EnableClientState(ArrayCap.ColorArray)
    GL.EnableClientState(ArrayCap.VertexArray)

    // These 'bt' arguments are all unused because of 'execInBuffer', see comment above
    let setVertexPointer bt = GL.VertexPointer(3, VertexPointerType.Float, 0, 0)
    let setColourPointer bt = GL.ColorPointer(3, ColorPointerType.Float, 0, 0)
    let drawElts bt = GL.DrawElements(BeginMode.Quads, 4, DrawElementsType.UnsignedInt, 0)

    vertexBuffer 
    |> Option.iter(execInBuffer setVertexPointer BufferTarget.ArrayBuffer)
    
    colourBuffer 
    |> Option.iter(execInBuffer setColourPointer BufferTarget.ArrayBuffer)

    vertexIndexBuffer 
    |> Option.iter(execInBuffer drawElts BufferTarget.ElementArrayBuffer)

    GL.DisableClientState(ArrayCap.ColorArray)
    GL.EnableClientState(ArrayCap.VertexArray)
      

    base.SwapBuffers()