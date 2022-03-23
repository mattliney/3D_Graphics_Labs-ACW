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

        //Currently 3 elements: floor, model, cube, cone

        private int[] mVBO_ID = new int[8]; //Add 2 more of these for each element
        private int[] mVAO_ID = new int[4]; //Add more of these for each element 
        private int mVBOindex = 0;
        private int mVAOindex = 0;

        private ModelUtility mArmadillo;
        private ShaderUtility mShader;
        private Matrix4 mView;
        private Matrix4 mFixedCam;

        private Matrix4 mModelMatrix = Matrix4.CreateScale(0.25f);
        private Matrix4 mCubeMatrix = Matrix4.CreateScale(0.4f);

        bool mIsScalingUp = true;

        protected override void OnLoad(EventArgs e)
        {
            mView = Matrix4.CreateTranslation(0, 0, -1);

            GL.ClearColor(Color4.DodgerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mArmadillo = ModelUtility.LoadModel(@"Utility/Models/model.bin");
            mShader = new ShaderUtility(@"ACW/Shaders/myVert.vert", @"ACW/Shaders/myFrag.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");

            //Vertices and Indices

            float[] floorVertices = new float[] { -3f, 0f, -3f,
                                             -3f, 0f, 3f,
                                              3f, 0f, 3f,
                                              3f, 0f, -3f};

            int[] floorIndices = new int[] { 0, 1, 2,
                                          0, 2, 3};

            float[] cubeVertices = new float[] { -1f, 1f, 0f,
                                                 -1f, -1f, 0f, 
                                                 1f, -1f, 0f, 
                                                 1f, 1f, 0f, 
            
                                                 -1f, 1f, -2f, 
                                                 -1f, -1f, -2f, 
            
                                                  1f, 1f, -2f,
                                                  1f, -1f, -2f};

            int[] cubeIndices = new int[] { 0, 1, 2, 
                                              0, 2, 3,
            
                                              0, 5, 1,
                                              0, 4, 5,
                
                                              2, 7, 6,
                                              2, 6, 3,
            
                                              6, 7, 5,
                                              6, 5, 4,
            
                                              0, 3, 6,
                                              6, 4, 0,
            
                                              1, 7, 2,
                                              1, 5, 7};

            float[] coneVertices = new float[] { 0f, 2f, 0f,
                                                 0.5f, 0, 1f,
                                                 1f, 0f, 0.5f,
            
                                                 1f, 0f, -0.5f,
                                                 0.5f, 0f, -1f,
                                                 -0.5f, 0f, -1f,

                                                 -1f, 0f, -0.5f,
                                                 -1f, 0f, 0.5f,
                                                 -0.5f, 0f, 1f};

            int[] coneIndices = new int[] { 0, 1, 2,
                                            0, 2, 3,
                                            0, 3, 4,
                                            0, 4, 5,
                                            0, 5, 6,
                                            0, 6, 7,
                                            0, 7, 8,
                                            0, 8, 1};

            GL.GenBuffers(mVBO_ID.Length, mVBO_ID);
            GL.GenVertexArrays(mVAO_ID.Length, mVAO_ID);

            //Model

            Element armadillo = new Element(mArmadillo.Vertices, mArmadillo.Indices, ref mVAOindex, ref mVBOindex, vColourLocation, vPositionLocation, true);
            armadillo.Initialise(ref mVAO_ID, ref mVBO_ID);

            //Floor

            Element floor = new Element(floorVertices, floorIndices, ref mVAOindex, ref mVBOindex, vColourLocation, vPositionLocation, false);
            floor.Initialise(ref mVAO_ID, ref mVBO_ID);

            //Cube

            Element cube = new Element(cubeVertices, cubeIndices, ref mVAOindex, ref mVBOindex, vColourLocation, vPositionLocation, false);
            cube.Initialise(ref mVAO_ID, ref mVBO_ID);

            //Cone

            Element cone = new Element(coneVertices, coneIndices, ref mVAOindex, ref mVBOindex, vColourLocation, vPositionLocation, false);
            cone.Initialise(ref mVAO_ID, ref mVBO_ID);

            //Camera

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
                Vector3 eye = new Vector3(1f, 2f, 1.5f);
                Vector3 lookAt = new Vector3(0, 0, 0);
                Vector3 up = new Vector3(0, 1, 0);
                mFixedCam = Matrix4.LookAt(eye, lookAt, up);
                int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
                GL.UniformMatrix4(uViewLocation, true, ref mFixedCam);
            }
            else if (e.KeyChar == '2')
            {
                Vector3 eye = new Vector3(-1f, 2f, 1.5f);
                Vector3 lookAt = new Vector3(0, 0, 0);
                Vector3 up = new Vector3(0, 1, 0);
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

            Vector3 cubeScale = mCubeMatrix.ExtractScale();

            if(cubeScale.Y < 0.3f)
            {
                mIsScalingUp = true;
            }
            else if(cubeScale.Y > 0.5f)
            {
                mIsScalingUp = false;
            }

            if(mIsScalingUp)
            {
                mCubeMatrix = mCubeMatrix * Matrix4.CreateScale(1.01f);
            }
            else if(!mIsScalingUp)
            {
                mCubeMatrix = mCubeMatrix * Matrix4.CreateScale(0.99f);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            Matrix4 mat = mModelMatrix * Matrix4.CreateTranslation(1f,-0.25f,0);
            GL.UniformMatrix4(uModelLocation, true, ref mat);

            GL.BindVertexArray(mVAO_ID[0]);
            GL.DrawElements(BeginMode.Triangles, mArmadillo.Indices.Length, DrawElementsType.UnsignedInt, 0);

            mat = Matrix4.CreateTranslation(0, -1f, -1f) * Matrix4.CreateScale(0.5f);
            GL.UniformMatrix4(uModelLocation, true, ref mat);
            GL.BindVertexArray(mVAO_ID[1]);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
             
            GL.UniformMatrix4(uModelLocation, true, ref mCubeMatrix);
            GL.BindVertexArray(mVAO_ID[2]);
            GL.DrawElements(PrimitiveType.Triangles, 48, DrawElementsType.UnsignedInt, 0);

            mat = Matrix4.CreateTranslation(-2f, -1f, 0f) * Matrix4.CreateScale(0.5f);
            GL.UniformMatrix4(uModelLocation, true, ref mat);
            GL.BindVertexArray(mVAO_ID[3]);
            GL.DrawElements(PrimitiveType.TriangleFan, 24, DrawElementsType.UnsignedInt, 0);


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
