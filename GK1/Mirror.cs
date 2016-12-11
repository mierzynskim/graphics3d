using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GK1.Camera;
using GK1.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GK1
{
    public class Mirror
    {
        private readonly CModel mirrorMesh;
        private readonly Effect mirrorEffect;
        private readonly Vector3 position;
        private readonly GraphicsDevice graphics;
        private readonly RenderTarget2D reflectionTarg;
        public readonly List<IRenderable> Objects = new List<IRenderable>();
        private readonly Matrix scale;
        private readonly Matrix rotation;
        private readonly Model mirrorModel;
        private readonly Matrix[] modelTransforms;

        public Mirror(ContentManager content, GraphicsDevice graphics, Vector3 position, Vector2 size)
        {
            this.graphics = graphics;
            this.position = position;
            scale = Matrix.CreateScale(0.02f);
            rotation = Matrix.CreateRotationX(MathHelper.ToRadians(90)) * Matrix.CreateRotationZ(MathHelper.ToRadians(90));

            mirrorModel = content.Load<Model>("mirror");
            modelTransforms = new Matrix[mirrorModel.Bones.Count];
            mirrorModel.CopyAbsoluteBoneTransformsTo(modelTransforms);
            mirrorMesh = new LoadedModel(mirrorModel, position, rotation, scale, graphics);
            mirrorEffect = content.Load<Effect>("mirrorEffect");
            mirrorMesh.SetModelEffect(mirrorEffect, false);
            mirrorEffect.Parameters["viewportWidth"].SetValue((float)graphics.Viewport.Width);
            mirrorEffect.Parameters["viewportHeight"].SetValue((float)graphics.Viewport.Height);
            reflectionTarg = new RenderTarget2D(graphics, graphics.Viewport.Width,
                                                graphics.Viewport.Height, false, SurfaceFormat.Color,
                                                DepthFormat.Depth24);
        }

        public void RenderReflection(Camera.Camera camera,GameTime gameTime)
        {
            var reflectedCameraPosition = camera.Position;
            reflectedCameraPosition.X = -reflectedCameraPosition.X + mirrorMesh.Position.X * 2;
            var reflectedCameraTarget = camera.Target;
            reflectedCameraTarget.X = -reflectedCameraTarget.X + mirrorMesh.Position.X * 2;
            var reflectionCamera = new TargetCamera(graphics)
            {
                Position = reflectedCameraPosition,
                Target = reflectedCameraTarget
            };
            reflectionCamera.Update(gameTime);
            mirrorEffect.Parameters["ReflectedView"].SetValue(reflectionCamera.ViewMatrix);
            var clipPlane = new Vector4(1, 0, 0, -mirrorMesh.Position.X);
            graphics.SetRenderTarget(reflectionTarg);
            graphics.Clear(Color.Black);
            foreach (var renderable in Objects)
            {
                renderable.SetClipPlane(clipPlane);
                renderable.Draw(reflectionCamera);
                renderable.SetClipPlane(null);
            }
            graphics.SetRenderTarget(null);
            mirrorEffect.Parameters["ReflectionMap"].SetValue(reflectionTarg);
        }

        public void PreDraw(Camera.Camera camera, GameTime gameTime)
        {
            RenderReflection(camera, gameTime);
        }

        public void Draw(CameraAbstract camera)
        {
            var baseWorld = scale * rotation * Matrix.CreateTranslation(position);
            foreach (var mesh in mirrorModel.Meshes)
            {
                var localWorld = modelTransforms[mesh.ParentBone.Index]
                     * baseWorld;

                foreach (var part in mesh.MeshParts)
                {
                    var effect = part.Effect;
                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    //Material.SetEffectParameters(effect);
                }
                mesh.Draw();
            }
        }
    }
}
