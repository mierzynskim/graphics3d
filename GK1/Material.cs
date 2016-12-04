using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1
{
    public class Material
    {
        public virtual void SetEffectParameters(Effect effect)
        {
        }
    }

    public enum LightType
    {
        Reflector = 0,
        Point = 1
    }
    public class LightingMaterial : Material
    {

        public Vector3 AmbientColor { get; set; }
        public Vector3[] LightDirection { get; set; }
        public Vector3[] LightPosition { get; set; }
        public Vector3[] LightColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float[] LightAttenuation { get; set; }
        public float[] LightFalloff { get; set; }
        public LightType[] LightTypes { get; set; }

        public LightingMaterial()
        {
            AmbientColor = new Vector3(.1f, .1f, .1f);
            LightDirection = new Vector3[4];
            LightPosition = new Vector3[] {Vector3.Zero, Vector3.Zero, Vector3.Zero, Vector3.Zero };
            LightColor = new Vector3[] {Vector3.One, Vector3.One, Vector3.One, Vector3.One, };
            SpecularColor = new Vector3(1, 1, 1);
            LightAttenuation = new float[] {100, 100, 7, 7 };
            LightFalloff = new float[] { 2, 2, 10, 10};
        }

        public override void SetEffectParameters(Effect effect)
        {
            effect.Parameters["AmbientColor"]?.SetValue(AmbientColor);

            effect.Parameters["LightDirection"]?.SetValue(LightDirection);
            effect.Parameters["LightPosition"]?.SetValue(LightPosition);

            effect.Parameters["LightColor"]?.SetValue(LightColor);

            effect.Parameters["SpecularColor"]?.SetValue(SpecularColor);

            effect.Parameters["LightAttenuation"]?.SetValue(
                LightAttenuation);

            effect.Parameters["LightFalloff"]?.SetValue(LightFalloff);
            effect.Parameters["LightTypes"]?.SetValue(LightTypes.Select(x => (float) x).ToArray());
        }
    }
}
