using System;
using System.Collections.Generic;
using System.Linq;
using GK1.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GK1
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly List<CModel> loadedModels = new List<CModel>();
        private VertexPositionNormalTexture[] platformVertex;
        private VertexPositionNormalTexture[] floorVerts;
        private Effect effect;
        private Camera camera;
        private Texture2D metroTexture;
        private LightingMaterial mat;
        private TimeSpan prevLightUpdateTime;

        private readonly Random random = new Random();
        private ParticleSystem smoke;
        private BillboardSystem man;
        private KeyboardState prevKeyboardState;

        private readonly Platform platform = new Platform();
        private Water water;

        private Vector4 clipPlane = new Vector4(0, 10, 0, 0);
        private bool clipEnabled;
        private Texture benchWoodenTexture;
        private Dictionary<ModelMesh, Texture> benchModelTexture;

        public bool FogEnabled { get; set; }
        public float FogStart { get; set; } = 2;
        public float FogEnd { get; set; } = 10;
        public float FogIntensity { get; set; } = 0.3f;

        public float MipMapLevelOfDetailBias { get; set; }

        private FilterLevel MinFilter { get; set; } = FilterLevel.Anisotropic;
        private FilterLevel MagFilter { get; set; } = FilterLevel.Anisotropic;
        private FilterLevel MipFilter { get; set; } = FilterLevel.Anisotropic;

        private enum FilterLevel
        {
            Point = 0,
            Linear = 1,
            Anisotropic = 2
        }



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
                    Content.Load<Texture2D>("smoke"), 400, new Vector2(30), 6,
                    new Vector3(0, 0, 15), 10f);
            // Generate random tree positions
            var r = new Random();
            var positions = new Vector3[15];
            for (int i = 0; i < positions.Length; i++)
                positions[i] = new Vector3(r.Next(-40, 40), r.Next(25, 30), 2.5f);
            man = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("man2"), new Vector2(5), positions);
            metroTexture = Content.Load<Texture2D>("metro");

            camera = new Camera(graphics.GraphicsDevice);
            CreateModels();
            water = new Water(Content, GraphicsDevice, new Vector3(0, 0, 0), Vector2.Zero);
            foreach (var loadedModel in loadedModels)
                water.Objects.Add(loadedModel);

            //graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            //graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            //graphics.IsFullScreen = true;
            //graphics.ApplyChanges();
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
            AddPlatformAndStationModels();
            AddBenchModel(modelsPositions);
            AddBillboardModels(modelsPositions);

            var man = new LoadedModel(Content.Load<Model>("Old Asian Business Man"), modelsPositions[4],
                    Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(180f)),
                    Matrix.CreateScale(0.65f), GraphicsDevice);
            man.SetModelEffect(effect, true);
            man.Material = mat;
            loadedModels.Add(man);

        }

        private void AddPlatformAndStationModels()
        {
            var userDefinedModel = new UserDefinedModel(GraphicsDevice, new Vector3(0, 0, 40f), Matrix.Identity,
                Matrix.CreateScale(40f), floorVerts, metroTexture);
            userDefinedModel.SetModelEffect(effect, true);
            userDefinedModel.Material = mat;
            loadedModels.Add(userDefinedModel);
            userDefinedModel = new UserDefinedModel(GraphicsDevice, new Vector3(0, -37, 15f),
                Matrix.CreateRotationY(MathHelper.ToRadians(90f))*Matrix.CreateRotationX(MathHelper.ToRadians(90f)),
                Matrix.CreateScale(0.2f), platformVertex, null);
            userDefinedModel.SetModelEffect(effect, true);
            userDefinedModel.Material = mat;
            loadedModels.Add(userDefinedModel);
        }

        private void AddBillboardModels(List<Vector3> modelsPositions)
        {
            var billboard = Content.Load<Model>("billboard_a_2012");
            for (var i = 2; i < 4; i++)
            {
                var advertModel = new LoadedModel(billboard, modelsPositions[i],
                    Matrix.CreateRotationX(MathHelper.ToRadians(90f))*Matrix.CreateRotationZ(MathHelper.ToRadians(180f)),
                    Matrix.CreateScale(0.9f), GraphicsDevice);
                advertModel.SetModelEffect(effect, true);
                advertModel.Material = mat;
                loadedModels.Add(advertModel);
            }
        }

        private void AddBenchModel(List<Vector3> modelsPositions)
        {
            var benchModel = Content.Load<Model>("Bench");
            benchWoodenTexture = Content.Load<Texture>("benchTexture");
            benchModelTexture = new Dictionary<ModelMesh, Texture>();
            var j = 0;
            int[] woodenElementsIndexes = { 0, 1, 2, 19, 20, 21, 22, 23 };
            foreach (var mesh in benchModel.Meshes)
            {
                if (woodenElementsIndexes.Any(e => e == j))
                    benchModelTexture.Add(mesh, benchWoodenTexture);
                j++;
            }
            for (var i = 0; i < 2; i++)
            {
                var model = new CustomTexturedLoadedModel(benchModel, modelsPositions[i],
                    Matrix.CreateRotationX(MathHelper.ToRadians(90f)), Matrix.CreateScale(0.009f), GraphicsDevice, benchModelTexture);
                model.SetModelEffect(effect, true);
                model.Material = mat;
                loadedModels.Add(model);
            }
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
            UpdateClip(currentKeyboardState.GetPressedKeys(), prevKeyboardState.GetPressedKeys());
            UpdateLightsColor(gameTime);
            UpdateTexturesParameters(currentKeyboardState.GetPressedKeys(), prevKeyboardState.GetPressedKeys());
            prevKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        private void UpdateParticle()
        {
            // Generate a direction within 15 degrees of (0, 1, 0)
            var offset = new Vector3(MathHelper.ToRadians(15.0f));
            var randAngle = Vector3.Up + RandVec3(-offset, offset);
            // Generate a position between (-400, 0, -400) and (400, 0, 400)
            var randPosition = RandVec3(new Vector3(-1), new Vector3(1));
            // Generate a speed between 600 and 900
            var randSpeed = (float)random.NextDouble() * 3 + 6;
            //ps.AddParticle(randPosition, randAngle, randSpeed);
            smoke.AddParticle(randPosition + new Vector3(0, 5, 0), randAngle, randSpeed);
            smoke.Update();
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

        private void UpdateClip(Keys[] pressedKeys, Keys[] prevPressedKeys)
        {
            if (pressedKeys.Contains(Keys.C) && !prevPressedKeys.Contains(Keys.C))
            {
                clipEnabled = !clipEnabled;
                if (clipEnabled)
                {
                    foreach (var loadedModel in loadedModels)
                        loadedModel.SetClipPlane(clipPlane);
                    man.SetClipPlane(clipPlane);
                    smoke.SetClipPlane(clipPlane);
                }
                else
                {
                    foreach (var loadedModel in loadedModels)
                        loadedModel.SetClipPlane(null);
                    man.SetClipPlane(null);
                    smoke.SetClipPlane(null);
                }
            }

            if (pressedKeys.Contains(Keys.Left) && !prevPressedKeys.Contains(Keys.Left))
            {
                clipPlane.W -= 10;
                foreach (var loadedModel in loadedModels)
                    loadedModel.SetClipPlane(clipPlane);
            }

            if (pressedKeys.Contains(Keys.Right) && !prevPressedKeys.Contains(Keys.Right))
            {
                clipPlane.W += 10;
                foreach (var loadedModel in loadedModels)
                    loadedModel.SetClipPlane(clipPlane);
            }

        }

        private void UpdateTexturesParameters(Keys[] pressedKeys, Keys[] prevPressedKeys)
        {
            if (pressedKeys.Contains(Keys.P) && !prevPressedKeys.Contains(Keys.P))
                Multisampling = !Multisampling;
            if (pressedKeys.Contains(Keys.OemMinus))
                MipMapLevelOfDetailBias = MipMapLevelOfDetailBias - 0.1f;

            if (pressedKeys.Contains(Keys.OemPlus))
                MipMapLevelOfDetailBias = MipMapLevelOfDetailBias + 0.1f;


            if (pressedKeys.Contains(Keys.OemCloseBrackets) && !prevPressedKeys.Contains(Keys.OemCloseBrackets))
                MagFilter = (FilterLevel) (((int) MagFilter + 1)%3);

            if (pressedKeys.Contains(Keys.OemOpenBrackets) && !prevPressedKeys.Contains(Keys.OemOpenBrackets))
                MipFilter = (FilterLevel) (((int) MipFilter + 1)%3);

        }

        private bool Multisampling { get; set; }

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
            //water.PreDraw(camera, gameTime);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            effect.Parameters["CameraPosition"].SetValue(camera.Position);

            var ss = new SamplerState
            {
                Filter = TextureFilterFromMinMagMip(MinFilter, MagFilter, MipFilter),
                MaxAnisotropy = 16,
                MipMapLevelOfDetailBias = MipMapLevelOfDetailBias,
                AddressU = TextureAddressMode.Mirror,
                AddressW = TextureAddressMode.Mirror,
                AddressV = TextureAddressMode.Mirror,
                BorderColor = Color.Black
            };

            var originalRasterizerState = graphics.GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState { CullMode = CullMode.None };

            graphics.GraphicsDevice.RasterizerState = rasterizerState;
            graphics.GraphicsDevice.RasterizerState = originalRasterizerState;

            graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            graphics.PreferMultiSampling = Multisampling;
            graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = Multisampling ? 4 : 0;
            graphics.GraphicsDevice.RasterizerState = new RasterizerState
            {
                MultiSampleAntiAlias = Multisampling,
                CullMode = CullMode.CullCounterClockwiseFace
            };
            for (int i = 0; i < 15; i++)
                graphics.GraphicsDevice.SamplerStates[i] = ss;
            

            graphics.ApplyChanges();
            if (clipEnabled)
                DrawWithClipPlane();
            else
            {
                foreach (var cModel in loadedModels)
                {
                    cModel.Material = mat;
                    cModel.Draw(camera);
                }
                smoke.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.Up, camera.Right);
                man.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.WorldMatrix, camera.Up, camera.Right);
            }
            //water.RenderReflection(camera);

            base.Draw(gameTime);
        }

        private void DrawWithClipPlane()
        {
            var previousRasterizerState = GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState { FillMode = FillMode.WireFrame };
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (var cModel in loadedModels)
            {
                cModel.Material = mat;
                cModel.Draw(camera);
            }
            smoke.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.Up, camera.Right);
            man.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.WorldMatrix, camera.Up, camera.Right);
            GraphicsDevice.RasterizerState = previousRasterizerState;

            foreach (var cModel in loadedModels)
            {
                cModel.SetClipPlane(-clipPlane);
                cModel.Material = mat;
                cModel.Draw(camera);
            }
            smoke.SetClipPlane(-clipPlane);
            smoke.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.Up, camera.Right);
            man.SetClipPlane(-clipPlane);
            man.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.WorldMatrix, camera.Up, camera.Right);

            foreach (var cModel in loadedModels)
                cModel.SetClipPlane(clipPlane);
            man.SetClipPlane(clipPlane);
            smoke.SetClipPlane(clipPlane);
        }

        private TextureFilter TextureFilterFromMinMagMip(FilterLevel minFilter, FilterLevel magFilter, FilterLevel mipFilter)
        {
            var def = TextureFilter.Point;

            if (minFilter == FilterLevel.Anisotropic && magFilter == FilterLevel.Anisotropic)
                return TextureFilter.Anisotropic;

            if (minFilter == FilterLevel.Point)
            {
                if (magFilter == FilterLevel.Point)
                {
                    if (mipFilter == FilterLevel.Point)
                        return TextureFilter.Point;
                    return def;
                }
                if (mipFilter == FilterLevel.Point)
                    return TextureFilter.MinPointMagLinearMipPoint;
                return TextureFilter.MinPointMagLinearMipLinear;
            }
            if (magFilter == FilterLevel.Point)
            {
                if (mipFilter == FilterLevel.Point)
                    return TextureFilter.MinLinearMagPointMipPoint;
                return TextureFilter.MinLinearMagPointMipLinear;
            }
            if (mipFilter == FilterLevel.Point)
                return TextureFilter.LinearMipPoint;
            return TextureFilter.Linear;
        }
    }
}


