using Microsoft.Xna.Framework.Content;
using System.IO;

namespace Editor.Engine.Interfaces
{
    internal interface ISerializable
    {
        public void Serialize(BinaryWriter stream);
        public void Deserialize(BinaryReader stream, GameEditor game);
    }
}
