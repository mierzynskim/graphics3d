using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GK1
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly List<LoadedModel> loadedModels = new List<LoadedModel>();
        private VertexPositionNormalTexture[] platformVertex;
        private VertexPositionNormalTexture[] floorVerts;
        private Effect effect;
        private Camera camera;
        private Texture2D metroTexture;
        private LightingMaterial mat;
        private TimeSpan prevLightUpdateTime;

        private readonly Random random = new Random();
        private ParticleSystem smoke;
        private KeyboardState prevKeyboardState;

        private readonly Platform platform = new Platform();

        public bool FogEnabled { get; set; }
        public float FogStart { get; set; } = 2;
        public float FogEnd { get; set; } = 10;
        public float FogIntensity { get; set; } = 0.3f;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            LoadVertices();
            effect = Content.Load<Effect>("Shader");
            smoke = new ParticleSystem(GraphicsDevice, Content,
                    Content.Load<Texture2D>("smoke"), 400, new Vector2(10), 6,
                    new Vector3(0, 2, 10), 5f);
            metroTexture = Content.Load<Texture2D>("metro");
            camera = new Camera(graphics.GraphicsDevice);
            CreateModels();
            base.Initialize();
        }

        private void CreateModels()
        {
            var bench1Position = new Vector3(-10, 0, 0);
            var bench2Position = new Vector3(10, 0, 0);
            var billboard1Position = new Vector3(-10, 10, 0);
            var billboard2Position = new Vector3(10, 10, 0);
            var suicideManPosition = new Vector3(-10, -36, 1);
            var modelsPositions = new List<Vector3>
            {
                bench1Position,
                bench2Position,
                billboard1Position,
                billboard2Position,
                suicideManPosition
            };
            mat = new LightingMaterial
            {
                AmbientColor = Color.Gray.ToVector3() * .15f,
                LightColor = new[]
                {
                    new Vector3(.85f, .85f, .85f),
                    new Vector3(.6f, .85f, .85f),
                    new Vector3(.85f, .85f, .85f),
                    new Vector3(.85f, .85f, .85f),
                },
                LightTypes = new[] { LightType.Reflector, LightType.Reflector, LightType.Point, LightType.Point },
                LightDirection =
                {
                    [0] = new Vector3(5, -10, 5),
                    [1] = new Vector3(-10, -10, 5)
                },
                LightPosition =
                {
                    [0] = new Vector3(-10, 10, 5),
                    [1] = new Vector3(10, 10, 5),
                    [2] = new Vector3(10, 0, 5),
                    [3] = new Vector3(-10, 0, 5)
                },
            };
            var benchModel = Content.Load<Model>("Bench");
            for (var i = 0; i < 2; i++)
            {
                var model = new LoadedModel(benchModel, modelsPositions[i],
                    Matrix.CreateRotationX(MathHelper.ToRadians(90f)), Matrix.CreateScale(0.009f), GraphicsDevice);
                model.SetModelEffect(effect, true);
                model.Material = mat;
                loadedModels.Add(model);
            }
            var billboard = Content.Load<Model>("billboard_a_2012");
            for (var i = 2; i < 4; i++)
            {

                var advertModel = new LoadedModel(billboard, modelsPositions[i],
                    Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(180f)),
                    Matrix.CreateScale(0.9f), GraphicsDevice);
                advertModel.SetModelEffect(effect, true);
                advertModel.Material = mat;
                loadedModels.Add(advertModel);
            }

            var man = new LoadedModel(Content.Load<Model>("Old Asian Business Man"), modelsPositions[4],
                    Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(180f)),
                    Matrix.CreateScale(0.65f), GraphicsDevice);
            man.SetModelEffect(effect, true);
            man.Material = mat;
            loadedModels.Add(man);
        }

        private void LoadVertices()
        {
            floorVerts = platform.MakeCube();
            platformVertex = platform.CreatePlatformFinal();
        }

        protected override void Update(GameTime gameTime)
        {
            var currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();
            if (currentKeyboardState.IsKeyDown(Keys.Space))
                graphics.PreferMultiSampling = !graphics.PreferMultiSampling;
            camera.Update(gameTime);
            UpdateParticle();
            UpdateFogParameters(currentKeyboardState.GetPressedKeys(), prevKeyboardState.GetPressedKeys());
            UpdateLightsColor(gameTime);
            prevKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        private void UpdateParticle()
        {
            // Generate a direction within 15 degrees of (0, 1, 0)
            var offset = new Vector3(MathHelper.ToRadians(10.0f));
            var randAngle = Vector3.Up + RandVec3(-offset, offset);
            // Generate a position between (-400, 0, -400) and (400, 0, 400)
            var randPosition = RandVec3(new Vector3(-5), new Vector3(5));
            // Generate a speed between 600 and 900
            var randSpeed = (float)random.NextDouble() * 3 + 6;
            //ps.AddParticle(randPosition, randAngle, randSpeed);
            smoke.AddParticle(randPosition + new Vector3(0, 10, 0), randAngle, randSpeed);
            smoke.Update();
            //ps.Update();
        }

        // Returns a random Vector3 between min and max
        private Vector3 RandVec3(Vector3 min, Vector3 max)
        {
            return new Vector3(
            min.X + (float)random.NextDouble() * (max.X - min.X),
            min.Y + (float)random.NextDouble() * (max.Y - min.Y),
            min.Z + (float)random.NextDouble() * (max.Z - min.Z));
        }

        private void UpdateFogParameters(Keys[] pressedKeys, Keys[] prevPressedKeys)
        {
            if (pressedKeys.Contains(Keys.F) && !prevPressedKeys.Contains(Keys.F))
            {
                FogEnabled = !FogEnabled;
                loadedModels.First().Material.FogEnabled = !loadedModels.First().Material.FogEnabled;
            }

            if (pressedKeys.Contains(Keys.B) && !prevPressedKeys.Contains(Keys.B))
            {
                FogStart += 5f;
                    loadedModels.First().Material.FogStart += 5f;
            }

            if (pressedKeys.Contains(Keys.V) && !prevPressedKeys.Contains(Keys.V))
            {
                FogStart -= 5f;
                    loadedModels.First().Material.FogStart -= 5f;
            }

            if (pressedKeys.Contains(Keys.X) && !prevPressedKeys.Contains(Keys.X))
            {
                FogEnd += 5f;
                    loadedModels.First().Material.FogEnd += 5f;
            }

            if (pressedKeys.Contains(Keys.Z) && !prevPressedKeys.Contains(Keys.Z))
            {
                FogEnd -= 5f;
                    loadedModels.First().Material.FogEnd -= 5f;
            }

            if (pressedKeys.Contains(Keys.N) && !prevPressedKeys.Contains(Keys.N))
            {
                FogIntensity += 0.1f;
                loadedModels.First().Material.FogIntensity += 0.1f;
            }

            if (pressedKeys.Contains(Keys.M) && !prevPressedKeys.Contains(Keys.M))
            {
                FogIntensity -= 0.1f;
                loadedModels.First().Material.FogIntensity -= 0.1f;
            }
        }

        private void UpdateLightsColor(GameTime gameTime)
        {
            var black = Color.Black.ToVector3();
            var light = new Vector3(.85f, .85f, .85f);
            var r = new Random();
            if (gameTime.TotalGameTime.TotalMilliseconds - prevLightUpdateTime.TotalMilliseconds > 500)
            {
                if (r.Next(2) == 0)
                    mat.LightColor[2] = black;
                else
                    mat.LightColor[2] = light;
                prevLightUpdateTime = gameTime.TotalGameTime;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            effect.Parameters["CameraPosition"].SetValue(camera.Position);
            DrawGround();
            DrawPlatform();
            foreach (var cModel in loadedModels)
            {
                cModel.Material = mat;
                cModel.Draw(camera);
            }
            smoke.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.Up, camera.Right);


            base.Draw(gameTime);
        }

        private void DrawGround()
        {
            effect.Parameters[nameof(FogEnabled)]?.SetValue(FogEnabled);
            effect.Parameters[nameof(FogStart)]?.SetValue(FogStart);
            effect.Parameters[nameof(FogEnd)]?.SetValue(FogEnd);
            effect.Parameters[nameof(FogIntensity)]?.SetValue(FogIntensity);
            effect.Parameters["LightDirection"]?.SetValue(mat.LightDirection);
            effect.Parameters["LightPosition"]?.SetValue(mat.LightPosition);

            effect.Parameters["LightColor"]?.SetValue(mat.LightColor);

            effect.Parameters["SpecularColor"]?.SetValue(mat.SpecularColor);

            effect.Parameters["LightAttenuation"]?.SetValue(
                mat.LightAttenuation);

            effect.Parameters["LightFalloff"]?.SetValue(mat.LightFalloff);
            effect.Parameters["LightTypes"]?.SetValue(mat.LightTypes.Select(x => (float)x).ToArray());

            effect.Parameters["AmbientColor"].SetValue(Color.Black.ToVector3());
            effect.Parameters["BasicTexture"].SetValue(metroTexture);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["TextureEnabled"].SetValue(true);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(40f) * Matrix.CreateTranslation(new Vector3(0, 0, 40f)) * camera.WorldMatrix);

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

        private void DrawPlatform()
        {
            effect.Parameters[nameof(FogEnabled)]?.SetValue(FogEnabled);
            effect.Parameters[nameof(FogStart)]?.SetValue(FogStart);
            effect.Parameters[nameof(FogEnd)]?.SetValue(FogEnd);
            effect.Parameters[nameof(FogIntensity)]?.SetValue(FogIntensity);

            effect.Parameters["LightDirection"]?.SetValue(mat.LightDirection);
            effect.Parameters["LightPosition"]?.SetValue(mat.LightPosition);

            effect.Parameters["LightColor"]?.SetValue(mat.LightColor);

            effect.Parameters["SpecularColor"]?.SetValue(mat.SpecularColor);

            effect.Parameters["LightAttenuation"]?.SetValue(
                mat.LightAttenuation);

            effect.Parameters["LightFalloff"]?.SetValue(mat.LightFalloff);
            effect.Parameters["LightTypes"]?.SetValue(mat.LightTypes.Select(x => (float)x).ToArray());

            effect.Parameters["AmbientColor"].SetValue(Color.Black.ToVector3());
            effect.Parameters["BasicTexture"].SetValue(metroTexture);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["TextureEnabled"].SetValue(true);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            effect.Parameters["World"].SetValue(Matrix.CreateScale(0.2f) * Matrix.CreateRotationY(MathHelper.ToRadians(90f)) * Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * Matrix.CreateTranslation(new Vector3(0, -37, 15f)) * camera.WorldMatrix);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphics.GraphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    platformVertex,
                    0,
                    platformVertex.Length / 3);
            }
        }




    }
}


