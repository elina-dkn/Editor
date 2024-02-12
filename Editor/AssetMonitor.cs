using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Editor.Editor
{
    public delegate void AssetsUpdated();

    internal enum AssetTypes
    {
        NONE,
        MODEL,
        TEXTURE,
        FONT,
        AUDIO,
        EFFECT,
        SFX
    }

    internal class AssetMonitor
    {
        public event AssetsUpdated OnAssetsUpdated;

        private readonly FileSystemWatcher _watcher = null;
        public Dictionary<AssetTypes, List<string>> Assets { get; private set; } = new();
        private readonly string _metaInfo = string.Empty;

        internal AssetMonitor(string asset)
        {
            _metaInfo = Path.Combine(asset, ".mgstats");
            _watcher = new FileSystemWatcher(asset);
            _watcher.Changed += OnChanged;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Filter = "*.mgstats";
            _watcher.IncludeSubdirectories = false;
            _watcher.EnableRaisingEvents = true;
        }

        public void UpdateAssetDB()
        {
            bool updated = false;
            AssetTypes assetType = AssetTypes.MODEL;
            if (!File.Exists(_metaInfo)) return;
            using var inStream = new FileStream(_metaInfo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(inStream);
            string[] content = streamReader.ReadToEnd().Split(Environment.NewLine);

            foreach(string line in content)
            {
                if(string.IsNullOrEmpty(line)) continue;
                string[] fields = line.Split(',');
                if (fields[0] == "Source File") continue;
                switch(fields[2])
                {
                    case "\"ModelProcessor\"":
                        assetType = AssetTypes.MODEL;
                        break;
                    case "\"TextureProcessor\"":
                        assetType = AssetTypes.TEXTURE;
                        break;
                    case "\"SongProcessor\"":
                        assetType = AssetTypes.AUDIO;
                        break;
                    case "\"EffectProcessor\"":
                        assetType = AssetTypes.EFFECT;
                        break;
                    case "\"SoundEffectProcessor\"":
                        assetType = AssetTypes.SFX;
                        break;
                    default:
                        Debug.Assert(false, "Unhandled processor");
                        break;
                }
                if (AddAsset(assetType, fields[1])) updated = true;
            }

            if (updated) OnAssetsUpdated?.Invoke();
        }

        private bool AddAsset(AssetTypes assetType, string assetName)
        {
            if (!Assets.ContainsKey(assetType)) Assets.Add(assetType, new());
            string assetNameWithoutExtension = Path.GetFileNameWithoutExtension(assetName);
            Assets[assetType].Add(assetNameWithoutExtension);
            return true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Assets.Clear();
            UpdateAssetDB();
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Assets.Clear();
            UpdateAssetDB();
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Assets.Clear();
            OnAssetsUpdated?.Invoke();
        }
    }
}
