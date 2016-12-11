using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1.Camera
{
    public abstract class CameraAbstract
    {
        private readonly GraphicsDevice graphicsDevice;
        protected float AngleZ;
        protected float AngleX;

        public Vector3 Position { get; set; } = new Vector3(0, 0, 10);

        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Target { get; set; }



        public CameraAbstract(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;

        }
        public virtual Matrix ViewMatrix
        {
            get
            {
                var lookAtVector = new Vector3(0, -1, -.5f);
                var rotationMatrix = Matrix.Multiply(Matrix.CreateRotationZ(AngleZ), Matrix.CreateRotationX(AngleX));
                lookAtVector = Vector3.Transform(lookAtVector, rotationMatrix);
                lookAtVector += Position;

                var upVector = Vector3.UnitZ;

                var viewMatrix = Matrix.CreateLookAt(Position, lookAtVector, upVector);
                Up = Vector3.Transform(Vector3.UnitZ, rotationMatrix);
                Right = Vector3.Cross(Vector3.Transform(-Vector3.UnitY, Matrix.CreateRotationZ(AngleZ)), Up);

                Target = Vector3.Transform(Vector3.Forward, rotationMatrix);
                return viewMatrix;
            }
        }

        public  Matrix WorldMatrix => Matrix.Identity;

        public  Matrix ProjectionMatrix
        {
            get
            {
                float fieldOfView = MathHelper.PiOver4;
                float nearClipPlane = 1;
                float farClipPlane = 200;
                float aspectRatio = graphicsDevice.Viewport.Width / (float)graphicsDevice.Viewport.Height;

                return Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }

        public abstract void Update(GameTime gameTime);
    }
}