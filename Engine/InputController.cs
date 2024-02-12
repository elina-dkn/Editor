using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Editor.Engine
{
    internal class InputController
    {
        private static readonly Lazy<InputController> _instance = new(() => new InputController());
        public static InputController Instance { get { return _instance.Value; } }

        public Vector2 MousePosition { get; set; } = Vector2.Zero;
        public Vector2 LastPosition { get; private set; } = Vector2.Zero;
        public Vector2 DragStart { get; set; } = Vector2.Zero;
        public Vector2 DragEnd { get; set; } = Vector2.Zero;

        public Dictionary<Keys, bool> _keyState = new();
        private Dictionary<MouseButtons, bool> _buttonState = new();
        private int _mouseWheel = 0;

        private InputController() 
        { 
            foreach(Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (_keyState.ContainsKey(key)) continue;
                _keyState.Add(key, false);
            }

            foreach(MouseButtons button in Enum.GetValues(typeof(MouseButtons)))
            {
                if(_buttonState.ContainsKey(button)) continue;
                _buttonState.Add(button, false);
            }
        }

        public void SetKeyDown(Keys key)
        {
            _keyState[key] = true;  
        }

        public void SetKeyUp(Keys key)
        {
            _keyState[key] = false;
        }

        public bool IsKeyDown(Keys key) 
        {
            return _keyState[key];
        }

        public void SetButtonDown(MouseButtons button)
        {
            _buttonState[button] = true;
        }

        public void SetButtonUp(MouseButtons button)
        {
            _buttonState[button] = false;
        }

        public bool IsButtonDown(MouseButtons button)
        {
            return _buttonState[button];
        }

        public void SetWheel(int delta)
        {
            _mouseWheel += delta;
        }

        public int GetWheel()
        {
            return _mouseWheel;
        }

        public void Clear()
        {
            _mouseWheel = 0;
            LastPosition = MousePosition;
        }

        /*
        public Ray GetPickray(Camera camera)
        {
            Vector3 nearPoint = new Vector3(MousePosition, 0);
            Vector3 farPoint = new Vector3(MousePosition, 1);

            nearPoint = camera.Viewport.Unproject(nearPoint, camera.Projection, camera.View, Matrix.Identity);
            farPoint = camera.Viewport.Unproject(farPoint, camera.Projection, camera.View, Matrix.Identity);
            
            Vector3 direction = farPoint - nearPoint; ;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }
        */

        public override string ToString()
        {
            string s = "Keys Down: ";
            foreach (KeyValuePair<Keys, bool> key in _keyState)
            {
                if(key.Value)
                {
                    s += key.Key + " ";
                }
            }

            s += "\nButtons Down: ";
            foreach (var button in _buttonState)
            {
                if (button.Value)
                {
                    s += button.Key + " ";
                }
            }

            return s;
        }
    }
}
