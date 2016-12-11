using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GK1.Camera
{
    public class TargetCamera : CameraAbstract
    {
        private MouseState originalMouseState;
        private readonly GraphicsDevice graphicsDevice;

        public TargetCamera(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

        }

        public override void Update(GameTime gameTime)
        {
        }


        public override Matrix ViewMatrix
        {
            get
            {
                var forward = -Target + Position;
                var side = Vector3.Cross(forward, Vector3.Up);
                var up = Vector3.Cross(forward, side);
                return Matrix.CreateLookAt(Position, Target, up);
            }
        }
    }
}