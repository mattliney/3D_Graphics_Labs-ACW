using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Labs.ACW
{
    class Element
    {
        private float[] mVertices;
        private int[] mIndices;
        private int mVAOindex;
        private int mVBOindex;
        private int mNormalLocation;
        private int mPositionLocation;
        private int mTextureLocation;

        private bool mIsModel;
        private bool mHasTexture;

        public Element(float[] pVertices, int[] pIndices, ref int pVAOindex, ref int pVBOindex, int pNormal, int pPosition, int pTexture, bool pIsModel, bool pHasTexture)
        {
            mVertices = pVertices;
            mIndices = pIndices;

            mVAOindex = pVAOindex;
            pVAOindex++;

            mVBOindex = pVBOindex;
            pVBOindex = pVBOindex + 2;

            mNormalLocation = pNormal;
            mPositionLocation = pPosition;

            mTextureLocation = pTexture;

            mIsModel = pIsModel;
            mHasTexture = pHasTexture;
        }

        public void Initialise(ref int[] pVAO_ID, ref int[] pVBO_ID)
        {
            GL.BindVertexArray(pVAO_ID[mVAOindex]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, pVBO_ID[mVBOindex]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mVertices.Length * sizeof(float)), mVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, pVBO_ID[mVBOindex + 1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mIndices.Length * sizeof(float)), mIndices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mVertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mIndices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }


            if(!mHasTexture)
            {
                GL.EnableVertexAttribArray(mPositionLocation);
                GL.VertexAttribPointer(mPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
                GL.EnableVertexAttribArray(mNormalLocation);
                GL.VertexAttribPointer(mNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
            }
            else
            {
                GL.EnableVertexAttribArray(mPositionLocation);
                GL.VertexAttribPointer(mPositionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                GL.EnableVertexAttribArray(mNormalLocation);
                GL.VertexAttribPointer(mNormalLocation, 3, VertexAttribPointerType.Float, true, 8 * sizeof(float), 3 * sizeof(float));
                GL.EnableVertexAttribArray(mTextureLocation);
                GL.VertexAttribPointer(mTextureLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            }

        }
    }
}
