module ColourPrimitive

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input
open System
open System.Drawing

type ColourPrimitiveWindow(w, h, title) =
  inherit GameWindow(w, h, GraphicsMode.Default, title)
  
  override o.OnLoad e =
    base.OnLoad e

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
    
    GL.Color4(Color.White)

    GL.Begin(BeginMode.Quads)

    //Top-Right
    GL.Color3(0.8f, 0.3f, 0.4f)
    GL.Vertex3(1.0f, 1.0f, 4.0f)

    //Top-Left
    GL.Color3(0.3f, 0.3f, 0.8f)
    GL.Vertex3(-1.0f, 1.0f, 4.0f)

    //Bottom-Left
    GL.Color3(0.8f, 0.9f, 0.4f)
    GL.Vertex3(-1.0f, -1.0f, 4.0f)

    //Bottom-Right
    GL.Color3(0.8f, 0.3f, 0.4f)
    GL.Vertex3(1.0f, -1.0f, 4.0f)
    
    GL.End()

    base.SwapBuffers()