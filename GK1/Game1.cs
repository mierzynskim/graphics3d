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
            floorVerts = new VertexPositionNormalTexture[6];

            floorVerts[0].Position = new Vector3(-20, -20, 0);
            floorVerts[1].Position = new Vector3(-20, 20, 0);
            floorVerts[2].Position = new Vector3(20, -20, 0);

            floorVerts[3].Position = floorVerts[1].Position;
            floorVerts[4].Position = new Vector3(20, 20, 0);
            floorVerts[5].Position = floorVerts[2].Position;

            //floorVerts[6].Position = new Vector3(-20, -20, 5);
            //floorVerts[7].Position = new Vector3(-20, 20, 5);
            //floorVerts[8].Position = new Vector3(20, -20, 5);

            //floorVerts[9].Position = floorVerts[7].Position;
            //floorVerts[10].Position = new Vector3(20, 20, 5);
            //floorVerts[11].Position = floorVerts[8].Position;
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
            DrawModel(benchModel, camera.ProjectionMatrix, camera.ViewMatrix, camera.ProjectionMatrix);

            base.Draw(gameTime);
        }

        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = bench.GetWorld(camera.WorldMatrix, projection);
                    effect.View = view;
                    effect.Projection = projection;
                }

                mesh.Draw();
            }
        }

        void DrawGround()
        {
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;



            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    floorVerts,
                    0,
                    2);
            }
        }
    }
}
