﻿using OpenTK;
using System;
using OpenTK.Graphics;
using Labs.Utility;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab2
{
    public class Lab2_2Window : GameWindow
    {
        public Lab2_2Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 2_2 Understanding the Camera",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[2];
        private int mVAO_ID;
        private ShaderUtility mShader;
        private ModelUtility mModel;
        private Matrix4 mView;

        protected override void OnLoad(EventArgs e)
        {
            mView = Matrix4.CreateTranslation(0, 0, -2);

            // Set some GL state
            GL.ClearColor(Color4.DodgerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mModel = ModelUtility.LoadModel(@"Utility/Models/lab22model.sjg");    
            mShader = new ShaderUtility(@"Lab2/Shaders/vLab22.vert", @"Lab2/Shaders/fSimple.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");

            mVAO_ID = GL.GenVertexArray();
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);
            
            GL.BindVertexArray(mVAO_ID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModel.Vertices.Length * sizeof(float)), mModel.Vertices, BufferUsageHint.StaticDraw);           
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[1]);
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

            GL.BindVertexArray(0);

            Vector3 eye = new Vector3(0.0f, 0.5f, -4f);
            Vector3 lookAt = new Vector3(0, 0, 0);
            Vector3 up = new Vector3(0, 1, 0);
            mView = Matrix4.LookAt(eye, lookAt, up);

            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uViewLocation, true, ref mView);

            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            base.OnLoad(e);
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            Matrix4 m1 = Matrix4.CreateTranslation(0, 0, 0);
            m1 = m1 * Matrix4.CreateScale(0.2f);

            for (int i = 0; i < 10; i++)
            {
                GL.UniformMatrix4(uModelLocation, true, ref m1);
                GL.BindVertexArray(mVAO_ID);
                GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
                GL.BindVertexArray(0);
                m1 = Matrix4.CreateTranslation(i, 0, 0);
                m1 = m1 * Matrix4.CreateScale(0.2f);

                for (int j = 0; j < 10; j++)
                {
                    GL.UniformMatrix4(uModelLocation, true, ref m1);
                    GL.BindVertexArray(mVAO_ID);
                    GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
                    GL.BindVertexArray(0);
                    m1 = Matrix4.CreateTranslation(j, i, 0);
                    m1 = m1 * Matrix4.CreateScale(0.2f);

                    for (int k = 0; k < 10; k++)
                    {
                        GL.UniformMatrix4(uModelLocation, true, ref m1);
                        GL.BindVertexArray(mVAO_ID);
                        GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
                        GL.BindVertexArray(0);
                        m1 = Matrix4.CreateTranslation(j, i, k);
                        m1 = m1 * Matrix4.CreateScale(0.2f);
                    }
                }
            }

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArray(mVAO_ID);
            mShader.Delete();
            base.OnUnload(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateTranslation(0.01f, 0, 0);
                MoveCamera();
            }
            else if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateTranslation(-0.01f, 0, 0);
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
        }

        private void MoveCamera()
        {
            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uViewLocation, true, ref mView);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                float windowHeight = (float)this.ClientRectangle.Height;
                float windowWidth = (float)this.ClientRectangle.Width;

                if (windowHeight > windowWidth)
                {
                    if (windowWidth < 1) { windowWidth = 1; }

                    float ratio = windowWidth / windowHeight;
                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, ratio, 0.5f, 5);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
                else
                {
                    if (windowHeight < 1) { windowHeight = 1; }

                    float ratio = windowWidth / windowHeight;
                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, ratio, 0.5f, 5);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
            }
        }
    }
}
