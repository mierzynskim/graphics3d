using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GK1
{
    public class BillboardSystem
    {
        private VertexBuffer verts;
        private IndexBuffer ints;
        private VertexPositionTexture[] particles;
        private int[] indices;

        private readonly int nBillboards;
        private readonly Vector2 billboardSize;
        private readonly Texture2D texture;
        private readonly GraphicsDevice graphicsDevice;
        private readonly Effect effect;

        private const bool EnsureOcclusion = true;

        public BillboardSystem(GraphicsDevice graphicsDevice,
            ContentManager content, Texture2D texture,
            Vector2 billboardSize, Vector3[] particlePositions)
        {
            this.nBillboards = particlePositions.Length;
            this.billboardSize = billboardSize;
            this.graphicsDevice = graphicsDevice;
            this.texture = texture;
            effect = content.Load<Effect>("BillboardEffect");
            GenerateParticles(particlePositions);
        }

        private void GenerateParticles(Vector3[] particlePositions)
        {
            particles = new VertexPositionTexture[nBillboards * 4];
            indices = new int[nBillboards * 6];
            int x = 0;
            for (int i = 0; i < nBillboards * 4; i += 4)
            {
                Vector3 pos = particlePositions[i / 4];
                particles[i + 0] = new VertexPositionTexture(pos,
                    new Vector2(0, 0));
                particles[i + 1] = new VertexPositionTexture(pos,
                    new Vector2(0, 1));
                particles[i + 2] = new VertexPositionTexture(pos,
                    new Vector2(1, 1));
                particles[i + 3] = new VertexPositionTexture(pos,
                    new Vector2(1, 0));
                indices[x++] = i + 0;
                indices[x++] = i + 3;
                indices[x++] = i + 2;
                indices[x++] = i + 2;
                indices[x++] = i + 1;
                indices[x++] = i + 0;
                verts = new VertexBuffer(graphicsDevice,
                    typeof(VertexPositionTexture),
                    nBillboards * 4, BufferUsage.WriteOnly);
                verts.SetData<VertexPositionTexture>(particles);
                ints = new IndexBuffer(graphicsDevice,
                    IndexElementSize.ThirtyTwoBits,
                    nBillboards * 6, BufferUsage.WriteOnly);
                ints.SetData<int>(indices);
            }
        }


        public void Draw(Matrix view, Matrix projection, Vector3 right)
        {
            graphicsDevice.SetVertexBuffer(verts);
            graphicsDevice.Indices = ints;
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            SetEffectParameters(view, projection, right);

            DrawOpaquePixels();
            DrawTransparentPixels();

            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.SetVertexBuffer(null);
            graphicsDevice.Indices = null;
        }

        private void SetEffectParameters(Matrix view, Matrix projection, Vector3 right)
        {
            effect.Parameters["ParticleTexture"].SetValue(texture);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["World"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["Size"].SetValue(billboardSize / 2);
            effect.Parameters["Up"].SetValue(Vector3.UnitZ);
            effect.Parameters["Side"].SetValue(right);

            effect.CurrentTechnique.Passes[0].Apply();
        }

        private void DrawOpaquePixels()
        {
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            effect.Parameters["AlphaTest"].SetValue(true);
            effect.Parameters["AlphaTestGreater"].SetValue(true);
            DrawBillboards();
        }
        private void DrawBillboards()
        {
            effect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4 * nBillboards, 0, nBillboards * 2);
        }

        private void DrawTransparentPixels()
        {
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            effect.Parameters["AlphaTest"].SetValue(true);
            effect.Parameters["AlphaTestGreater"].SetValue(false);
            DrawBillboards();
        }

        public void SetClipPlane(Vector4? plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(plane.HasValue);
            if (plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(plane.Value);
        }
    }

}
