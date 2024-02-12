using Editor.Engine;
using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Editor.Engine
{
    internal class Camera : ISerializable
    {
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);
        public Vector3 Target { get; set; } = new Vector3(300, 0, 0);
        public Matrix View { get; set; } = Matrix.Identity;
        public Matrix Projection { get; set; } = Matrix.Identity;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 1000f;
        public float AspectRatio { get; set; } = 16 / 9;
        public Viewport Viewport { get; set; }

        public Camera () { }

        public Camera(Vector3 position, float aspectRation)
        {
            Update(position, aspectRation);
        }

        public void Update(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            View = Matrix.CreateLookAt(Position, Target, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.ToRadians(45), AspectRatio, NearPlane, FarPlane);
        }

        public void Translate(Vector3 translate)
        {
            float distance = Vector3.Distance(Target, Position);
            Vector3 forward = Target - Position;
            forward.Normalize();
            Vector3 left = Vector3.Cross(forward, Vector3.Up);
            left.Normalize();
            Vector3 up = Vector3.Cross(left, forward);
            up.Normalize();
            Position += left * translate.X * distance;
            Position += up * translate.Y * distance;
            Position += forward * translate.Z * 100f;
            Target += left * translate.X * distance;
            Target += up * translate.Y * distance;

            Update(Position, AspectRatio);
        }

        public void Rotate(Vector3 rotate)
        {
            Position = Vector3.Transform(Position - Target,
                                        Matrix.CreateRotationY(rotate.Y));
            Position += Target;
            Update(Position, AspectRatio);
        }

        public override string ToString()
        {
            string s = "Camera Position: " + Position.ToString();
            return s;
        }

        public void Serialize(BinaryWriter stream)
        {
            HelpSerialize.Vec3(stream, Position);
            stream.Write(NearPlane);
            stream.Write(FarPlane);
            stream.Write(AspectRatio);
        }

        public void Deserialize(BinaryReader stream, GameEditor game)
        {
            Position = HelpDeserialize.Vec3(stream);
            NearPlane = stream.ReadSingle();
            FarPlane = stream.ReadSingle();
            AspectRatio = stream.ReadSingle();
            Update(Position, AspectRatio);
        }
    }
}
