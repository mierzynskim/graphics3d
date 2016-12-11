using GK1.Camera;
using Microsoft.Xna.Framework;

namespace GK1
{
    public interface IRenderable
    {
        void Draw(CameraAbstract camera);
        void SetClipPlane(Vector4? plane);
    }
}