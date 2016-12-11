using GK1.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1.Models
{
    public class LoadedModel : CModel
    {
        private readonly Matrix[] modelTransforms;

        private GraphicsDevice graphicsDevice;
        public LoadedModel(Model model, Vector3 position, Matrix rotation, Matrix scale, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            Model = model;
            modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            Position = position;
            Rotation = rotation;
            Scale = scale;
            GenerateTags();
        }

        private void GenerateTags()
        {
            foreach (var mesh in Model.Meshes)
                foreach (var part in mesh.MeshParts)
                    if (part.Effect is BasicEffect)
                    {
                        var effect = (BasicEffect)part.Effect;
                        var tag = new MeshTag(effect.DiffuseColor, effect.Texture, effect.SpecularPower);
                        part.Tag = tag;
                    }
        }

        public override void Draw(CameraAbstract camera)
        {
            var baseWorld = Scale * Rotation * Matrix.CreateTranslation(Position);
            foreach (var mesh in Model.Meshes)
            {
                var localWorld = modelTransforms[mesh.ParentBone.Index]
                     * baseWorld;

                foreach (var part in mesh.MeshParts)
                {
                    var effect = part.Effect;
                    effect.Parameters["World"].SetValue(localWorld);
                    effect.Parameters["View"].SetValue(camera.ViewMatrix);
                    effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
                    Material.SetEffectParameters(effect);
                }
                mesh.Draw();
            }
        }


        public override void SetModelEffect(Effect effect, bool copyEffect)
        {
            foreach (var mesh in Model.Meshes)
                foreach (var part in mesh.MeshParts)
                {
                    var toSet = effect;
                    if (copyEffect)
                        toSet = effect.Clone();

                    var tag = ((MeshTag)part.Tag);
                    if (tag.Texture1 != null)
                    {
                        SetEffectParameter(toSet, "BasicTexture", tag.Texture1);
                        SetEffectParameter(toSet, "TextureEnabled", true);
                    }
                    else
                        SetEffectParameter(toSet, "TextureEnabled", false);
                    SetEffectParameter(toSet, "DiffuseColor", tag.Color);
                    SetEffectParameter(toSet, "SpecularPower", tag.SpecularPower);

                    part.Effect = toSet;
                }
        }

        public override void SetClipPlane(Vector4? plane)
        {
            foreach (var mesh in Model.Meshes)
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect.Parameters["ClipPlaneEnabled"]?.SetValue(plane.HasValue);
                    if (plane.HasValue)
                        part.Effect.Parameters["ClipPlane"]?.SetValue(plane.Value);
                }
        }


    }
}
