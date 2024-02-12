using Editor.Engine.Interfaces;
using Editor.Engine.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Editor.Engine
{
    internal class Level : ISerializable
    {
        // Property
        public Camera GetCamera() { return _camera; }
        // Members
        private List<Models> _models = new();
        private Camera _camera = new(new Vector3(0, 400, 500), 16 / 9);
        private Light _light = new Light() { Position = new(0, 400, -500), Color = new(0.9f, 0.9f, 0.9f) };
        private Effect terrainEffect = null;
        private Terrain terrain = null;


        public Level() { }

        public void LoadContent(GameEditor game)
        {
            //terrain = new(game.DefaultEffect, game.DefaultHeightMap, game.DefaultGrass, 200, game.GraphicsDevice);
        }

        public void AddModel(Models model)
        {
            _models.Add(model);
        }

        public void ClearSelectedModels()
        {
            foreach(var model in _models)
            {
                model.Selected = false;
            }
            if(terrain != null)
            {
                terrain.Selected = false;
            }
        }

        public List<ISelectable> GetSelectedModels()
        {
            List<ISelectable> models = new List<ISelectable>();
            foreach(var model in _models)
            {
                if(model.Selected) models.Add(model);
            }
            if(terrain != null && terrain.Selected) models.Add(terrain);

            return models;
        }

        private void HandleAudio()
        {
            foreach(Models m in _models)
            {
                if((Models.SelectedDirty) && m.Selected)
                {
                    var sfi = m.SoundEffects[(int)SoundEffectTypes.OnSelect];
                    if(sfi?.State == SoundState.Stopped)
                    {
                        sfi.Play();
                    }
                }
            }
        }

        public void HandleTranslate()
        {
            InputController ic = InputController.Instance;
            Vector3 translate = Vector3.Zero;
            if (ic.IsKeyDown(Keys.Left)) translate.X += -10;
            if (ic.IsKeyDown(Keys.Right)) translate.X += +10;

            if (ic.IsKeyDown(Keys.Menu))
            {
                if (ic.IsKeyDown(Keys.Up)) translate.Z += 1;
                if (ic.IsKeyDown(Keys.Down)) translate.Z += -1;
            }
            else
            {
                if (ic.IsKeyDown(Keys.Up)) translate.Y += 10;
                if (ic.IsKeyDown(Keys.Down)) translate.Y += -10;
            }

            if (ic.IsButtonDown(MouseButtons.Middle))
            {
                Vector2 dir = ic.MousePosition - ic.LastPosition;
                translate.X = dir.X;
                translate.Y = -dir.Y;
            }

            if (ic.GetWheel() != 0)
            {
                translate.Z = ic.GetWheel() * 2;
            }

            if (translate != Vector3.Zero)
            {
                bool modelTranslated = false;
                foreach(Models model in _models)
                {
                    if(model.Selected)
                    {
                        modelTranslated = true;
                        model.Translate(translate / 1000, _camera);
                    }
                }
                if (!modelTranslated)
                {                    
                    _camera.Translate(translate * 0.001f);
                }
            }
        }

        public void HandleRotate(float delta) {
            InputController ic = InputController.Instance;
            if (ic.IsButtonDown(MouseButtons.Right) && (!ic.IsKeyDown(Keys.Menu)))
            {              
                Vector2 dir = ic.MousePosition - ic.LastPosition;
                if (dir != Vector2.Zero)
                {
                    Vector3 movement = new Vector3(dir.Y, dir.X, 0) * delta;
                    bool modelRoated = false;
                    foreach (Models model in _models) 
                    {
                        if(model.Selected)
                        {
                            modelRoated = true;
                            model.Rotation += movement;
                        }
                    }
                    if (!modelRoated)
                    {
                        _camera.Rotate(movement);
                    }
                }
            }
        }

        private void HandleScale(float delta)
        {
            InputController ic = InputController.Instance;
            if(ic.IsButtonDown(MouseButtons.Right) && ic.IsKeyDown(Keys.Menu)) 
            {
                float l = ic.MousePosition.X - ic.LastPosition.X;
                if(l != 0) 
                {
                    l += delta;
                    foreach(Models model in _models)
                    {
                        if(model.Selected)
                        {
                            model.Scale += l;
                        }
                    }
                }
            }
        }

        internal ISelectable HandlePick(bool selected = true)
        {
            float? f;
            Matrix transform = Matrix.Identity;
            InputController ic = InputController.Instance;
            if(ic.IsButtonDown(MouseButtons.Left) || !selected)
            {
                Ray r = HelpMath.GetPickRay(ic.MousePosition, _camera);
                // Check for Models
                foreach(Models model in _models)
                {
                    if(selected) model.Selected = false;
                    transform = model.GetTransform();
                    foreach (ModelMesh mesh in model.Mesh.Meshes)
                    {
                        BoundingSphere s = mesh.BoundingSphere;
                        s.Transform(ref transform, out s);
                        f = r.Intersects(s);
                        if(f.HasValue)
                        {
                            f = HelpMath.PickTriangle(in mesh, ref r, ref transform);
                            if(f.HasValue )
                            {
                                if (!selected) return model;
                                model.Selected = true;
                            }
                        }
                    }
                }
                // Check for Terrain
                if(terrain != null)
                {
                    transform = Matrix.Identity;
                    f = HelpMath.PickTriangle(in terrain, ref r, ref transform);
                    terrain.Selected = false;
                    if (f.HasValue)
                    {
                        if (!selected) return terrain;
                        terrain.Selected = true;
                    }
                }
            }
            return null;
        }

        public void Update(float delta)
        {
            HandleTranslate();
            HandleRotate(delta);
            HandleScale(delta);
            HandlePick();
            HandleAudio();
        }

        public void Render()
        {
            Renderer r = Renderer.Instance;
            r.Camera = _camera;
            r.Light = _light;   
            foreach (Models model in _models)
            {
                r.Render(model);
            }
            if(terrain != null)
            {
                r.Render(terrain);
            }
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(_models.Count);
            foreach(Models model in _models)
            {
                model.Serialize(stream);
            }
            _camera.Serialize(stream);
        }

        public void Deserialize(BinaryReader stream, GameEditor game)
        {
            int modelCount = stream.ReadInt32();
            for(int count = 0; count < modelCount; count++)
            {
                Models model = new Models();
                model.Deserialize(stream, game);
                _models.Add(model);
            }
            _camera.Deserialize(stream, game);
        }

        public override string ToString()
        {
            string s = string.Empty;
            foreach (Models model in _models)
            {
                if(model.Selected)
                {
                    s += "\nModel: Pos: " + model.Position.ToString() + " Rot: " + model.Rotation.ToString(); 
                }
            }
            return _camera.ToString() + s;
        }
    }
}
