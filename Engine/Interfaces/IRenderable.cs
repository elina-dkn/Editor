using Microsoft.Xna.Framework;

namespace Editor.Engine.Interfaces
{
    internal interface IRenderable : IMaterial
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public float Scale { get; set; }

        public void Render();
        public Matrix GetTransform();
    }
}
