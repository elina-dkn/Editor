
using Editor.Engine;

namespace Editor.GUI
{
    internal class ListItemLevel
    {
        public Models Model { get; set; }
        public override string ToString()
        {
            return Model.Name;
        }
    }
}
