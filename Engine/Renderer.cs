using Editor.Engine.Interfaces;
using Editor.Engine.Lights;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Editor.Engine
{
    internal class Renderer
    {
        private static readonly Lazy<Renderer> lazy = new(()  => new Renderer());
        public static Renderer Instance { get { return lazy.Value; } }

        internal Camera Camera { get; set; }
        internal Light Light { get; set; }

        private Renderer() { }

        public void Render(IRenderable obj)
        {
            SetShaderParams(obj);
            obj.Render();
        }

        public void SetShaderParams(IRenderable obj)
        {
            Material m = obj.Material;
            Effect e = m.Effect;
            e.Parameters["World"]?.SetValue(obj.GetTransform());
            e.Parameters["WorldViewProjection"]?.SetValue(obj.GetTransform() * Camera.View * Camera.Projection);
            e.Parameters["Texture"]?.SetValue(m.Diffuse);
            if(obj is ISelectable)
            {
                ISelectable s = obj as ISelectable;
                e.Parameters["Tint"]?.SetValue(s.Selected);
            } else
            {
                e.Parameters["Tint"]?.SetValue(false);
            }
            e.Parameters["CameraPosition"]?.SetValue(Camera.Position);
            e.Parameters["View"]?.SetValue(Camera.View);
            e.Parameters["Projection"]?.SetValue(Camera.Projection);
            e.Parameters["TextureTiling"]?.SetValue(15.0f);
            e.Parameters["LightDirection"]?.SetValue(obj.Position - Light.Position);
            e.Parameters["LightColor"]?.SetValue(Light.Color);
        }
    }
}
