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
        // Vertex buffer and index buffer, particle
        // and index arrays
        private VertexBuffer verts;
        private IndexBuffer ints;
        private VertexPositionTexture[] particles;
        private int[] indices;
        // Billboard settings
        private int nBillboards;
        private Vector2 billboardSize;
        private Texture2D texture;
        // GraphicsDevice and Effect
        private GraphicsDevice graphicsDevice;
        private Effect effect;

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

        void GenerateParticles(Vector3[] particlePositions)
        {
            // Create vertex and index arrays
            particles = new VertexPositionTexture[nBillboards*4];
            indices = new int[nBillboards*6];
            int x = 0;
            // For each billboard...
            for (int i = 0; i < nBillboards*4; i += 4)
            {
                Vector3 pos = particlePositions[i/4];
                // Add 4 vertices at the billboard's position
                particles[i + 0] = new VertexPositionTexture(pos,
                    new Vector2(0, 0));
                particles[i + 1] = new VertexPositionTexture(pos,
                    new Vector2(0, 1));
                particles[i + 2] = new VertexPositionTexture(pos,
                    new Vector2(1, 1));
                particles[i + 3] = new VertexPositionTexture(pos,
                    new Vector2(1, 0));
                // Add 6 indices to form two triangles
                indices[x++] = i + 0;
                indices[x++] = i + 3;
                indices[x++] = i + 2;
                indices[x++] = i + 2;
                indices[x++] = i + 1;
                indices[x++] = i + 0;
                // Create and set the vertex buffer
                verts = new VertexBuffer(graphicsDevice,
                    typeof(VertexPositionTexture),
                    nBillboards*4, BufferUsage.WriteOnly);
                verts.SetData<VertexPositionTexture>(particles);
                // Create and set the index buffer
                ints = new IndexBuffer(graphicsDevice,
                    IndexElementSize.ThirtyTwoBits,
                    nBillboards*6, BufferUsage.WriteOnly);
                ints.SetData<int>(indices);
            }
        }

        public void Draw(Matrix view, Matrix projection, Matrix world, Vector3 up, Vector3 right)
        {
            // Set the vertex and index buffer to the graphics card
            graphicsDevice.SetVertexBuffer(verts);
            graphicsDevice.Indices = ints;
            SetEffectParameters(view, projection, world, up, right);
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            // Draw the billboards
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4 * nBillboards, 0, nBillboards * 2);
            graphicsDevice.BlendState = BlendState.Opaque;
            // Un-set the vertex and index buffer
            graphicsDevice.SetVertexBuffer(null);
            graphicsDevice.Indices = null;
        }

        private void SetEffectParameters(Matrix view, Matrix projection, Matrix world, Vector3 up, Vector3 right)
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

        public void SetClipPlane(Vector4? plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(plane.HasValue);
            if (plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(plane.Value);
        }
    }

}
