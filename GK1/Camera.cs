using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GK1
{

    public abstract class CameraAbstract
    {
        public virtual Matrix ViewMatrix { get; protected set; }
        public virtual Matrix ProjectionMatrix { get; protected set; }
        public virtual Matrix WorldMatrix { get; set; } = Matrix.Identity;
        protected GraphicsDevice GraphicsDevice { get; set; }
        public CameraAbstract(GraphicsDevice graphicsDevice)
        {
            this.GraphicsDevice = graphicsDevice;
            
        }

        public virtual void Update()
        {
        }
    }

    public class TargetCamera : CameraAbstract
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public TargetCamera(Vector3 position, Vector3 target, GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            Position = position;
            Target = target;
            GeneratePerspectiveProjectionMatrix(MathHelper.PiOver4);
        }

        private void GeneratePerspectiveProjectionMatrix(float fieldOfView)
        {
            var pp = GraphicsDevice.PresentationParameters;
            float aspectRatio = (float)pp.BackBufferWidth / pp.BackBufferHeight;
            this.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), aspectRatio, 0.1f, 1000000.0f);
        }
        public override void Update()
        {
            var forward = Target - Position;
            var side = Vector3.Cross(forward, Vector3.Up);
            var up = Vector3.Cross(forward, side);
            this.ViewMatrix = Matrix.CreateLookAt(Position, Target, up);
        }
    }
    public class Camera : CameraAbstract
    {
        private readonly GraphicsDevice graphicsDevice;
        private float angleZ;
        private float angleX;

        public Vector3 Position { get; private set; } = new Vector3(0, 0, 10);

        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Target { get; private set; }

        public override Matrix ViewMatrix
        {
            get
            {
                var lookAtVector = new Vector3(0, -1, -.5f);
                var rotationMatrix = Matrix.Multiply(Matrix.CreateRotationZ(angleZ), Matrix.CreateRotationX(angleX));
                lookAtVector = Vector3.Transform(lookAtVector, rotationMatrix);
                lookAtVector += Position;

                var upVector = Vector3.UnitZ;

                var viewMatrix = Matrix.CreateLookAt(Position, lookAtVector, upVector);
                Up = Vector3.Transform(Vector3.Up, rotationMatrix);
                Right = Vector3.Cross(Vector3.Transform(Vector3.Forward, rotationMatrix), Up); ;
                Target = viewMatrix.Forward;
                return viewMatrix;
            }
        }

        public override Matrix WorldMatrix => Matrix.Identity;

        public override Matrix ProjectionMatrix
        {
            get
            {
                float fieldOfView = MathHelper.PiOver4;
                float nearClipPlane = 1;
                float farClipPlane = 200;
                float aspectRatio = graphicsDevice.Viewport.Width / (float)graphicsDevice.Viewport.Height;
                //float aspectRatio = graphicsDevice.Viewport.Bounds.Width / (float)graphicsDevice.Viewport.Bounds.Height;

                return Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            }
        }

        public Camera(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            //Mouse.SetPosition(graphicsDevice.Adapter.CurrentDisplayMode.Width / 2, graphicsDevice.Adapter.CurrentDisplayMode.Height / 2);
            originalMouseState = Mouse.GetState();
        }

        public void Update(GameTime gameTime)
        {
            HandleRotation(gameTime);
            HandleWsadMoves(gameTime);
        }

        private MouseState originalMouseState;

        private void HandleRotation(GameTime gameTime)
        {
            var currentMouseState = Mouse.GetState();
            if (currentMouseState == originalMouseState) return;
            float xDifference = currentMouseState.X - originalMouseState.X;
            float yDifference = currentMouseState.Y - originalMouseState.Y;
            angleZ -= 0.3f * xDifference * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;
            angleX -= 0.3f * yDifference * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            //Mouse.SetPosition(graphicsDevice.Adapter.CurrentDisplayMode.Width / 2, graphicsDevice.Adapter.CurrentDisplayMode.Height / 2);
        }

        private void HandleWsadMoves(GameTime gameTime)
        {
            var state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.W))
                MoveForwardBackwards(gameTime, Direction.Forward);
            if (state.IsKeyDown(Keys.S))
                MoveForwardBackwards(gameTime, Direction.Backwards);
            if (state.IsKeyDown(Keys.A))
                MoveForwardBackwards(gameTime, Direction.Left);
            if (state.IsKeyDown(Keys.D))
                MoveForwardBackwards(gameTime, Direction.Right);
        }

        private void MoveForwardBackwards(GameTime gameTime, Direction direction)
        {
            var directionVector = CreateDirectionVector(direction);

            var rotationMatrix = Matrix.CreateRotationZ(angleZ);
            directionVector = Vector3.Transform(directionVector, rotationMatrix);

            const float unitsPerSecond = 7;

            Position += directionVector * unitsPerSecond *
                        (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private static Vector3 CreateDirectionVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    return new Vector3(0, -1, 0);
                case Direction.Backwards:
                    return new Vector3(0, 1, 0);
                case Direction.Left:
                    return new Vector3(1, 0, 0);
                case Direction.Right:
                    return new Vector3(-1, 0, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}
