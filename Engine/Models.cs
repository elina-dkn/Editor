using Editor.Engine.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System;

namespace Editor.Engine
{
    internal class Models : ISerializable, ISelectable, IRenderable, ISoundEmitter
    {
        // Members
        private Vector3 _position;
        private Vector3 _rotation;

        // Accessors
        public Model Mesh { get; set; }
        public Material Material { get; private set; }
        public SoundEffectInstance[] SoundEffects { get; private set; }
        public Vector3 Position { get => _position; set { _position = value; } }
        public Vector3 Rotation { get => _rotation; set { _rotation = value; } }
        public float Scale { get; set; }
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    SelectedDirty = true;
                }
            }
        }

        public string Name { get; set; }
        private bool _selected;
        public static bool SelectedDirty { get; set; } = false;

        public Models() { }

        public Models(
            GameEditor game, string model, string texture,
            string effect, Vector3 position, float scale)
        {
            Create(game, model, texture, effect, position, scale);
        }

        public void Create(
            GameEditor game, string model, string texture,
            string effect, Vector3 position, float scale)
        {
            Mesh = game.Content.Load<Model>(model);
            Mesh.Tag = model;
            Name = model;
            Material = new Material();
            SetTexture(game, texture);
            SetShader(game, effect);
            Position = position;
            Scale = scale;
            SoundEffects = new SoundEffectInstance[Enum.GetNames(typeof(SoundEffectTypes)).Length];
        }

        public void SetTexture(GameEditor game, string texture)
        {
            if(texture == "DefaultTexture")
            {
                Material.Diffuse = game.DefaultTexture;
            } else
            {
                Material.Diffuse = game.Content.Load<Texture>(texture);
            }
            Material.Diffuse.Tag = texture;
        }

        public void SetShader(GameEditor game, string shader)
        {
            if(shader == "DefaultEffect")
            {
                Material.Effect = game.DefaultEffect;
            } else
            {
                Material.Effect = game.Content.Load<Effect>(shader);
            }
            Material.Effect.Tag = shader;
            SetShader(Material.Effect);
        }


        public void SetShader(Effect effect)
        {
            Material.Effect = effect;
            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }
        }

        public void Translate(Vector3 translate, Camera camera)
        {
            float distance = Vector3.Distance(camera.Target, camera.Position);
            Vector3 forward = camera.Target - camera.Position;
            forward.Normalize();
            Vector3 left = Vector3.Cross(forward, Vector3.Up);
            left.Normalize();
            Vector3 up = Vector3.Cross(left, forward);
            up.Normalize();
            Position += left * translate.X * distance;
            Position += up * translate.Y * distance;
            Position += forward * translate.Z * 100f;
        }

        public void Rotate(Vector3 rotate)
        {
            Rotation += rotate;
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateScale(Scale) *
                   Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z) *
                   Matrix.CreateTranslation(Position);
        }

        public void Render()
        {
            /*
            Material.Effect.Parameters["World"]?.SetValue(GetTransform());
            Material.Effect.Parameters["WorldViewProjection"]?.SetValue(GetTransform() * camera.View * camera.Projection);
            Material.Effect.Parameters["Texture"]?.SetValue(Material.Diffuse);
            Material.Effect.Parameters["Tint"]?.SetValue(Selected);
            Material.Effect.Parameters["View"]?.SetValue(camera.View);
            Material.Effect.Parameters["Projection"]?.SetValue(camera.Projection);
            Material.Effect.Parameters["TextureTiling"]?.SetValue(15.0f);
            Material.Effect.Parameters["LigvhtDirection"]?.SetValue(Vector3.One);
            */

            foreach (ModelMesh mesh in Mesh.Meshes)
            {
                mesh.Draw();
            }
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(Mesh.Tag.ToString());
            stream.Write(Material.Diffuse.Tag.ToString());
            stream.Write(Material.Effect.Tag.ToString());
            HelpSerialize.Vec3(stream, Position);
            HelpSerialize.Vec3(stream, Rotation);
            stream.Write(Scale);
        }

        public void Deserialize(BinaryReader stream, GameEditor game)
        {
            string mesh = stream.ReadString();
            string texture = stream.ReadString();
            string shader = stream.ReadString();
            Position = HelpDeserialize.Vec3(stream);
            Rotation = HelpDeserialize.Vec3(stream);
            Scale = stream.ReadSingle();
            Material = new Material();
            Create(game, mesh, texture, shader, Position, Scale);
        }
    }
}
