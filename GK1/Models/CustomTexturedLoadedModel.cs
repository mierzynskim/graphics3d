using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1.Models
{
    public class CustomTexturedLoadedModel : LoadedModel
    {
        private readonly Dictionary<ModelMesh, Texture> textures;

        public CustomTexturedLoadedModel(Model model, Vector3 position, Matrix rotation, Matrix scale, GraphicsDevice graphicsDevice, Dictionary<ModelMesh, Texture> textures) 
            : base(model, position, rotation, scale, graphicsDevice)
        {
            this.textures = textures;
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
                    if (textures != null && textures.ContainsKey(mesh))
                    {
                        SetEffectParameter(toSet, "BasicTexture", textures[mesh]);
                        SetEffectParameter(toSet, "TextureEnabled", true);
                    }
                    else if (tag.Texture1 != null)
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
    }
}