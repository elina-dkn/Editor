using Editor.Editor;
using Editor.Engine;
using Editor.Engine.Interfaces;
using Editor.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Editor
{
    public partial class FormEditor : Form
    {
        public GameEditor Game { get => _game; set { _game = value; HookEvents(); } }
        private GameEditor _game = null;
        private Process _MGCBProcess = null;
        private Object dropped = null;

        public FormEditor()
        {
            InitializeComponent();
            KeyPreview = true;
            toolStripStatusLabel.Text = Directory.GetCurrentDirectory();
            listBoxAssets.MouseDown += ListBoxAssets_MouseDown;
        }

        private void ListBoxAssets_MouseDown(object sender, MouseEventArgs e)
        {
            if (listBoxAssets.Items.Count == 0) return;

            int index = listBoxAssets.IndexFromPoint(e.X, e.Y);
            var lia = listBoxAssets.Items[index] as ListItemAsset;
            if(lia.Type == AssetTypes.MODEL || lia.Type == AssetTypes.TEXTURE || lia.Type == AssetTypes.EFFECT || lia.Type == AssetTypes.SFX)
            {
                DoDragDrop(lia, DragDropEffects.Copy);
            }
        }

        private void HookEvents()
        {
            Form gameForm = Control.FromHandle(_game.Window.Handle) as Form;
            gameForm.MouseDown += GameForm_MouseDown;
            gameForm.MouseUp += GameForm_MouseUp;
            gameForm.MouseWheel += GameForm_MouseWheel;
            gameForm.MouseMove += GameForm_MouseMove;
            KeyDown += GameForm_KeyDown;
            KeyUp += GameForm_KeyUp;

            gameForm.DragDrop += GameForm_DragDrop;
            gameForm.DragOver += GameForm_DragOver;
            gameForm.AllowDrop = true;  
        }

        private void GameForm_DragOver(object sender, DragEventArgs e)
        {
            dropped = null;
            Form gameForm = Control.FromHandle(_game.Window.Handle) as Form;
            var p = gameForm.PointToClient(new System.Drawing.Point(e.X, e.Y));
            InputController.Instance.MousePosition = new Vector2(p.X, p.Y);
            e.Effect = DragDropEffects.None;
            if (e.Data.GetDataPresent(typeof(ListItemAsset)))
            {
                var lia = e.Data.GetData(typeof(ListItemAsset)) as ListItemAsset;
                ISelectable obj = _game.Project.CurrentLevel.HandlePick(false);
                if (lia.Type == AssetTypes.MODEL)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else if (lia.Type == AssetTypes.TEXTURE || lia.Type == AssetTypes.EFFECT)
                {
                    if (obj is IMaterial) dropped = obj as IMaterial;
                }
                else if (lia.Type == AssetTypes.SFX)
                {
                    if (obj is ISoundEmitter) dropped = obj as ISoundEmitter;
                }
                if (dropped != null)
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }


        private void GameForm_DragDrop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(typeof(ListItemAsset)))
            {
                var lia = e.Data.GetData(typeof(ListItemAsset)) as ListItemAsset;
                if(lia.Type == AssetTypes.MODEL)
                {
                    Models model = new(_game, lia.Name, "DefaultTexture", "DefaultEffect", Vector3.Zero, 1.0f);
                    _game.Project.CurrentLevel?.AddModel(model);
                    listBoxLevel.Items.Add(new ListItemLevel() { Model = model});
                }
                else if(lia.Type == AssetTypes.TEXTURE) 
                {
                    IMaterial material = dropped as IMaterial;
                    material?.SetTexture(_game, lia.Name);
                }
                else if(lia.Type == AssetTypes.EFFECT)
                {
                    IMaterial material = dropped as IMaterial;
                    material?.SetShader(_game, lia.Name);
                }
                else if (lia.Type == AssetTypes.SFX)
                {
                    ISoundEmitter emitter = (ISoundEmitter)dropped;
                    ContextMenuStrip menuStrip = new();
                    var items = Enum.GetNames(typeof(SoundEffectTypes));
                    int index = 0;
                    foreach(var i in items)
                    {
                        ToolStripMenuItem menuItem = new(i);
                        menuItem.Click += MenuItem_Click;
                        menuItem.Name = index.ToString();
                        menuItem.Tag = lia;
                        menuStrip.Items.Add(menuItem);
                        index++;
                    }
                    menuStrip.Show(new System.Drawing.Point(e.X, e.Y));
                }
            }
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            ISoundEmitter emitter = (ISoundEmitter)dropped;
            var tmi = sender as ToolStripMenuItem;
            int index = Int32.Parse(tmi.Name);
            var lia = tmi.Tag as ListItemAsset;
            SoundEffect ef = _game.Content.Load<SoundEffect>(lia.Name);
            SoundEffectInstance efi = ef.CreateInstance();
            efi.Volume = 1;
            efi.IsLooped = false;
            emitter.SoundEffects[index] = efi;
        }
    

        private void GameForm_MouseMove(object sender, MouseEventArgs e)
        {
            var p = new Vector2(e.Location.X, e.Location.Y);
            InputController.Instance.MousePosition = p;
        }

        private void GameForm_MouseWheel(object sender, MouseEventArgs e)
        {
            InputController.Instance.SetWheel(e.Delta / SystemInformation.MouseWheelScrollDelta);
        }

        private void GameForm_MouseUp(object sender, MouseEventArgs e)
        {
            InputController.Instance.SetButtonUp(e.Button);
            var p = new Vector2(e.Location.X, e.Location.Y);
            InputController.Instance.DragEnd = p;
        }

        private void GameForm_MouseDown(object sender, MouseEventArgs e)
        {
            InputController.Instance.SetButtonDown(e.Button);
            var p = new Vector2(e.Location.X, e.Location.Y);
            InputController.Instance.DragStart = p;
        }

        private void GameForm_KeyUp(object sender, KeyEventArgs e)
        {
            InputController.Instance.SetKeyUp(e.KeyCode);
            e.Handled = true;
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            InputController.Instance.SetKeyDown(e.KeyCode);
            e.Handled = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Game.Exit();
        }

        private void FormEditor_SizeChanged(object sender, EventArgs e)
        {
            Game?.AdjustAspectRatio();
        }

        private void splitContainer_Panel1_SizeChanged(object sender, EventArgs e)
        {
            Game?.AdjustAspectRatio();
        }

        private void splitContainer_Panel2_SizeChanged(object sender, EventArgs e)
        {
            Game?.AdjustAspectRatio();
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Game.Project = new Project(Game, sfd.FileName);
                Game.Project.OnAssetsUpdated += Project_OnAssetsUpdated;
                Game.Project.AssetMonitor.UpdateAssetDB();
                Text = "Our Cool Editor - " + Game.Project.Name;
                Game.AdjustAspectRatio();
            }
            saveToolStripMenuItem_Click(sender, e);
        }

        private void Project_OnAssetsUpdated()
        {
            this.Invoke(delegate
            {
                listBoxAssets.Items.Clear();
                var assets = Game.Project.AssetMonitor.Assets;
                if (!assets.ContainsKey(AssetTypes.MODEL)) return;
                foreach(AssetTypes assetType in Enum.GetValues(typeof(AssetTypes)))
                {
                    if(assets.ContainsKey(assetType))
                    {
                        listBoxAssets.Items.Add(new ListItemAsset()
                        {
                            Name = assetType.ToString().ToUpper() + "S:",
                            Type = AssetTypes.NONE
                        });
                        foreach(string asset in assets[assetType])
                        {
                            ListItemAsset lia = new()
                            {
                                Name = asset,
                                Type = assetType
                            };
                            listBoxAssets.Items.Add(lia);
                        }
                        listBoxAssets.Items.Add(new ListItemAsset()
                        {
                            Name = " ",
                            Type = AssetTypes.NONE
                        });
                    }
                }
            });
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fname = Path.Combine(Game.Project.Folder, Game.Project.Name);
            using var stream = File.Open(fname, FileMode.Create);
            using var writer = new BinaryWriter(stream, Encoding.UTF8, false);
            Game.Project.Serialize(writer);
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "OCE Files|*.oce";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using var stream = File.Open(ofd.FileName, FileMode.Open);
                using var reader = new BinaryReader(stream, Encoding.UTF8, false);
                Game.Project = new();
                Game.Project.Deserialize(reader, Game);
                Game.Project.AssetMonitor.UpdateAssetDB();
                Text = "Our Cool Editor - " + Game.Project.Name;
                Game.AdjustAspectRatio();
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string mgcbEditorPath = ConfigurationManager.AppSettings["MGCB_EditorPath"];
            ProcessStartInfo startInfo = new()
            {
                FileName = "\"" + Path.Combine(mgcbEditorPath, "mgcb-editor-windows.exe") + "\"",
                Arguments = "\"" + Path.Combine(Game.Project.ContentFolder, "Content.mgcb") + "\""
            };
            _MGCBProcess = Process.Start(startInfo);
        }

        private void FormEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_MGCBProcess == null) return;
            _MGCBProcess.Kill();
        }

        private void listBoxLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listBoxLevel.Items.Count == 0) return;

            Game.Project.CurrentLevel.ClearSelectedModels();
            int index = listBoxLevel.SelectedIndex;
            if(index == -1) return;
            var lia = listBoxLevel.Items[index] as ListItemLevel;
            lia.Model.Selected = true;

        }
    }
}
