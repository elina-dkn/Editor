using Editor;
using Editor.Editor;
using System.Threading;

// Set STA thread mode, for OpenFileDialog to work
Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

FormEditor editor = new();
editor.Game = new GameEditor(editor);
editor.Show();
editor.Game.Run();
