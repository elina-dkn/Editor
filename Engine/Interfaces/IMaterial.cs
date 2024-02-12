
namespace Editor.Engine.Interfaces
{
    internal interface IMaterial
    {
        Material Material { get; }

        void SetTexture(GameEditor game, string texture);
        void SetShader(GameEditor game, string shader);
    }
}
