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
    public struct ParticleVertex : IVertexType
    {
        Vector3 startPosition;
        Vector2 uv;
        Vector3 direction;
        float speed;
        float startTime;
        // Starting position of that particle (t = 0)
        public Vector3 StartPosition
        {
            get { return startPosition; }
            set { startPosition = value; }
        }

        // UV coordinate, used for texturing and to offset vertex in shader
        public Vector2 UV
        {
            get { return uv; }
            set { uv = value; }
        }

        // Movement direction of the particle
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        // Speed of the particle in units/second
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        // The time since the particle system was created that this
        // particle came into use
        public float StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        public ParticleVertex(Vector3 StartPosition, Vector2 UV,
            Vector3 Direction, float Speed, float StartTime)
        {
            this.startPosition = StartPosition;
            this.uv = UV;
            this.direction = Direction;
            this.speed = Speed;
            this.startTime = StartTime;
        }

        // Vertex declaration
        public readonly static VertexDeclaration VertexDeclaration =
            new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector3,
                    // Start position
                    VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Vector2,
                    // UV coordinates
                    VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(20, VertexElementFormat.Vector3,
                    // Movement direction
                    VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(32, VertexElementFormat.Single,
                    // Movement speed
                    VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(36, VertexElementFormat.Single,
                    // Start time
                    VertexElementUsage.TextureCoordinate, 3)
                );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
    }

    public class ParticleSystem
    {
        private VertexBuffer verts;
        private IndexBuffer ints;

        private GraphicsDevice graphicsDevice;
        private Effect effect;

        private int nParticles;
        private Vector2 particleSize;
        private float lifespan = 1;
        private Vector3 wind;
        private Texture2D texture;
        private float fadeInTime;

        private ParticleVertex[] particles;
        private int[] indices;

        // Queue variables
        int activeStart = 0, nActive = 0;
        // Time particle system was created
        readonly DateTime start;

        public ParticleSystem(GraphicsDevice graphicsDevice,
            ContentManager content, Texture2D tex, int nParticles,
            Vector2 particleSize, float lifespan,
            Vector3 wind, float FadeInTime)
        {
            this.nParticles = nParticles;
            this.particleSize = particleSize;
            this.lifespan = lifespan;
            this.graphicsDevice = graphicsDevice;
            this.wind = wind;
            this.texture = tex;
            this.fadeInTime = FadeInTime;
            // Create vertex and index buffers to accomodate all particles
            verts = new VertexBuffer(graphicsDevice, typeof(ParticleVertex),
                nParticles*4, BufferUsage.WriteOnly);
            ints = new IndexBuffer(graphicsDevice,
                IndexElementSize.ThirtyTwoBits, nParticles*6,
                BufferUsage.WriteOnly);
            GenerateParticles();
            effect = content.Load<Effect>("ParticleEffect");
            start = DateTime.Now;
        }

        private void GenerateParticles()
        {
            // Create particle and index arrays
            particles = new ParticleVertex[nParticles*4];
            indices = new int[nParticles*6];
            Vector3 z = Vector3.Zero;

            int x = 0;
            // Initialize particle settings and fill index and vertex arrays
            for (int i = 0; i < nParticles*4; i += 4)
            {
                particles[i + 0] = new ParticleVertex(z, new Vector2(0, 0),
                    z, 0, -1);
                particles[i + 1] = new ParticleVertex(z, new Vector2(0, 1),
                    z, 0, -1);
                particles[i + 2] = new ParticleVertex(z, new Vector2(1, 1),
                    z, 0, -1);
                particles[i + 3] = new ParticleVertex(z, new Vector2(1, 0),
                    z, 0, -1);
                indices[x++] = i + 0;
                indices[x++] = i + 3;
                indices[x++] = i + 2;
                indices[x++] = i + 2;
                indices[x++] = i + 1;
                indices[x++] = i + 0;
            }
        }


        public void AddParticle(Vector3 position, Vector3 direction, float speed)
        {
            // If there are no available particles, give up
            if (nActive + 4 == nParticles*4)
                return;
            // Determine the index at which this particle should be created
            int index = OffsetIndex(activeStart, nActive);
            nActive += 4;
            // Determine the start time
            float startTime = (float) (DateTime.Now - start).TotalSeconds;
            // Set the particle settings to each of the particle's vertices
            //position = new Vector3((float)Math.Pow(position.X / 0.9, 2), (float)Math.Pow(position.Y / 0.9, 2), position.Z);
            for (int i = 0; i < 4; i++)
            {
                particles[index + i].StartPosition = position;
                particles[index + i].Direction = direction;
                particles[index + i].Speed = speed;
                particles[index + i].StartTime = startTime;
            }
        }

        // Increases the 'start' parameter by 'count' positions, wrapping
        // around the particle array if necessary
        private int OffsetIndex(int start, int count)
        {
            for (int i = 0; i < count; i++)
            {
                start++;
                if (start == particles.Length)
                    start = 0;
            }
            return start;
        }

        public void Update()
        {
            float now = (float) (DateTime.Now - start).TotalSeconds;
            int startIndex = activeStart;
            int end = nActive;
            // For each particle marked as active...
            for (int i = 0; i < end; i++)
            {
                // If this particle has gotten older than 'lifespan'...
                if (particles[activeStart].StartTime < now - lifespan)
                {
                    // Advance the active particle start position past
                    // the particle's index and reduce the number of
                    // active particles by 1
                    activeStart++;
                    nActive--;
                    if (activeStart == particles.Length)
                        activeStart = 0;
                }
            }
            // Update the vertex and index buffers
            verts.SetData(particles);
            ints.SetData(indices);
        }

        public void Draw(Matrix view, Matrix projection, Vector3 up, Vector3 right)
        {
            // Set the vertex and index buffer to the graphics card
            graphicsDevice.SetVertexBuffer(verts);
            graphicsDevice.Indices = ints;
            // Set the effect parameters
            effect.Parameters["ParticleTexture"].SetValue(texture);
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["Time"].SetValue((float)(DateTime.Now - start).
            TotalSeconds);
            effect.Parameters["Lifespan"].SetValue(lifespan);
            effect.Parameters["Wind"].SetValue(wind);
            effect.Parameters["Size"].SetValue(particleSize / 2f);
            effect.Parameters["Up"].SetValue(up);
            effect.Parameters["Side"].SetValue(right);
            effect.Parameters["FadeInTime"].SetValue(fadeInTime);
            // Enable blending render states
            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            // Apply the effect
            effect.CurrentTechnique.Passes[0].Apply();
            // Draw the billboards
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
            0, 0, nParticles * 4, 0, nParticles * 2);
            // Un-set the buffers
            graphicsDevice.SetVertexBuffer(null);
            graphicsDevice.Indices = null;
            // Reset render states
            graphicsDevice.BlendState = BlendState.Opaque;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        public void SetClipPlane(Vector4? plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(plane.HasValue);
            if (plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(plane.Value);
        }
    }
}


