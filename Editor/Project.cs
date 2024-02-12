using Editor.Engine;
using Editor.Engine.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace Editor.Editor
{
    internal class Project : ISerializable
    {
        public event AssetsUpdated OnAssetsUpdated;

        public Level CurrentLevel { get; set; } = null;
        public List<Level> Levels { get; set; } = new();
        public string Folder { get; set; } = string.Empty;
        public string ContentFolder { get; private set; } = string.Empty;
        public string AssetFolder { get; private set; } = string.Empty;
        public string ObjectFolder { get; private set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public AssetMonitor AssetMonitor { get; private set; } = null;

        public Project() { }
        public Project(GameEditor game, string name)
        {
            Folder = Path.GetDirectoryName(name);
            Name = Path.GetFileName(name);
            if(!Name.ToLower().EndsWith(".oce"))
            {
                Name += ".oce";
            }
            // Create Content folder for assets, and copy the mgcb template
            ContentFolder = Path.Combine(Folder, "Content");
            AssetFolder = Path.Combine(ContentFolder, "bin");
            ObjectFolder = Path.Combine(ContentFolder, "obj");
            char d = Path.DirectorySeparatorChar;
            if(!Directory.Exists(ContentFolder))
            {
                Directory.CreateDirectory(ContentFolder);
                Directory.CreateDirectory(AssetFolder);
                Directory.CreateDirectory(ObjectFolder);
                File.Copy($"ContentTemplate.mgcb", ContentFolder + $"{d}Content.mgcb");
            }
            AssetMonitor = new(ObjectFolder);
            AssetMonitor.OnAssetsUpdated += AssetMon_OnAssetsUpdated;
            // Add a default level
            AddLevel(game);
        }

        private void AssetMon_OnAssetsUpdated()
        {
            OnAssetsUpdated?.Invoke();
        }

        public void AddLevel(GameEditor game)
        {
            CurrentLevel = new();
            CurrentLevel.LoadContent(game);
            Levels.Add(CurrentLevel);
        }

        public void Update(float delta)
        {
            CurrentLevel?.Update(delta);
        }

        public void Render()
        {
            CurrentLevel.Render();
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(Levels.Count);
            int clIndex = Levels.IndexOf(CurrentLevel);
            foreach(Level level in Levels )
            {
                level.Serialize(stream);
            }
            stream.Write(clIndex);
            stream.Write(Folder);
            stream.Write(Name);
        }

        public void Deserialize(BinaryReader stream, GameEditor game)
        {
            int levelCount = stream.ReadInt32();
            for(int count = 0; count < levelCount; count++)
            {
                Level level = new Level();
                level.Deserialize(stream, game);
                Levels.Add(level);
            }
            int clIndex = stream.ReadInt32();
            CurrentLevel = Levels[clIndex];
            Folder = stream.ReadString();
            Name = stream.ReadString(); 
        }
    }
}
