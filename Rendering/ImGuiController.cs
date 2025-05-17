using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Simple3DGame.Rendering
{
    public class ImGuiController : IDisposable
    {
        private bool _frameBegun;
        private int _vertexArray;
        private int _vertexBuffer;
        private int _indexBuffer;
        private int _fontTexture;
        private int _shader;
        private int _shaderFontTextureLocation;
        private int _shaderProjectionMatrixLocation;
        
        private readonly List<char> _pressedChars = new List<char>();

        public ImGuiController(int width, int height) // Removed unsafe from constructor signature
        {
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            
            var io = ImGui.GetIO();

            // Загрузка шрифта с поддержкой кириллицы
            // Используем существующий шрифт minecraft.ttf. Убедитесь, что он поддерживает кириллицу.
            string fontPath = "Assets/Fonts/minecraft.ttf"; // Путь к файлу шрифта
            if (System.IO.File.Exists(fontPath))
            {
                unsafe // Added unsafe block specifically around the font loading call
                {
                    io.Fonts.AddFontFromFileTTF(fontPath, 16.0f, null, io.Fonts.GetGlyphRangesCyrillic());
                }
            }
            else
            {
                // Если файл шрифта не найден, используем шрифт по умолчанию
                io.Fonts.AddFontDefault();
                Console.WriteLine($"[WARN] Файл шрифта не найден: {fontPath}. Используется шрифт по умолчанию. Кириллические символы могут не отображаться корректно.");
            }
            
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.DisplaySize = new System.Numerics.Vector2(width, height);
            
            CreateDeviceObjects();
        }

        public void Update(GameWindow window, float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
                _frameBegun = false;
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(window);

            _frameBegun = true;
            ImGui.NewFrame();
        }

        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        public void WindowResized(int width, int height)
        {
            ImGui.GetIO().DisplaySize = new System.Numerics.Vector2(width, height);
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceObjects()
        {
            _vertexArray = GL.GenVertexArray();
            _vertexBuffer = GL.GenBuffer();
            _indexBuffer = GL.GenBuffer();
            _shader = CreateShader();

            GL.BindVertexArray(_vertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);

            // Setup vertex attributes
            int stride = Unsafe.SizeOf<ImDrawVert>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            // Create font texture
            RecreateFontDeviceTexture();

            // Get shader locations
            _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "FontTexture");
            _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "ProjectionMatrix");
        }

        public void PressChar(char keyChar)
        {
            _pressedChars.Add(keyChar);
        }

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState mouseState = wnd.MouseState;
            KeyboardState keyboardState = wnd.KeyboardState;

            io.MouseDown[0] = mouseState.IsButtonDown(MouseButton.Left);
            io.MouseDown[1] = mouseState.IsButtonDown(MouseButton.Right);
            io.MouseDown[2] = mouseState.IsButtonDown(MouseButton.Middle);

            io.MousePos = new System.Numerics.Vector2(mouseState.X, mouseState.Y);

            io.MouseWheel = mouseState.ScrollDelta.Y;
            io.MouseWheelH = mouseState.ScrollDelta.X;

            foreach (var c in _pressedChars)
            {
                io.AddInputCharacter(c);
            }
            _pressedChars.Clear();

            io.KeyCtrl = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper);
        }

        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DeltaTime = deltaSeconds;
        }

        private void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            io.Fonts.SetTexID((IntPtr)_fontTexture);
            io.Fonts.ClearTexData();
        }

        private void RenderImDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0) return;

            // Backup GL state
            bool lastBlendState = GL.IsEnabled(EnableCap.Blend);
            bool lastCullFaceState = GL.IsEnabled(EnableCap.CullFace);
            bool lastDepthTestState = GL.IsEnabled(EnableCap.DepthTest);
            bool lastScissorTestState = GL.IsEnabled(EnableCap.ScissorTest);
            bool lastTexture2DState = GL.IsEnabled(EnableCap.Texture2D);
            
            int lastProgram; GL.GetInteger(GetPName.CurrentProgram, out lastProgram);
            int lastTexture; GL.GetInteger(GetPName.TextureBinding2D, out lastTexture);
            int lastArrayBuffer; GL.GetInteger(GetPName.ArrayBufferBinding, out lastArrayBuffer);
            int lastElementArrayBuffer; GL.GetInteger(GetPName.ElementArrayBufferBinding, out lastElementArrayBuffer);
            int lastVertexArray; GL.GetInteger(GetPName.VertexArrayBinding, out lastVertexArray);
            int[] lastViewport = new int[4]; GL.GetInteger(GetPName.Viewport, lastViewport);
            int[] lastScissorBox = new int[4]; GL.GetInteger(GetPName.ScissorBox, lastScissorBox);
            
            BlendingFactor lastBlendSrcRgb; GL.GetInteger(GetPName.BlendSrcRgb, out int lastBlendSrcRgbValue); lastBlendSrcRgb = (BlendingFactor)lastBlendSrcRgbValue;
            BlendingFactor lastBlendDstRgb; GL.GetInteger(GetPName.BlendDstRgb, out int lastBlendDstRgbValue); lastBlendDstRgb = (BlendingFactor)lastBlendDstRgbValue;
            BlendingFactor lastBlendSrcAlpha; GL.GetInteger(GetPName.BlendSrcAlpha, out int lastBlendSrcAlphaValue); lastBlendSrcAlpha = (BlendingFactor)lastBlendSrcAlphaValue;
            BlendingFactor lastBlendDstAlpha; GL.GetInteger(GetPName.BlendDstAlpha, out int lastBlendDstAlphaValue); lastBlendDstAlpha = (BlendingFactor)lastBlendDstAlphaValue;
            BlendEquationMode lastBlendEquationRgb; GL.GetInteger(GetPName.BlendEquationRgb, out int lastBlendEquationRgbValue); lastBlendEquationRgb = (BlendEquationMode)lastBlendEquationRgbValue;
            BlendEquationMode lastBlendEquationAlpha; GL.GetInteger(GetPName.BlendEquationAlpha, out int lastBlendEquationAlphaValue); lastBlendEquationAlpha = (BlendEquationMode)lastBlendEquationAlphaValue;

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.ScissorTest);
            GL.Enable(EnableCap.Texture2D);
            GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);

            // Setup viewport
            GL.Viewport(0, 0, (int)drawData.DisplaySize.X, (int)drawData.DisplaySize.Y);
            
            Matrix4 orthoProjection = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                drawData.DisplaySize.X,
                drawData.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            GL.UseProgram(_shader);
            GL.Uniform1(_shaderFontTextureLocation, 0);
            GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref orthoProjection);
            GL.BindVertexArray(_vertexArray);
            
            drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                // Updated: Use proper indexing instead of GetCmdListPtr
                ImDrawListPtr cmdList = drawData.CmdLists[n];

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmdList.VtxBuffer.Data, BufferUsageHint.StreamDraw);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
                GL.BufferData(BufferTarget.ElementArrayBuffer, cmdList.IdxBuffer.Size * sizeof(ushort), cmdList.IdxBuffer.Data, BufferUsageHint.StreamDraw);

                int vtxOffset = 0;
                int idxOffset = 0;

                for (int cmd_i = 0; cmd_i < cmdList.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        GL.Scissor((int)pcmd.ClipRect.X, (int)(drawData.DisplaySize.Y - pcmd.ClipRect.W), (int)(pcmd.ClipRect.Z - pcmd.ClipRect.X), (int)(pcmd.ClipRect.W - pcmd.ClipRect.Y));
                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idxOffset * sizeof(ushort)), vtxOffset);
                    }
                    idxOffset += (int)pcmd.ElemCount;
                }
                vtxOffset += cmdList.VtxBuffer.Size;
            }

            // Restore GL state
            GL.UseProgram(lastProgram);
            GL.BindTexture(TextureTarget.Texture2D, lastTexture);
            GL.BindVertexArray(lastVertexArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, lastArrayBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, lastElementArrayBuffer);
            GL.BlendEquationSeparate(lastBlendEquationRgb, lastBlendEquationAlpha);
            
            // Fix for BlendFuncSeparate parameters
            GL.BlendFuncSeparate(
                (BlendingFactorSrc)(int)lastBlendSrcRgb, 
                (BlendingFactorDest)(int)lastBlendDstRgb, 
                (BlendingFactorSrc)(int)lastBlendSrcAlpha, 
                (BlendingFactorDest)(int)lastBlendDstAlpha);
            
            if (lastBlendState) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
            if (lastCullFaceState) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (lastDepthTestState) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (lastScissorTestState) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
            if (lastTexture2DState) GL.Enable(EnableCap.Texture2D); else GL.Disable(EnableCap.Texture2D);
            
            GL.Viewport(lastViewport[0], lastViewport[1], lastViewport[2], lastViewport[3]);
            GL.Scissor(lastScissorBox[0], lastScissorBox[1], lastScissorBox[2], lastScissorBox[3]);
        }

        private int CreateShader()
        {
            int program = GL.CreateProgram();

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            string vertShaderSource = @"
                #version 330 core
                uniform mat4 ProjectionMatrix;
                layout(location = 0) in vec2 Position;
                layout(location = 1) in vec2 UV;
                layout(location = 2) in vec4 Color;
                out vec2 Frag_UV;
                out vec4 Frag_Color;
                void main()
                {
                    Frag_UV = UV;
                    Frag_Color = Color;
                    gl_Position = ProjectionMatrix * vec4(Position.xy, 0, 1);
                }
            ";

            string fragShaderSource = @"
                #version 330 core
                uniform sampler2D FontTexture;
                in vec2 Frag_UV;
                in vec4 Frag_Color;
                out vec4 Out_Color;
                void main()
                {
                    Out_Color = Frag_Color * texture(FontTexture, Frag_UV.st);
                }
            ";

            GL.ShaderSource(vertexShader, vertShaderSource);
            GL.ShaderSource(fragmentShader, fragShaderSource);
            GL.CompileShader(vertexShader);
            GL.CompileShader(fragmentShader);
            
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            
            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return program;
        }

        public void Dispose()
        {
            if (_fontTexture > 0)
            {
                GL.DeleteTexture(_fontTexture);
                ImGui.GetIO().Fonts.SetTexID(IntPtr.Zero);
                _fontTexture = 0;
            }
            
            if (_shader > 0)
            {
                GL.DeleteProgram(_shader);
                _shader = 0;
            }
            
            if (_vertexArray > 0)
            {
                GL.DeleteVertexArray(_vertexArray);
                _vertexArray = 0;
            }
            
            if (_vertexBuffer > 0)
            {
                GL.DeleteBuffer(_vertexBuffer);
                _vertexBuffer = 0;
            }
            
            if (_indexBuffer > 0)
            {
                GL.DeleteBuffer(_indexBuffer);
                _indexBuffer = 0;
            }
            
            ImGui.DestroyContext();
        }
    }
}