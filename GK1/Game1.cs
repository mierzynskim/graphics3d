using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GK1
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private VertexPositionNormalTexture[] floorVerts;
        private Effect effect;
        private Camera camera;
        private List<CModel> models = new List<CModel>();
        private Texture2D metroTexture;
        private LightingMaterial mat;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            LoadVertices();
            effect = Content.Load<Effect>("Shader");
            metroTexture = Content.Load<Texture2D>("metro");

            camera = new Camera(graphics.GraphicsDevice);
            var modelsPositions = new List<Vector3> { new Vector3(-10, 0, 0), new Vector3(10, 0, 0) };
            mat = new LightingMaterial
            {
                AmbientColor = Color.Gray.ToVector3() * .15f,
                LightColor = new[] {
                            new Vector3(0.5f, 0.5f, 0.5f),
                            new Vector3(0.5f, 0.5f, 0.5f),
                            new Vector3(0.5f, 0.5f, 0.5f) },
                LightTypes = new[] { LightType.Reflector, LightType.Point, LightType.Point },
                LightDirection = new Vector3[] { Vector3.One, Vector3.UnitZ, Vector3.UnitZ },
                LightPosition = new Vector3[] { Vector3.Zero, new Vector3(0, 30, 0), new Vector3(10, 10, 10) }
            };
            for (var i = 0; i < 2; i++)
            {
                var model = new CModel(Content.Load<Model>("Bench"), modelsPositions[i], Matrix.CreateRotationX(MathHelper.ToRadians(90f)), Matrix.CreateScale(0.009f), GraphicsDevice);
                model.SetModelEffect(effect, true);
                model.Material = mat;
                models.Add(model);
            }

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
            camera.Update(gameTime);
            UpdateLightsColor(gameTime);
            base.Update(gameTime);
        }

        private void UpdateLightsColor(GameTime gameTime)
        {
            var r = new Random();
            if (gameTime.TotalGameTime.TotalMilliseconds - PrevLightUpdateTime.TotalMilliseconds > 500)
            {
                var newColor = new Color(r.Next(255), r.Next(255), r.Next(255));
                mat.LightColor[0] = newColor.ToVector3();
                PrevLightUpdateTime = gameTime.TotalGameTime;
            }
        }

        public TimeSpan PrevLightUpdateTime { get; set; }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            effect.Parameters["CameraPosition"].SetValue(camera.Position);
            DrawGround();
            foreach (var cModel in models)
            {
                cModel.Material = mat;
                cModel.Draw(camera);
            }

            base.Draw(gameTime);
        }

        private void DrawGround()
        {
            //effect.Parameters["AmbientColor"].SetValue(Color.Black.ToVector3());
            effect.Parameters["BasicTexture"].SetValue(metroTexture);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["TextureEnabled"].SetValue(true);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(15f) * Matrix.CreateTranslation(new Vector3(0, 0, 15f)) * camera.WorldMatrix);


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
            var texcoordsArr = new[] { new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f) };

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

            for (int i = 0, j = 0; i <= 2; i++)
            {
                vertices[i + 6] = new VertexPositionNormalTexture(face[2 - i] - Vector3.UnitZ, -Vector3.UnitZ, texcoordsArr[j++]);
                vertices[i + 6 + 3] = new VertexPositionNormalTexture(face[5 - i] - Vector3.UnitZ, -Vector3.UnitZ, texcoordsArr[j++]);
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

            for (int i = 0, j = 0; i <= 2; i++)
            {
                vertices[i + 30] = new VertexPositionNormalTexture(Vector3.Transform(face[2 - i], RotX90) - Vector3.UnitY, -Vector3.UnitY, texcoordsArr[j++]);
                vertices[i + 30 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[5 - i], RotX90) - Vector3.UnitY, -Vector3.UnitY, texcoordsArr[j++]);
            }

            return vertices;
        }
    }
}


