using GK1.Camera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1.Models
{
    public abstract class CModel : IRenderable
    {
        public Vector3 Position { get; set; }
        public Matrix Rotation { get; set; }
        public Matrix Scale { get; set; }

        public Model Model { get; protected set; }
        public Material Material { get; set; }
        public abstract void Draw(CameraAbstract camera);

        public abstract void SetClipPlane(Vector4? plane);

        public abstract void SetModelEffect(Effect effect, bool copyEffect);

        protected void SetEffectParameter(Effect effect, string paramName, object val)
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