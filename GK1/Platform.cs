using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GK1
{
    public class Platform
    {
        public VertexPositionNormalTexture[] MakeCube()
        {
            var vertices = new VertexPositionNormalTexture[36];

            var face = new Vector3[6];
            //TopLeft
            face[0] = new Vector3(-1f, 1f, 0.0f);
            //BottomLeft
            face[1] = new Vector3(-1f, -1f, 0.0f);
            //TopRight
            face[2] = new Vector3(1f, 1f, 0.0f);
            //BottomLeft
            face[3] = new Vector3(-1f, -1f, 0.0f);
            //BottomRight
            face[4] = new Vector3(1f, -1f, 0.0f);
            //TopRight
            face[5] = new Vector3(1f, 1f, 0.0f);

            var disc = new Dictionary<Vector3, Vector2>
            {
                {face[0],  new Vector2(0f, 0f)},
                {face[1],  new Vector2(0f, 1f)},
                {face[4],  new Vector2(1f, 1f)},
                {face[5],  new Vector2(1f, 0f)},
            };
            //front face
            for (int i = 0; i <= 2; i++)
            {
                vertices[i] = new VertexPositionNormalTexture(face[i] + Vector3.UnitZ, Vector3.UnitZ, disc[face[i]]);
                vertices[i + 3] = new VertexPositionNormalTexture(face[i + 3] + Vector3.UnitZ, Vector3.UnitZ, disc[face[i + 3]]);
            }

            //back face

            for (int i = 0, j = 0; i <= 2; i++)
            {
                vertices[i + 6] = new VertexPositionNormalTexture(face[2 - i] - Vector3.UnitZ, Vector3.UnitZ, disc[face[2 - i]]);
                vertices[i + 6 + 3] = new VertexPositionNormalTexture(face[5 - i] - Vector3.UnitZ, Vector3.UnitZ, disc[face[5 - i]]);
            }

            //left face
            Matrix RotY90 = Matrix.CreateRotationY(-(float)Math.PI / 2f);
            for (int i = 0; i <= 2; i++)
            {
                vertices[i + 12] = new VertexPositionNormalTexture(Vector3.Transform(face[i], RotY90) - Vector3.UnitX, Vector3.UnitZ, disc[face[i]]);
                vertices[i + 12 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[i + 3], RotY90) - Vector3.UnitX, Vector3.UnitZ, disc[face[i + 3]]);
            }

            //Right face

            for (int i = 0; i <= 2; i++)
            {
                vertices[i + 18] = new VertexPositionNormalTexture(Vector3.Transform(face[2 - i], RotY90) + Vector3.UnitX, Vector3.UnitZ, disc[face[2 - i]]);
                vertices[i + 18 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[5 - i], RotY90) + Vector3.UnitX, Vector3.UnitZ, disc[face[5 - i]]);

            }

            //Top face

            Matrix RotX90 = Matrix.CreateRotationX(-(float)Math.PI / 2f);
            for (int i = 0; i <= 2; i++)
            {
                vertices[i + 24] = new VertexPositionNormalTexture(Vector3.Transform(face[i], RotX90) + Vector3.UnitY, Vector3.UnitZ, disc[face[i]]);
                vertices[i + 24 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[i + 3], RotX90) + Vector3.UnitY, Vector3.UnitZ, disc[face[i + 3]]);

            }

            //Bottom face

            for (int i = 0, j = 0; i <= 2; i++)
            {
                vertices[i + 30] = new VertexPositionNormalTexture(Vector3.Transform(face[2 - i], RotX90) - Vector3.UnitY, Vector3.UnitZ, disc[face[2 - i]]);
                vertices[i + 30 + 3] = new VertexPositionNormalTexture(Vector3.Transform(face[5 - i], RotX90) - Vector3.UnitY, Vector3.UnitZ, disc[face[5 - i]]);
            }

            return vertices;
        }

        public VertexPositionNormalTexture[] CreatePlatformFinal()
        {
            var normalize = new Vector3(0, 0, 1);
            var texture = Vector2.Zero;

            const int length = 200;
            const int wight = 10;
            const int height = 80;

            var platformVertex = new VertexPositionNormalTexture[18];
            platformVertex[0] = new VertexPositionNormalTexture(new Vector3(-wight, -height, -length), normalize, texture);
            platformVertex[1] = new VertexPositionNormalTexture(new Vector3(-wight, -height + 10, -length), normalize, texture);
            platformVertex[2] = new VertexPositionNormalTexture(new Vector3(-wight, -height, length), normalize, texture);
            platformVertex[3] = platformVertex[1];
            platformVertex[4] = new VertexPositionNormalTexture(new Vector3(-wight, -height + 10, length), normalize, texture);
            platformVertex[5] = platformVertex[2];
            platformVertex[6] = platformVertex[1];
            platformVertex[7] = new VertexPositionNormalTexture(new Vector3(wight, -height + 10, -length), normalize, texture);
            platformVertex[8] = platformVertex[4];
            platformVertex[9] = platformVertex[7];
            platformVertex[10] = new VertexPositionNormalTexture(new Vector3(wight, -height + 10, length), normalize, texture);
            platformVertex[11] = platformVertex[4];

            platformVertex[12] = platformVertex[7];
            platformVertex[13] = new VertexPositionNormalTexture(new Vector3(wight, -height, -length), normalize, texture);
            platformVertex[14] = platformVertex[10];
            platformVertex[15] = platformVertex[13];
            platformVertex[16] = new VertexPositionNormalTexture(new Vector3(wight, -height, length), normalize, texture);
            platformVertex[17] = platformVertex[10];

            return platformVertex;
        }
    }
}
