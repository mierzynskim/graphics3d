using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1
{
    public class MeshTag

    {
        public Vector3 Color { get; }
        public Texture2D Texture1 { get; }
        public float SpecularPower { get; }
        public Effect CachedEffect = null;

        public MeshTag(Vector3 color, Texture2D texture, float specularPower)
        {
            this.Color = color;
            this.Texture1 = texture;
            this.SpecularPower = specularPower;
        }
    }
}