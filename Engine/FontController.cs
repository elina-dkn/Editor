using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Engine
{
    internal class FontController
    {
        private SpriteFont _fontArial16 = null;
        private SpriteFont _fontArial18 = null;
        private SpriteFont _fontArial20 = null; 

        public FontController() { }

        public void LoadContent(ContentManager content)
        {
            _fontArial16 = content.Load<SpriteFont>("Arial16");
            _fontArial18 = content.Load<SpriteFont>("Arial18");
            _fontArial20 = content.Load<SpriteFont>("Arial20");
        }

        public void Draw(SpriteBatch spriteBatch, int size, string text, Vector2 position, Color color)
        {
            switch(size)
            {
                case 16:
                    spriteBatch.DrawString(_fontArial16, text, position, color);
                    break;
                case 18:
                    spriteBatch.DrawString(_fontArial18, text, position, color);
                    break;
                case 20:
                    spriteBatch.DrawString(_fontArial20, text, position, color);
                    break;
            }
        }
    }
}
