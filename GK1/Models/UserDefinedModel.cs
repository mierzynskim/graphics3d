using System.Linq;
using GK1.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1.Models
{
    public class UserDefinedModel : CModel
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly Vector3 position;
        private readonly Matrix rotation;
        private readonly Matrix scale;
        private readonly VertexPositionNormalTexture[] platformVertex;
        private readonly Texture2D texture;
        private Effect effect;

        public UserDefinedModel(GraphicsDevice graphicsDevice, Vector3 position, Matrix rotation, Matrix scale, VertexPositionNormalTexture[] platformVertex, Texture2D texture)
        {
            this.graphicsDevice = graphicsDevice;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.platformVertex = platformVertex;
            this.texture = texture;
        }

        public Texture2D LiquidTexture { get; set; }

        public override void Draw(CameraAbstract camera)
        {
            Matrix worldMatrix;
            if (camera is TargetCamera)
                worldMatrix = scale * rotation * Matrix.CreateTranslation(position) * (camera).WorldMatrix;
            else
                worldMatrix = scale * rotation * Matrix.CreateTranslation(position) * ((Camera.Camera)camera).WorldMatrix;
            SetEffectParameters(camera, worldMatrix);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    platformVertex,
                    0,
                    platformVertex.Length / 3);
            }

        }

        private void SetEffectParameters(CameraAbstract camera, Matrix localWorld)
        {
            effect.Parameters["FogEnabled"]?.SetValue(Material.FogEnabled);
            effect.Parameters["FogStart"]?.SetValue(Material.FogStart);
            effect.Parameters["FogEnd"]?.SetValue(Material.FogEnd);
            effect.Parameters["FogIntensity"]?.SetValue(Material.FogIntensity);

            effect.Parameters["LightDirection"]?.SetValue(((LightingMaterial)Material).LightDirection);
            effect.Parameters["LightPosition"]?.SetValue(((LightingMaterial)Material).LightPosition);

            effect.Parameters["LightColor"]?.SetValue(((LightingMaterial)Material).LightColor);

            effect.Parameters["SpecularColor"]?.SetValue(((LightingMaterial)Material).SpecularColor);

            effect.Parameters["LightAttenuation"]?.SetValue(
                ((LightingMaterial)Material).LightAttenuation);

            effect.Parameters["LightFalloff"]?.SetValue(((LightingMaterial)Material).LightFalloff);
            effect.Parameters["LightTypes"]?.SetValue(((LightingMaterial)Material).LightTypes.Select(x => (float)x).ToArray());

            effect.Parameters["AmbientColor"].SetValue(Color.Black.ToVector3());
            if (LiquidTexture != null)
            {
                effect.CurrentTechnique = effect.Techniques["TexturedLiquid"];
                effect.Parameters["BackgroundColor"].SetValue(Color.Gray.ToVector4());
                effect.Parameters["ForegroundColor"].SetValue(Color.IndianRed.ToVector4());
                effect.Parameters["BasicTexture"].SetValue(LiquidTexture);
            }
            else
                effect.Parameters["BasicTexture"].SetValue(texture);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["TextureEnabled"].SetValue(true);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            effect.Parameters["World"].SetValue(localWorld);
        }

        public override void SetModelEffect(Effect effect, bool copyEffect)
        {
            this.effect = effect;
        }

        public override void SetClipPlane(Vector4? plane)
        {
            effect.Parameters["ClipPlaneEnabled"].SetValue(plane.HasValue);
            if (plane.HasValue)
                effect.Parameters["ClipPlane"].SetValue(plane.Value);
        }
    }
}