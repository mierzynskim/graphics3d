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
            HandleRotation(gameTime);
            HandleWsadMoves(gameTime);
        }

        private void HandleRotation(GameTime gameTime)
        {
            var currentMouseState = Mouse.GetState();
            if (currentMouseState == originalMouseState) return;
            float xDifference = currentMouseState.X - originalMouseState.X;
            float yDifference = currentMouseState.Y - originalMouseState.Y;
            AngleZ -= 0.3f * xDifference * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;
            AngleX -= 0.3f * yDifference * (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;
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

            var rotationMatrix = Matrix.CreateRotationZ(AngleZ);
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

        public override Matrix ViewMatrix
        {
            get
            {
                var forward = Target - Position;
                var side = Vector3.Cross(forward, Vector3.UnitZ);
                var up = Vector3.Cross(forward, side);
                return Matrix.CreateLookAt(Position, Target, up);
            }
        }
    }
}