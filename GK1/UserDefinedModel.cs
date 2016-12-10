using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1
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

        public override void Draw(CameraAbstract camera)
        {
            var worldMatrix = scale * rotation * Matrix.CreateTranslation(position) * ((Camera)camera).WorldMatrix;
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
            //foreach (var mesh in Model.Meshes)
            //    foreach (var part in mesh.MeshParts)
            //    {
            //        part.Effect.Parameters["ClipPlaneEnabled"]?.SetValue(plane.HasValue);
            //        if (plane.HasValue)
            //            part.Effect.Parameters["ClipPlane"]?.SetValue(plane.Value);
            //    }
        }
    }
}