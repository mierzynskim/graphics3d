using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1
{
    public class CModel
    {
        public Vector3 Position { get; set; }
        public Matrix Rotation { get; set; }
        public Matrix Scale { get; set; }

        public Model Model { get; private set; }
        public Material Material { get; set; }

        private readonly Matrix[] modelTransforms;

        private GraphicsDevice graphicsDevice;
        public CModel(Model model, Vector3 position, Matrix rotation, Matrix scale, GraphicsDevice graphicsDevice)
        {
            Model = model;

            modelTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Position = position;
            Rotation = rotation;
            Scale = scale;

            this.graphicsDevice = graphicsDevice;
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

        public void Draw(Camera camera)
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

        public void SetModelEffect(Effect effect, bool copyEffect)
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

        private void SetEffectParameter(Effect effect, string paramName, object val)
        {
            if (effect.Parameters[paramName] == null)
                return;

            if (val is Vector3)
                effect.Parameters[paramName].SetValue((Vector3)val);
            else if (val is bool)
                effect.Parameters[paramName].SetValue((bool)val);
            else if (val is Matrix)
                effect.Parameters[paramName].SetValue((Matrix)val);
            else if (val is Texture2D)
                effect.Parameters[paramName].SetValue((Texture2D)val);
        }
    }
}