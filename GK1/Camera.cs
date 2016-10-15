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
        // We need this to calculate the aspectRatio
        // in the ProjectionMatrix property.
        GraphicsDevice graphicsDevice;

        Vector3 position = new Vector3(0, 50, 10);

        private float angleZ;
        private float angleX;

        public Matrix ViewMatrix
        {
            get
            {
                var lookAtVector = new Vector3(0, -1, -.5f);
                // We'll create a rotation matrix using our angle
                var rotationMatrix = Matrix.CreateRotationZ(angleZ);
                // Then we'll modify the vector using this matrix:
                lookAtVector = Vector3.Transform(lookAtVector, rotationMatrix);
                lookAtVector += position;

                var upVector = Vector3.UnitZ;

                return Matrix.CreateLookAt(
                    position, lookAtVector, upVector);
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
        }

        public void Update(GameTime gameTime)
        {
            HandleRotation(gameTime);
            HandleWsadMoves(gameTime);
        }

        private void HandleRotation(GameTime gameTime)
        {
            var touchCollection = Mouse.GetState();
            var isMousePressed = touchCollection.LeftButton == ButtonState.Pressed;
            if (isMousePressed)
            {
                var xPosition = touchCollection.Position.X;
                var yPosition = touchCollection.Position.Y;

                var xRatio = xPosition/(float) graphicsDevice.Viewport.Width;
                var yRatio = yPosition/(float) graphicsDevice.Viewport.Height;

                if (xRatio < 1/3.0f)
                    angleZ += (float) gameTime.ElapsedGameTime.TotalSeconds;
                else
                    angleZ -= (float) gameTime.ElapsedGameTime.TotalSeconds;
            }
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

            const float unitsPerSecond = 3;

            position += directionVector * unitsPerSecond *
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
