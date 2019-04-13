module TextureVBO

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input
open System
open System.Drawing
open OpenTK.Graphics.OpenGL

type TextureVBOWindow(w, h, title) =
  inherit GameWindow(w, h, GraphicsMode.Default, title)
  
  let mutable current_texture = 0

  let vertexCoords = [| 1.0f; 1.0f; 4.0f; -1.0f; 1.0f; 4.0f; -1.0f; -1.0f; 4.0f; 1.0f; -1.0f; 4.0f |] 
  let textureCoords = [| 0.0f; 0.0f; 1.0f; 0.0f; 1.0f; 1.0f; 0.0f; 1.0f |] 
  let vertexIndices = [| 0u; 1u; 2u; 3u |]
  let mutable vertexBuffer : int option = None
  let mutable textureCoordBuffer : int option = None
  let mutable vertexIndexBuffer : int option = None
  
  // Below function copied from http://deathbyalgorithm.blogspot.com/2013/05    /opentk-textures.html
  let loadTexture (path : string) =
    let quality = 0
    let repeat = true
    let flip_y = false
    let bitmap = new Bitmap(path)

    //Flip the image
    if flip_y then
      bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY)
    else
      ()

    //Generate a new texture target in gl
    let texture = GL.GenTexture()

    //Will bind the texture newly/empty created with GL.GenTexture
    //All gl texture methods targeting Texture2D will relate to this texture
    GL.BindTexture(TextureTarget.Texture2D, texture)

    //The reason why your texture will show up glColor without setting these parameters is actually
    //TextureMinFilters fault as its default is NearestMipmapLinear but we have not established mipmapping
    //We are only using one texture at the moment since mipmapping is a collection of textures pre filtered
    //I'm assuming it stops after not having a collection to check.
    match quality with
    | 1 ->
            //This is in my opinion the best since it doesnt average the result and not blurred to shirt
            //but most consider this low quality...
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int All.Nearest)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int All.Nearest)
    | _ ->
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int All.Linear)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int All.Linear)

    if repeat then
        //This will repeat the texture past its bounds set by TexImage2D
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int All.Repeat)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int All.Repeat)
    else
        //This will clamp the texture to the edge, so manipulation will result in skewing
        //It can also be useful for getting rid of repeating texture bits at the borders
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, int All.ClampToEdge)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, int All.ClampToEdge)

    //Creates a definition of a texture object in opengl
     //Parameters
     //* Target - Since we are using a 2D image we specify the target Texture2D
     //* MipMap Count / LOD - 0 as we are not using mipmapping at the moment
     //* InternalFormat - The format of the gl texture, Rgba is a base format it works all around
     //* Width;
     //* Height;
     //* Border - must be 0;
     //* 
     //* Format - this is the images format not gl's the format Bgra i believe is only language specific
     //*          C# uses little-endian so you have ARGB on the image A 24 R 16 G 8 B, B is the lowest
     //*          So it gets counted first, as with a language like Java it would be PixelFormat.Rgba
     //*          since Java is big-endian default meaning A is counted first.
     //*          but i could be wrong here it could be cpu specific :P
     //*          
     //* PixelType - The type we are using, eh in short UnsignedByte will just fill each 8 bit till the pixelformat is full
     //*             (don't quote me on that...)
     //*             you can be more specific and say for are RGBA to little-endian BGRA -> PixelType.UnsignedInt8888Reversed
     //*             this will mimic are 32bit uint in little-endian.
     //*             
     //* Data - No data at the moment it will be written with TexSubImage2D
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero)

    //Load the data from are loaded image into virtual memory so it can be read at runtime
    let bitmap_data = 
      bitmap.LockBits(
        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
          System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb)

    //Writes data to are texture target
    // * Target;
    // * MipMap;
    // * X Offset - Offset of the data on the x axis
    // * Y Offset - Offset of the data on the y axis
    // * Width;
    // * Height;
    // * Format;
    // * Type;
    // * Data - Now we have data from the loaded bitmap image we can load it into are texture data
    // *
    GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, bitmap.Width, bitmap.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bitmap_data.Scan0)

    //Release from memory
    bitmap.UnlockBits(bitmap_data)

    //get rid of bitmap object its no longer needed in this method
    bitmap.Dispose()

    ///*Binding to 0 is telling gl to use the default or null texture target
    //*This is useful to remember as you may forget that a texture is targeted
    //*And may overflow to functions that you dont necessarily want to
    //*Say you bind a texture
    //*
    //* Bind(Texture);
    //* DrawObject1();
    //*                <-- Insert Bind(NewTexture) or Bind(0)
    //* DrawObject2();
    //* 
    //* Object2 will use Texture if not set to 0 or another.
    //*/
    GL.BindTexture(TextureTarget.Texture2D, 0)
    texture
    
  
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
    textureCoordBuffer <- GL.GenBuffer() |> Some
    vertexIndexBuffer <- GL.GenBuffer() |> Some

    let n = vertexCoords |> Array.length
    let m = vertexIndices |> Array.length
    let n_uv = textureCoords |> Array.length
    
    // Various copy functions from our data structures to GL buffers
    // All intended to be wrapped in the 'execInBuffer' function
    let bufferVertices bt = GL.BufferData(bt, sizeof<OpenTK.Vector3> * n, vertexCoords, BufferUsageHint.StreamRead)
    let bufferIndices bt = GL.BufferData(bt, sizeof<uint32> * m, vertexIndices, BufferUsageHint.StreamRead)
    let bufferTextureUVCoords bt = GL.BufferData(bt, sizeof<OpenTK.Vector2> * n_uv, textureCoords, BufferUsageHint.StreamRead)

    vertexBuffer 
    |> Option.iter(execInBuffer bufferVertices BufferTarget.ArrayBuffer)
    vertexIndexBuffer 
    |> Option.iter(execInBuffer bufferIndices BufferTarget.ElementArrayBuffer)
    textureCoordBuffer 
    |> Option.iter(execInBuffer bufferTextureUVCoords BufferTarget.ArrayBuffer)

  override o.OnLoad e =
    base.OnLoad e
    
    bufferGlData ()

    GL.ClearColor(0.10f, 0.75f, 0.30f, 0.0f)

    current_texture <- loadTexture("texture.png")

    GL.Enable(EnableCap.Texture2D)
    GL.Enable(EnableCap.VertexArray)
    GL.Enable(EnableCap.TextureCoordArray)
    //Basically enables the alpha channel to be used in the color buffer
    GL.Enable(EnableCap.Blend)
    //The operation/order to blend
    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha)
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

    
    GL.EnableClientState(ArrayCap.TextureCoordArray)
    GL.EnableClientState(ArrayCap.VertexArray)
    GL.BindTexture(TextureTarget.Texture2D, current_texture)

    // These 'bt' arguments are all unused because of 'execInBuffer', see comment above
    let setVertexPointer bt = GL.VertexPointer(3, VertexPointerType.Float, 0, 0)
    let setTextureUVCoordPointer bt = GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, 0)
    let drawElts bt = GL.DrawElements(BeginMode.Quads, 4, DrawElementsType.UnsignedInt, 0)

    vertexBuffer 
    |> Option.iter(execInBuffer setVertexPointer BufferTarget.ArrayBuffer)
    textureCoordBuffer 
    |> Option.iter(execInBuffer setTextureUVCoordPointer BufferTarget.ArrayBuffer)
    vertexIndexBuffer 
    |> Option.iter(execInBuffer drawElts BufferTarget.ElementArrayBuffer)

    GL.DisableClientState(ArrayCap.TextureCoordArray)
    GL.EnableClientState(ArrayCap.VertexArray)
    GL.BindTexture(TextureTarget.Texture2D, 0)
    

    base.SwapBuffers()
