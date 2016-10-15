using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace GK1
{
    public class Bench
    {
        private float scale = 0.2f;
        public virtual Matrix GetWorld(Matrix worldMatrix, Matrix projectionMatrix)
        {
            return Matrix.CreateScale(scale) * Matrix.CreateRotationX(90) * worldMatrix;
        }
    }
}
