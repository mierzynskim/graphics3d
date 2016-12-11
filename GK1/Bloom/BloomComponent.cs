using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1.Bloom
{
    public class BloomComponent : DrawableGameComponent
    {
        #region Fields

        SpriteBatch spriteBatch;

        Effect bloomExtractEffect;
        Effect bloomCombineEffect;
        Effect gaussianBlurEffect;

        RenderTarget2D sceneRenderTarget;
        RenderTarget2D renderTarget1;
        RenderTarget2D renderTarget2;


        // Choose what display settings the bloom should use.
        public BloomSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        BloomSettings settings = BloomSettings.PresetSettings[0];


        // Optionally displays one of the intermediate buffers used
        // by the bloom postprocess, so you can see exactly what is
        // being drawn into each rendertarget.
        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

        public IntermediateBuffer ShowBuffer
        {
            get { return showBuffer; }
            set { showBuffer = value; }
        }

        IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;


        #endregion

        #region Initialization


        public BloomComponent(Game game)
            : base(game)
        {
            if (game == null)
                throw new ArgumentNullException("game");
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            bloomExtractEffect = Game.Content.Load<Effect>("BloomExtract");
            bloomCombineEffect = Game.Content.Load<Effect>("BloomCombine");
            gaussianBlurEffect = Game.Content.Load<Effect>("GaussianBlur");

            // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for rendering the main scene, prior to applying bloom.
            sceneRenderTarget = new RenderTarget2D(GraphicsDevice, width, height, false,
                                                   format, pp.DepthStencilFormat, pp.MultiSampleCount,
                                                   RenderTargetUsage.DiscardContents);

            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

            renderTarget1 = new RenderTarget2D(GraphicsDevice, width, height, false, format, DepthFormat.None);
            renderTarget2 = new RenderTarget2D(GraphicsDevice, width, height, false, format, DepthFormat.None);
        }


        /// <summary>
        /// Unload your graphics content.
        /// </summary>
        protected override void UnloadContent()
        {
            sceneRenderTarget.Dispose();
            renderTarget1.Dispose();
            renderTarget2.Dispose();
        }


        #endregion

        #region Draw


        /// <summary>
        /// This should be called at the very start of the scene rendering. The bloom
        /// component uses it to redirect drawing into its custom rendertarget, so it
        /// can capture the scene image in preparation for applying the bloom filter.
        /// </summary>
        public void BeginDraw()
        {
            if (Visible)
            {
                GraphicsDevice.SetRenderTarget(sceneRenderTarget);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var screenWidth = Game.GraphicsDevice.PresentationParameters.BackBufferWidth;
            var screenHeight = Game.GraphicsDevice.PresentationParameters.BackBufferHeight;
            var screenRectangle = new Rectangle(0, 0, screenWidth, screenHeight);

            CalculateBloomEffect(screenRectangle);
        }

        /// <summary>
        /// This is where it all happens. Grabs a scene that has already been rendered,
        /// and uses postprocess magic to add a glowing bloom effect over the top of it.
        /// </summary>
        private void CalculateBloomEffect(Rectangle screenRectangle)
        {
            GraphicsDevice.SetRenderTarget(renderTarget1);
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(Settings.BloomThreshold);
            bloomExtractEffect.Parameters["BaseTexture"].SetValue(sceneRenderTarget);
            DrawTextureOnRectangle(bloomExtractEffect,screenRectangle, sceneRenderTarget);

            GraphicsDevice.SetRenderTarget(renderTarget2);
            gaussianBlurEffect.Parameters["BaseTexture"].SetValue(renderTarget1);
            SetBlurEffectParameters(1.0f / renderTarget1.Width, 0);
            DrawTextureOnRectangle(gaussianBlurEffect, screenRectangle, renderTarget1);


            GraphicsDevice.SetRenderTarget(renderTarget1);
            gaussianBlurEffect.Parameters["BaseTexture"].SetValue(renderTarget2);
            SetBlurEffectParameters(0, 1.0f / renderTarget1.Height);
            DrawTextureOnRectangle(gaussianBlurEffect, screenRectangle, renderTarget2);

            GraphicsDevice.SetRenderTarget(renderTarget2);
            bloomCombineEffect.Parameters["BaseTexture"].SetValue(sceneRenderTarget);
            bloomCombineEffect.Parameters["BloomTexture"].SetValue(renderTarget1);
            bloomCombineEffect.Parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
            bloomCombineEffect.Parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
            bloomCombineEffect.Parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
            bloomCombineEffect.Parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);
            
            DrawTextureOnRectangle(bloomCombineEffect, screenRectangle, renderTarget1);

            sceneRenderTarget = renderTarget2;
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, bloomCombineEffect);
            spriteBatch.Draw(renderTarget1, screenRectangle, Color.White);
            spriteBatch.End();
        }

        private void DrawTextureOnRectangle(Effect effect, Rectangle screenRectangle, Texture2D texture)
        {
            DrawTextureOnRectangle(screenRectangle, texture, effect);
        }

        private void DrawTextureOnRectangle(Rectangle screenRectangle, Texture2D texture, Effect effect)
        {
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            spriteBatch.Draw(texture, screenRectangle, Color.White);
            spriteBatch.End();
        }


        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }


        #endregion
    }
}
