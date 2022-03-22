using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        public ACWWindow()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_ID = new int[4]; //Add 2 more of these for each element
        private int[] mVAO_ID = new int[2]; //Add more of these for each element 
        private ModelUtility mModel;
        private ShaderUtility mShader;
        private Matrix4 mView;
        private Matrix4 mFixedCam;
        private Matrix4 mModelMatrix = Matrix4.CreateScale(0.5f);

        protected override void OnLoad(EventArgs e)
        {
            mView = Matrix4.CreateTranslation(0, 0, -1);

            GL.ClearColor(Color4.DodgerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mModel = ModelUtility.LoadModel(@"Utility/Models/model.bin");
            mShader = new ShaderUtility(@"ACW/Shaders/myVert.vert", @"ACW/Shaders/myFrag.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");

            //Vertices and Indices

            float[] vertices = new float[] { -3f, 0f, -3f,
                                             -3f, 0f, 3f,
                                              3f, 0f, 3f,
                                              3f, 0f, -3f};

            uint[] indices = new uint[] { 0, 1, 2,
                                          0, 2, 3};

            GL.GenBuffers(mVBO_ID.Length, mVBO_ID);
            GL.GenVertexArrays(mVAO_ID.Length, mVAO_ID);

            //Model

            GL.BindVertexArray(mVAO_ID[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_ID[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModel.Vertices.Length * sizeof(float)), mModel.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_ID[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mModel.Indices.Length * sizeof(float)), mModel.Indices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vColourLocation);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            //Primitive

            GL.BindVertexArray(mVAO_ID[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_ID[2]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_ID[3]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false,3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vColourLocation);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uViewLocation, true, ref mView);

            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            base.OnLoad(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateTranslation(0.1f, 0, 0);
                MoveCamera();
            }
            else if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateTranslation(-0.1f, 0, 0);
                MoveCamera();
            }
            else if (e.KeyChar == 'w')
            {
                mView = mView * Matrix4.CreateTranslation(0, -0.01f, 0.5f);
                MoveCamera();
            }
            else if (e.KeyChar == 's')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0.01f, -0.5f);
                MoveCamera();
            }
            else if (e.KeyChar == 'q')
            {
                mView = mView * Matrix4.CreateRotationY(0.1f);
                MoveCamera();
            }
            else if (e.KeyChar == 'e')
            {
                mView = mView * Matrix4.CreateRotationY(-0.1f);
                MoveCamera();
            }
            else if (e.KeyChar == 'r')
            {
                mView = mView * Matrix4.CreateTranslation(0, 0.1f, 0);
                MoveCamera();
            }
            else if (e.KeyChar == 'f')
            {
                mView = mView * Matrix4.CreateTranslation(0, -0.1f, 0);
                MoveCamera();
            }
            else if (e.KeyChar == '1')
            {
                Vector3 eye = new Vector3(0.0f, 0.5f, -4f);
                Vector3 lookAt = new Vector3(0, 0, 0);
                Vector3 up = new Vector3(0, 1, 0);
                mFixedCam = Matrix4.LookAt(eye, lookAt, up);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mFixedCam);
            }
            else if (e.KeyChar == '2')
            {
                Vector3 eye = new Vector3(-3f, 0.5f, -4f);
                Vector3 lookAt = new Vector3(0, 0, 0);
                Vector3 up = new Vector3(0, 2, 0);
                mFixedCam = Matrix4.LookAt(eye, lookAt, up);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mFixedCam);
            }
        }

        private void MoveCamera()
        {
            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uViewLocation, true, ref mView);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            mModelMatrix = mModelMatrix * Matrix4.CreateRotationY(0.1f);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");

            Matrix4 newMatrix = mModelMatrix * Matrix4.CreateTranslation(0,-0.5f,0);
            GL.UniformMatrix4(uModelLocation, true, ref newMatrix);

            GL.BindVertexArray(mVAO_ID[0]);
            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 mat = Matrix4.CreateTranslation(0, -1f, -1f);
            GL.UniformMatrix4(uModelLocation, true, ref mat);

            GL.BindVertexArray(mVAO_ID[1]);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_ID.Length, mVBO_ID);
            GL.DeleteVertexArrays(mVAO_ID.Length, mVAO_ID);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
