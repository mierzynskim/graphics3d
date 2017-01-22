using System;
using System.Collections.Generic;
using System.Linq;
using GK1.Bloom;
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
        private Camera.Camera camera;
        private Texture2D metroTexture;
        private LightingMaterial mat;
        private TimeSpan prevLightUpdateTime;

        private readonly Random random = new Random();
        private ParticleSystem smoke;
        private BillboardSystem manBillboardSystem;
        private KeyboardState prevKeyboardState;

        private readonly Platform platform = new Platform();
        private Mirror mirror;

        private Vector4 clipPlane = new Vector4(0, 10, 0, 0);
        private bool clipEnabled;
        private Texture benchWoodenTexture;
        private Dictionary<ModelMesh, Texture> benchModelTexture;

        private float mipMapLevelOfDetailBias;
        private FilterLevel minFilter = FilterLevel.Anisotropic;
        private FilterLevel magFilter = FilterLevel.Anisotropic;
        private FilterLevel mipFilter = FilterLevel.Anisotropic;
        private bool multisampling;

        private BloomComponent bloom;
        private SpriteBatch spriteBatch;

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
            //bloom = new BloomComponent(this);
            //Components.Add(bloom);
        }

        protected override void Initialize()
        {
            LoadVertices();
            effect = Content.Load<Effect>("Shader");
            liquidEffect = Content.Load<Effect>("Liquid");
            CreateBillboardSystems();
            metroTexture = Content.Load<Texture2D>("metro");
            initialTexture = Content.Load<Texture2D>("Texture");
            perlinNoiseTexture = Content.Load<Texture2D>("perlinNoise");
            camera = new Camera.Camera(graphics.GraphicsDevice);
            CreateModels();
            CreateMirror();
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            var pp = graphics.GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(graphics.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, graphics.GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);

            renderTargetLiquid1 = new RenderTarget2D(graphics.GraphicsDevice, initialTexture.Width, initialTexture.Height, true, initialTexture.Format, DepthFormat.Depth24);
            renderTargetLiquid2 = new RenderTarget2D(graphics.GraphicsDevice, initialTexture.Width, initialTexture.Height, true, initialTexture.Format, DepthFormat.Depth24);
        }

        private void CreateMirror()
        {
            mirror = new Mirror(Content, GraphicsDevice, new Vector3(-20, 0, 10f), Vector2.Zero);
            foreach (var loadedModel in loadedModels)
                mirror.Objects.Add(loadedModel);
        }

        private void CreateBillboardSystems()
        {
            smoke = new ParticleSystem(GraphicsDevice, Content,
                Content.Load<Texture2D>("smoke"), 400, new Vector2(30), 6,
                new Vector3(0, 0, 15), 10f);
            var r = new Random();
            var positions = new Vector3[15];
            for (var i = 0; i < positions.Length; i++)
                positions[i] = new Vector3(r.Next(-40, 40), r.Next(25, 30), 2.5f);
            manBillboardSystem = new BillboardSystem(GraphicsDevice, Content, Content.Load<Texture2D>("man2"), new Vector2(5),
                positions);
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
            AddManModel(modelsPositions);
        }

        private void AddManModel(List<Vector3> modelsPositions)
        {
            //var manModel = new LoadedModel(Content.Load<Model>("Old Asian Business Man"), modelsPositions[4],
            //    Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(180f)),
            //    Matrix.CreateScale(0.65f), GraphicsDevice);
            //manModel.SetModelEffect(effect, true);
            //manModel.Material = mat;
            //loadedModels.Add(manModel);
        }

        private void AddPlatformAndStationModels()
        {
            var userDefinedModel = new UserDefinedModel(GraphicsDevice, new Vector3(0, 0, 40f), Matrix.Identity,
                Matrix.CreateScale(40f), floorVerts, metroTexture);
            userDefinedModel.SetModelEffect(effect, true);
            userDefinedModel.Material = mat;
            loadedModels.Add(userDefinedModel);
            userDefinedModel = new UserDefinedModel(GraphicsDevice, new Vector3(0, -37, 15f),
                Matrix.CreateRotationY(MathHelper.ToRadians(90f)) * Matrix.CreateRotationX(MathHelper.ToRadians(90f)),
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
                    Matrix.CreateRotationX(MathHelper.ToRadians(90f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(180f)),
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
            if (currentKeyboardState.GetPressedKeys().Contains(Keys.Enter)) resetLiquidTexture = true;
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
            var offset = new Vector3(MathHelper.ToRadians(15.0f));
            var randAngle = Vector3.Up + RandVec3(-offset, offset);
            var randPosition = RandVec3(new Vector3(-1), new Vector3(1));
            var randSpeed = (float)random.NextDouble() * 3 + 6;
            smoke.AddParticle(randPosition + new Vector3(0, 5, 0), randAngle, randSpeed);
            smoke.Update();
        }

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
                loadedModels.First().Material.FogEnabled = !loadedModels.First().Material.FogEnabled;

            if (pressedKeys.Contains(Keys.B) && !prevPressedKeys.Contains(Keys.B))
                loadedModels.First().Material.FogStart += 5f;

            if (pressedKeys.Contains(Keys.V) && !prevPressedKeys.Contains(Keys.V))
                loadedModels.First().Material.FogStart -= 5f;

            if (pressedKeys.Contains(Keys.X) && !prevPressedKeys.Contains(Keys.X))
                loadedModels.First().Material.FogEnd += 5f;

            if (pressedKeys.Contains(Keys.Z) && !prevPressedKeys.Contains(Keys.Z))
                loadedModels.First().Material.FogEnd -= 5f;

            if (pressedKeys.Contains(Keys.N) && !prevPressedKeys.Contains(Keys.N))
                loadedModels.First().Material.FogIntensity += 0.1f;

            if (pressedKeys.Contains(Keys.M) && !prevPressedKeys.Contains(Keys.M))
                loadedModels.First().Material.FogIntensity -= 0.1f;
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
                    manBillboardSystem.SetClipPlane(clipPlane);
                    smoke.SetClipPlane(clipPlane);
                }
                else
                {
                    foreach (var loadedModel in loadedModels)
                        loadedModel.SetClipPlane(null);
                    manBillboardSystem.SetClipPlane(null);
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
                multisampling = !multisampling;
            if (pressedKeys.Contains(Keys.OemMinus))
                mipMapLevelOfDetailBias = mipMapLevelOfDetailBias - 0.1f;

            if (pressedKeys.Contains(Keys.OemPlus))
                mipMapLevelOfDetailBias = mipMapLevelOfDetailBias + 0.1f;


            if (pressedKeys.Contains(Keys.OemCloseBrackets) && !prevPressedKeys.Contains(Keys.OemCloseBrackets))
                magFilter = (FilterLevel)(((int)magFilter + 1) % 3);

            if (pressedKeys.Contains(Keys.OemOpenBrackets) && !prevPressedKeys.Contains(Keys.OemOpenBrackets))
                mipFilter = (FilterLevel)(((int)mipFilter + 1) % 3);

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
            effect.Parameters["CameraPosition"].SetValue(camera.Position);
            //mirror.PreDraw(camera, gameTime);
            SetRasterizerParameters();
            //bloom?.BeginDraw();
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //mirror.Draw(camera);
            CreateLiquidText(gameTime);
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
                manBillboardSystem.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.Right);
            }
            //bloom?.Draw(gameTime);



            base.Draw(gameTime);
        }

        private RenderTarget2D renderTarget;

        private bool switchTargetFlag = true;
        private bool resetLiquidTexture = true;
        private RenderTarget2D renderTargetLiquid1;
        private RenderTarget2D renderTargetLiquid2;

        private RenderTarget2D CurrentTarget => switchTargetFlag ? renderTargetLiquid1 : renderTargetLiquid2;
        private RenderTarget2D PreviousTarget => switchTargetFlag ? renderTargetLiquid2 : renderTargetLiquid1;

        private Texture2D initialTexture;
        private Texture2D perlinNoiseTexture;
        private Effect liquidEffect;
        private void CreateLiquidText(GameTime gameTime)
        {
            var time = (float)gameTime.TotalGameTime.TotalSeconds;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var prevWidth = graphics.PreferredBackBufferWidth;
            var prevHeight = graphics.PreferredBackBufferHeight;

            graphics.PreferredBackBufferWidth = initialTexture.Width;
            graphics.PreferredBackBufferHeight = initialTexture.Height;
            graphics.ApplyChanges();

            var liquidRectangle = new Rectangle(0, 0, initialTexture.Width, initialTexture.Height);

            if (resetLiquidTexture)
            {
                graphics.GraphicsDevice.SetRenderTarget(PreviousTarget);

                spriteBatch.Begin();
                spriteBatch.Draw(initialTexture, liquidRectangle, Color.White);
                spriteBatch.End();

                renderTarget = PreviousTarget;
                if (renderTarget != null) (loadedModels[0] as UserDefinedModel).LiquidTexture = renderTarget;
                resetLiquidTexture = false;
            }

            graphics.GraphicsDevice.SetRenderTarget(CurrentTarget);

            liquidEffect.CurrentTechnique = liquidEffect.Techniques["LiquidTechnique"];
            var pixelSize = new Vector2((float)1 / initialTexture.Width, (float)1 / initialTexture.Height);

            liquidEffect.Parameters["PixelSize"].SetValue(pixelSize);
            liquidEffect.Parameters["Perlin"].SetValue(perlinNoiseTexture);
            liquidEffect.Parameters["time"].SetValue(time);
            liquidEffect.Parameters["dt"].SetValue(dt);

            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, liquidEffect);
            spriteBatch.Draw(PreviousTarget, liquidRectangle, Color.White);
            spriteBatch.End();

            renderTarget = CurrentTarget;
            if (renderTarget != null) (loadedModels[0] as UserDefinedModel).LiquidTexture = renderTarget;
            switchTargetFlag = !switchTargetFlag;

            graphics.PreferredBackBufferWidth = prevWidth;
            graphics.PreferredBackBufferHeight = prevHeight;
            graphics.ApplyChanges();
            graphics.GraphicsDevice.SetRenderTarget(null);
        }

        private void SetRasterizerParameters()
        {
            var ss = new SamplerState
            {
                Filter = TextureFilterFromMinMagMip(minFilter, magFilter, mipFilter),
                MaxAnisotropy = 16,
                MipMapLevelOfDetailBias = mipMapLevelOfDetailBias,
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

            graphics.PreferMultiSampling = multisampling;
            graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = multisampling ? 4 : 0;
            graphics.GraphicsDevice.RasterizerState = new RasterizerState
            {
                MultiSampleAntiAlias = multisampling,
                CullMode = CullMode.CullCounterClockwiseFace
            };
            for (int i = 0; i < 15; i++)
                graphics.GraphicsDevice.SamplerStates[i] = ss;
            graphics.ApplyChanges();
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
            manBillboardSystem.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.Right);
            GraphicsDevice.RasterizerState = previousRasterizerState;

            foreach (var cModel in loadedModels)
            {
                cModel.SetClipPlane(-clipPlane);
                cModel.Material = mat;
                cModel.Draw(camera);
            }
            smoke.SetClipPlane(-clipPlane);
            smoke.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.Up, camera.Right);
            manBillboardSystem.SetClipPlane(-clipPlane);
            manBillboardSystem.Draw(camera.ViewMatrix, camera.ProjectionMatrix, camera.Right);

            foreach (var cModel in loadedModels)
                cModel.SetClipPlane(clipPlane);
            manBillboardSystem.SetClipPlane(clipPlane);
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


