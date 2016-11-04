using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GK1
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;

        VertexPositionNormalTexture[] floorVerts;

        BasicEffect effect;

        // New camera code
        Camera camera;
        private Bench bench;

        private Model benchModel;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            LoadVertices();
            effect = new BasicEffect(graphics.GraphicsDevice);
            effect.AmbientLightColor = new Vector3(0.0f, 1.0f, 0.0f);


            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = Vector3.One;
            effect.DirectionalLight0.Direction = Vector3.Normalize(Vector3.One);
            effect.LightingEnabled = true;

            camera = new Camera(graphics.GraphicsDevice);
            bench = new Bench();

            benchModel = Content.Load<Model>("benchSeat");

            base.Initialize();
        }

        private void LoadVertices()
        {
            floorVerts = MakeCube();
        }

        protected override void LoadContent()
        {
            //using (var stream = TitleContainer.OpenStream("Content/checkerboard.png"))
            //{
            //    checkerboardTexture = Texture2D.FromStream(this.GraphicsDevice, stream);
            //}
        }

        protected override void Update(GameTime gameTime)
        {
            //robot.Update(gameTime);
            // New camera code
            camera.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            DrawGround();
            DrawModel(new Vector3(0, 0, 0));
            DrawModel(new Vector3(40, 0, 40));

            base.Draw(gameTime);
        }

        private void DrawModel(Vector3 position)
        {
            foreach (var mesh in benchModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = Matrix.CreateTranslation(position) * bench.GetWorld(camera.WorldMatrix);
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                }

                mesh.Draw();
            }
        }

        void DrawGround()
        {
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;
            effect.World = Matrix.CreateScale(15f)* Matrix.CreateTranslation(new Vector3(0, 0, 15f)) * camera.WorldMatrix; 


            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    floorVerts,
                    0,
                    floorVerts.Length / 3);
            }
        }

        protected VertexPositionNormalTexture[] MakeCube()
        {
            var vertices = new VertexPositionNormalTexture[36];
            var texcoords = new Vector2(0f, 0f);

            var face = new Vector3[6];
            //TopLeft
            face[0] = new Vector3(-1f, 1f, 0.0f);
            //BottomLeft
            face[1] = new Vector3(-1f, -1f, 0.0f);
            //TopRight
            face[2] = new Vector3(1f, 1f, 0.0f);
            //BottomLeft
            face[3] = new Vector3(-1f, -1f, 0.0f);
            //BottomRight
            face[4] = new Vector3(1f, -1f, 0.0f);
            //TopRight
            face[5] = new Vector3(1f, 1f, 0.0f);

            //front face
            for (int i = 0; i <= 2; i++)
            {
                vertices[i] = new VertexPositionNormalTexture(face[i] + Vector3.UnitZ, Vector3.UnitZ, texcoords);
                vertices[i + 3] = new VertexPositionNormalTexture(face[i + 3] + Vector3.UnitZ, Vector3.UnitZ, texcoords);
            }

            //back face

            for (int i = 0; i <= 2; i++)
            {
                vertices[i + 6] = new VertexPositionNormalTexture(face[2 - i] - Vector3.UnitZ, -Vector3.UnitZ, texcoords);
                vertices[i + 6 + 3] = new VertexPositionNormalTexture(face[5 - i] - Vector3.UnitZ, -Vector3.UnitZ, texcoords);
            }

            //left face
            Matrix RotY90 = Matrix.CreateRotationY(-(float)Math.PI / 2f);
            for (int i = 0; i <= 2; i++)
            {
                vertices[i + 12] = new VertexPositionNormalTexture(Vector3.Transform(face[i], RotY90) - Vector3.UnitX, -Vector3.UnitX, texcoords);
                vertices[i + 12 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[i + 3], RotY90) - Vector3.UnitX, -Vector3.UnitX, texcoords);
            }

            //Right face

            for (int i = 0; i <= 2; i++)
            {
                vertices[i + 18] = new VertexPositionNormalTexture(Vector3.Transform(face[2 - i], RotY90) + Vector3.UnitX, Vector3.UnitX, texcoords);
                vertices[i + 18 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[5 - i], RotY90) + Vector3.UnitX, Vector3.UnitX, texcoords);

            }

            //Top face

            Matrix RotX90 = Matrix.CreateRotationX(-(float)Math.PI / 2f);
            for (int i = 0; i <= 2; i++)
            {
                vertices[i + 24] = new VertexPositionNormalTexture(Vector3.Transform(face[i], RotX90) + Vector3.UnitY, Vector3.UnitY, texcoords);
                vertices[i + 24 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[i + 3], RotX90) + Vector3.UnitY, Vector3.UnitY, texcoords);

            }

            //Bottom face

            for (int i = 0; i <= 2; i++)
            {
                vertices[i + 30] = new VertexPositionNormalTexture(Vector3.Transform(face[2 - i], RotX90) - Vector3.UnitY, -Vector3.UnitY, texcoords);
                vertices[i + 30 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[5 - i], RotX90) - Vector3.UnitY, -Vector3.UnitY, texcoords);
            }

            return vertices;
        }
    }
}
