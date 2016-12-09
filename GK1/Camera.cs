using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace GK1
{
    public class Camera
    {
        private readonly GraphicsDevice graphicsDevice;
        private float angleZ;
        private float angleX;

        public Vector3 Position { get; private set; } = new Vector3(0, 0, 10);

        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }

        public Matrix ViewMatrix
        {
            get
            {
                var lookAtVector = new Vector3(0, -1, -.5f);
                var rotationMatrix = Matrix.Multiply(Matrix.CreateRotationZ(angleZ), Matrix.CreateRotationX(angleX));
                lookAtVector = Vector3.Transform(lookAtVector, rotationMatrix);
                lookAtVector += Position;

                var upVector = Vector3.UnitZ;

                var viewMatrix = Matrix.CreateLookAt(Position, lookAtVector, upVector);
                Up = viewMatrix.Up;
                Right = viewMatrix.Left;
                return viewMatrix;
            }
        }

        public Matrix WorldMatrix => Matrix.Identity;

        public Matrix ProjectionMatrix
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

        public Camera(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            Mouse.SetPosition(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
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
