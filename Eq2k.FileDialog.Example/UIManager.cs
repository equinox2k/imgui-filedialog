using System;
using System.Numerics;
using System.Threading;
using ImGuiNET;
using Eq2k.FileDialog.Example.UI;

namespace Eq2k.FileDialog.Example
{
    public class UiManager
    {
        private readonly MenuBar _menuBar;
        private readonly PathPicker _filePicker;
        private readonly PathPicker _folderPicker;

        private string _filePath;
        private string _folderPath;

        public UiManager()
        {
            _filePath = string.Empty;
            _folderPath = string.Empty;

            _menuBar = new MenuBar();

            _filePicker = new PathPicker
            {
                Mode = PathPicker.ModeEnum.File
            };
            _folderPicker = new PathPicker
            {
                Mode = PathPicker.ModeEnum.Folder
            };
        }

        public void Render(float width, float height)
        {
            _menuBar.Render(out var menuHeight);
            if (_menuBar.DemoMode)
            {
                ImGui.ShowDemoWindow();
                return;
            }

            if (_filePicker.Render() && !_filePicker.Cancelled)
            {
                _filePath = _filePicker.SelectedFile;
            }

            if (_folderPicker.Render() && !_folderPicker.Cancelled)
            {
                _folderPath = _folderPicker.SelectedFolder;
            }

            if (ImGui.Begin("Demo", ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.Button("File Picker", new Vector2(100, 30)))
                {
                    _filePicker.ShowModal("/");
                }
                ImGui.SameLine();
                ImGui.Text(_filePath);

                if (ImGui.Button("Folder Picker", new Vector2(100, 30)))
                {
                    _folderPicker.ShowModal("/");
                }
                ImGui.SameLine();
                ImGui.Text(_folderPath);

                ImGui.End();
            }

            var io = ImGui.GetIO();
            io.DeltaTime = 2.0f;
        }
    }
}
