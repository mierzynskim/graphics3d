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
    public class Water
    {
        private readonly CModel waterMesh;
        private readonly Effect waterEffect;
        private GraphicsDevice graphics;

        private RenderTarget2D reflectionTarg;
        public List<IRenderable> Objects = new List<IRenderable>();

        public Water(ContentManager content, GraphicsDevice graphics, Vector3 position, Vector2 size)
        {
            this.graphics = graphics;
            waterMesh = new LoadedModel(content.Load<Model>("mirror"), position, Matrix.Identity, Matrix.Identity, graphics);
            waterEffect = content.Load<Effect>("WaterEffect");
            waterMesh.SetModelEffect(waterEffect, false);
            waterEffect.Parameters["viewportWidth"].SetValue((float)graphics.Viewport.Width);
            waterEffect.Parameters["viewportHeight"].SetValue((float)graphics.Viewport.Height);
            reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width,
                                                graphics.Viewport.Height, false, SurfaceFormat.Color,
                                                DepthFormat.Depth24);
        }

        public void RenderReflection(Camera camera)
        {
            // Reflect the camera's properties across the water plane
            var reflectedCameraPosition = camera.Position;
            reflectedCameraPosition.Y = -reflectedCameraPosition.Y + waterMesh.Position.Y*2;
            var reflectedCameraTarget = camera.Target;
            reflectedCameraTarget.Y = -reflectedCameraTarget.Y + waterMesh.Position.Y*2;
            // Create a temporary camera to render the reflected scene
            var reflectionCamera = new TargetCamera(reflectedCameraPosition, reflectedCameraTarget, graphics);
            reflectionCamera.Update();
            // Set the reflection camera's view matrix to the water effect
            waterEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.ViewMatrix);
            // Create the clip plane
            Vector4 clipPlane = new Vector4(0, 1, 0, -waterMesh.Position.Y);
            // Set the render target
            graphics.SetRenderTarget(reflectionTarg);
            graphics.Clear(Color.Black);
            // Draw all objects with clip plane
            foreach (IRenderable renderable in Objects)
            {
                renderable.SetClipPlane(clipPlane);
                renderable.Draw(reflectionCamera);
                renderable.SetClipPlane(null);
            }
            graphics.SetRenderTarget(null);
            // Set the reflected scene to its effect parameter in
            // the water effect
            waterEffect.Parameters["ReflectionMap"].SetValue(reflectionTarg);
        }

        public void PreDraw(Camera camera, GameTime gameTime)
        {
            RenderReflection(camera);
        }
    }
}
