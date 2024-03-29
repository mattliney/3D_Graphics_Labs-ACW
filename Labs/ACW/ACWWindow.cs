﻿using Labs.Utility;
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
        private Matrix4 mFreeCam;
        private Matrix4 mFixedCam;
        private Matrix4 mCurrentCam;

        private Matrix4 mModelMatrix = Matrix4.CreateScale(0.25f);
        private Matrix4 mCubeMatrix = Matrix4.CreateTranslation(0,0,-2f);
        private Matrix4 mConeMatrix = Matrix4.CreateTranslation(-3f, -1f, -3.5f);

        private bool mIsScalingUp = true;
        private bool mIsMovingForward = true;

        private int[] mTexture_ID = new int[2];
        private int mTextureIndex = 0;
        private int mTextureSamplerLocation;
        private Bitmap mTextureBitmap;
        private BitmapData mTextureData;

        private Vector3 mLightPosition = new Vector3(0, 0, 0);

        protected override void OnLoad(EventArgs e)
        {
            string filePath1 = @"ACW\texture.jpg";
            string filePath2 = @"ACW\texture2.jpg";
            LoadTexture(filePath1);
            LoadTexture(filePath2);

            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mArmadillo = ModelUtility.LoadModel(@"Utility/Models/model.bin");
            mShader = new ShaderUtility(@"ACW/Shaders/myVert.vert", @"ACW/Shaders/myFrag.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            int vTextureLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vTexCoords");
            mTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler");

            //Vertices and Indices

            float[] floorVertices = new float[] { -3f, 0f, -3f, 0, 1, 0, 0f, 0f,
                                             -3f, 0f, 3f, 0, 1, 0, 0f, 1f,
                                              3f, 0f, 3f, 0, 1, 0, 1f, 1f,
                                              3f, 0f, -3f, 0, 1, 0, 1f, 0f};

            int[] floorIndices = new int[] { 0, 1, 2,
                                          0, 2, 3};

            float[] cubeVertices = new float[] { -1f, 1f, 0f, 0f, 0f, 1f,
                                                 -1f, -1f, 0f,  0f, 0f, 1f,
                                                 1f, -1f, 0f,  0f, 1f, 1f,
                                                 1f, 1f, 0f, 0f, 1f, 1f,

                                                 -1f, 1f, -2f, -1f, 1f, 0f,
                                                 -1f, -1f, -2f, -1f, 1f, 0f,

                                                  1f, 1f, -2f, 1f, 0f, 0f,
                                                  1f, -1f, -2f, 1f, 0f, 0f};

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

            float[] coneVertices = new float[] { 0f, 2f, 0f, 0f, 1f, 0f,
                                                 0.5f, 0, 1f, 1f, 0f, 1f,
                                                 1f, 0f, 0.5f, 1f, 0f, 1f,

                                                 1f, 0f, -0.5f, 1f, 0f, -1f,
                                                 0.5f, 0f, -1f, 1f, 0f, -1f,
                                                 -0.5f, 0f, -1f, -1f, 0f, -1f,

                                                 -1f, 0f, -0.5f, -1f, 0f, -1f,
                                                 -1f, 0f, 0.5f, -1f, 0f, 1f,
                                                 -0.5f, 0f, 1f, -1f, 0f, 1f};

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

            Element armadillo = new Element(mArmadillo.Vertices, mArmadillo.Indices, ref mVAOindex, ref mVBOindex, vNormalLocation, vPositionLocation, vTextureLocation, true, false);
            armadillo.Initialise(ref mVAO_ID, ref mVBO_ID);

            //Floor

            Element floor = new Element(floorVertices, floorIndices, ref mVAOindex, ref mVBOindex, vNormalLocation, vPositionLocation, vTextureLocation, false, true);
            floor.Initialise(ref mVAO_ID, ref mVBO_ID);

            //Cube

            Element cube = new Element(cubeVertices, cubeIndices, ref mVAOindex, ref mVBOindex, vNormalLocation, vPositionLocation, vTextureLocation, false, false);
            cube.Initialise(ref mVAO_ID, ref mVBO_ID);

            //Cone

            Element cone = new Element(coneVertices, coneIndices, ref mVAOindex, ref mVBOindex, vNormalLocation, vPositionLocation, vTextureLocation, false, false);
            cone.Initialise(ref mVAO_ID, ref mVBO_ID);

            //Camera

            mFreeCam = Matrix4.CreateTranslation(0, 0, -1);

            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uViewLocation, true, ref mFreeCam);

            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            int uLightDirectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID,"uLightDirection");
            Vector3 normalisedLightDirection, lightDirection = new Vector3(0,0,-2);
            Vector3.Normalize(ref lightDirection, out normalisedLightDirection);
            GL.Uniform3(uLightDirectionLocation, normalisedLightDirection);

            MoveCamera(mFreeCam);

            base.OnLoad(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 'a')
            {
                mFreeCam = mFreeCam * Matrix4.CreateTranslation(0.1f, 0, 0);
                mCurrentCam = mFreeCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == 'd')
            {
                mFreeCam = mFreeCam * Matrix4.CreateTranslation(-0.1f, 0, 0);
                mCurrentCam = mFreeCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == 'w')
            {
                mFreeCam = mFreeCam * Matrix4.CreateTranslation(0, -0.01f, 0.5f);
                mCurrentCam = mFreeCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == 's')
            {
                mFreeCam = mFreeCam * Matrix4.CreateTranslation(0, 0.01f, -0.5f);
                mCurrentCam = mFreeCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == 'q')
            {
                mFreeCam = mFreeCam * Matrix4.CreateRotationY(0.1f);
                mCurrentCam = mFreeCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == 'e')
            {
                mFreeCam = mFreeCam * Matrix4.CreateRotationY(-0.1f);
                mCurrentCam = mFreeCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == 'r')
            {
                mFreeCam = mFreeCam * Matrix4.CreateTranslation(0, 0.1f, 0);
                mCurrentCam = mFreeCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == 'f')
            {
                mFreeCam = mFreeCam * Matrix4.CreateTranslation(0, -0.1f, 0);
                mCurrentCam = mFreeCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == '1')
            {
                Vector3 eye = new Vector3(1f, 2f, 1.5f);
                Vector3 lookAt = new Vector3(0, 0, 0);
                Vector3 up = new Vector3(0, 1, 0);
                mFixedCam = Matrix4.LookAt(eye, lookAt, up);
                
                mCurrentCam = mFixedCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == '2')
            {
                Vector3 eye = new Vector3(-2f, 0f, 1.5f);
                Vector3 lookAt = new Vector3(0, 0, 0);
                Vector3 up = new Vector3(0, 1, 0);
                mFixedCam = Matrix4.LookAt(eye, lookAt, up);

                mCurrentCam = mFixedCam;
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == '3')
            {
                mLightPosition = new Vector3(-1,0,0);
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == '4')
            {
                mLightPosition = new Vector3(0, 0, 0);
                MoveCamera(mCurrentCam);
            }
            else if (e.KeyChar == '5')
            {
                mLightPosition = new Vector3(1, 0, 0);
                MoveCamera(mCurrentCam);
            }
        }

        private void MoveCamera(Matrix4 pView)
        {
            int uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uViewLocation, true, ref pView);

            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            int uLightPosition = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightPosition");
            Vector4 lightPosition = Vector4.Transform(new Vector4(mLightPosition, 1), pView);
            GL.Uniform4(uLightPosition, lightPosition);
            GL.UniformMatrix4(uView, true, ref pView);
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

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            mModelMatrix = mModelMatrix * Matrix4.CreateRotationY(0.1f);

            Vector3 cubeScale = mCubeMatrix.ExtractScale();

            if(cubeScale.Y < 0.3f)
            {
                mIsScalingUp = true;
            }
            else if(cubeScale.Y > 0.4f)
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

            Vector3 conePos = mConeMatrix.ExtractTranslation();

            if(conePos.Z < -2)
            {
                mIsMovingForward = true;
            }
            else if(conePos.Z > 1)
            {
                mIsMovingForward = false;
            }

            if(mIsMovingForward)
            {
                mConeMatrix = mConeMatrix * Matrix4.CreateTranslation(0, 0, 0.1f);
            }
            else if(!mIsMovingForward)
            {
                mConeMatrix = mConeMatrix * Matrix4.CreateTranslation(0, 0, -0.1f);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            Matrix4 mat = mModelMatrix * Matrix4.CreateTranslation(1f,-0.25f,-1.5f);
            GL.UniformMatrix4(uModelLocation, true, ref mat);

            GL.BindVertexArray(mVAO_ID[0]);
            GL.DrawElements(BeginMode.Triangles, mArmadillo.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.Uniform1(mTextureSamplerLocation, 1);

            mat = Matrix4.CreateTranslation(0, -1f, -1f) * Matrix4.CreateScale(0.5f);
            GL.UniformMatrix4(uModelLocation, true, ref mat);
            GL.BindVertexArray(mVAO_ID[1]);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
             
            GL.UniformMatrix4(uModelLocation, true, ref mCubeMatrix);
            GL.BindVertexArray(mVAO_ID[2]);
            GL.DrawElements(PrimitiveType.Triangles, 48, DrawElementsType.UnsignedInt, 0);

            mat = mConeMatrix * Matrix4.CreateScale(0.3f);
            GL.UniformMatrix4(uModelLocation, true, ref mat);
            GL.BindVertexArray(mVAO_ID[3]);
            GL.DrawElements(PrimitiveType.TriangleFan, 24, DrawElementsType.UnsignedInt, 0);

            GL.Uniform1(mTextureSamplerLocation, 0);

            mat = Matrix4.CreateTranslation(0, -4f, -2f) * Matrix4.CreateScale(0.5f) * Matrix4.CreateRotationX(1.5708f);
            GL.UniformMatrix4(uModelLocation, true, ref mat);
            GL.BindVertexArray(mVAO_ID[1]);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            this.SwapBuffers();
        }

        private void LoadTexture(string pFilePath)
        {
            if (System.IO.File.Exists(pFilePath))
            {
                mTextureBitmap = new Bitmap(pFilePath);
                mTextureData = mTextureBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, mTextureBitmap.Width,
                mTextureBitmap.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                if (mTextureIndex == 0) { GL.ActiveTexture(TextureUnit.Texture0); }
                else { GL.ActiveTexture(TextureUnit.Texture1); }
                GL.GenTextures(1, out mTexture_ID[mTextureIndex]);
                GL.BindTexture(TextureTarget.Texture2D, mTexture_ID[mTextureIndex]);
                mTextureIndex++;

                GL.TexImage2D(TextureTarget.Texture2D,
                0, PixelInternalFormat.Rgba, mTextureData.Width, mTextureData.Height,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, mTextureData.Scan0);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

                mTextureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            else
            {
                throw new Exception("Could not find file " + pFilePath);
            }

        }
        
        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_ID.Length, mVBO_ID);
            GL.DeleteVertexArrays(mVAO_ID.Length, mVAO_ID);
            GL.DeleteTexture(0);
            GL.DeleteTexture(1);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
