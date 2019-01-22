﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace Eq2k.FileDialog.Example.UI
{
    public class PathPicker
    {
        public enum ModeEnum
        {
            File,
            Folder
        }

        private bool Like(string str, string pattern)
        {
            return new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(str);
        }

        private bool _showModal;
        private bool _open;

        public ModeEnum Mode { get; set; }

        private string _selectedFolder; //todo tidy + spacing
        public string SelectedFolder
        {
            get
            {
                return _selectedFolder;
            }
            set
            {
                _selectedFolder = value;
            }
        }

        public string SelectedFile { get; private set; }
        public bool Cancelled { get; private set; }
        public string[] AllowedFiles { get; set; }
        public bool ShowHidden { get; set; }

        public void ShowModal(string path)
        {
            _showModal = true;
            SelectedFolder = path;
            SelectedFile = string.Empty;
        }

        private void CloseModal()
        {
            _open = false;
            ImGui.CloseCurrentPopup();
        }

        public PathPicker()
        {
            Mode = ModeEnum.File;
            AllowedFiles = new string[] { "*.*" };
            ShowHidden = false;
        }

        private string[] GetSpecialFolders()
        {
            var specialFolders = new List<string>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                specialFolders.Add($"/|/");
                specialFolders.Add($"Home|{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}");
                specialFolders.Add($"Desktop|{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}");
                specialFolders.Add($"Documents|{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Documents");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                //TODO
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //TODO
            }

            var logicalDrives = Directory.GetLogicalDrives();
            foreach (var logicalDrive in logicalDrives)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (logicalDrive.StartsWith("/Volume", StringComparison.CurrentCultureIgnoreCase))
                    {
                        specialFolders.Add($"{Path.GetFileName(logicalDrive)}|{logicalDrive}");
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    //TODO
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    //TODO
                }
            }

            return specialFolders.ToArray();
        }


        private void DrawLines(Vector2[] points, Vector2 location, float size)
        {
            var iconColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1));
            var drawList = ImGui.GetWindowDrawList();
            for (var i = 0; i < points.Length; i += 2)
            {
                var vector1 = (points[i] / 100) * size;
                var vector2 = (points[i + 1] / 100) * size;
                drawList.AddLine(location + vector1, location + vector2, iconColor);
            }
        }

        private void GenerateFileIcon(Vector2 location, float size)
        {
            var points = new[] {
                new Vector2(0.0f,0.0f), new Vector2(45.0f, 0.0f),
                new Vector2(45.0f,0.0f), new Vector2(55.0f, 22.5f),
                new Vector2(55.0f,22.5f), new Vector2(100.0f, 22.5f),
                new Vector2(100.0f,22.5f), new Vector2(100.0f, 87.5f),
                new Vector2(100.0f,87.5f), new Vector2(0.0f, 87.5f),
                new Vector2(0.0f,87.5f), new Vector2(0.0f, 0.0f)
            };
            DrawLines(points, location, size);
        }

        private void GenerateFolderIcon(Vector2 location, float size)
        {
            var points = new[] {
                new Vector2(12.5f,0.0f), new Vector2(62.5f, 0.0f),
                new Vector2(62.5f,0.0f), new Vector2(87.5f, 50.0f),
                new Vector2(87.5f,50.0f), new Vector2(87.5f, 100.0f),
                new Vector2(87.5f,100.0f), new Vector2(12.5f, 100.0f),
                new Vector2(12.5f,100.0f), new Vector2(12.5f, 0.0f),
                new Vector2(62.5f,0.0f), new Vector2(62.5f, 50.0f),
                new Vector2(62.5f,50.0f), new Vector2(87.5f, 50.0f)
            };
            DrawLines(points, location, size);
        }

        private bool ProcessChildFolders(string path)
        {
            var result = false;

            foreach (var fse in Directory.EnumerateFileSystemEntries(path))
            {
                string name = Path.GetFileName(fse);

                FileAttributes attributes = File.GetAttributes(fse);
                var isHidden = (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

                if (!ShowHidden && isHidden)
                {
                    continue;
                }

                var isDirectory = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
                if (isDirectory)
                {
                    var iconPosition = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                    iconPosition.Y -= ImGui.GetScrollY();

                    var lineHeight = ImGui.GetTextLineHeight();
                    ImGui.SetCursorPosX(lineHeight * 2);

                    if (ImGui.Selectable(name, false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        SelectedFile = string.Empty;
                        _selectedFolder = fse;
                    }

                    GenerateFolderIcon(iconPosition, lineHeight);
                }
            }

            return result;
        }

        private bool ProcessChildFiles(string path)
        {
            var result = false;

            if (Mode == ModeEnum.Folder)
            {
                return result;
            }

            foreach (var fse in Directory.EnumerateFileSystemEntries(path))
            {
                string name = Path.GetFileName(fse);

                FileAttributes attributes = File.GetAttributes(fse);
                var isHidden = (attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

                if (!ShowHidden && isHidden)
                {
                    continue;
                }

                var isDirectory = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
                if (!isDirectory)
                {
                    var allowed = false;
                    foreach (var allowedFile in AllowedFiles)
                    {
                        allowed |= Like(name, allowedFile);
                    }

                    if (!allowed)
                    {
                        continue;
                    }

   
                    var iconPosition = ImGui.GetWindowPos() + ImGui.GetCursorPos();
                    iconPosition.Y -= ImGui.GetScrollY();

                    var lineHeight = ImGui.GetTextLineHeight();
                    ImGui.SetCursorPosX(lineHeight * 2);

                    bool isSelected = SelectedFile == fse;
                    if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups | ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        SelectedFile = fse;
                        if (ImGui.IsMouseDoubleClicked(0))
                        {
                            Cancelled = false;
                            result = true;
                            ImGui.CloseCurrentPopup();
                        }
                    }

                    GenerateFileIcon(iconPosition, lineHeight);

                }
            }

            return result;
        }

        public bool Render()
        {
            if (_showModal)
            {
                _showModal = false;
                _open = true;
                ImGui.OpenPopup($"{Mode} Browser");
            }

            if (!_open)
            {
                return false;
            }

            var result = false;

            if (!ImGui.BeginPopupModal($"{Mode} Browser"))
            {
                return result;
            }

            if (ImGui.IsWindowAppearing())
            {
                ImGui.SetWindowSize(new Vector2(800, 600));
            }

            var size = ImGui.GetWindowSize();

            ImGui.PushItemWidth(size.X - 16);
            ImGui.InputText("###file-path", ref _selectedFolder, 300, ImGuiInputTextFlags.ReadOnly);

            if (ImGui.BeginChildFrame(1, new Vector2(200, size.Y - 96), ImGuiWindowFlags.None))
            {
                var specialFolders = GetSpecialFolders();
                foreach (var specialFolder in specialFolders)
                {
                    var parts = specialFolder.Split('|');
                    if (ImGui.Selectable(parts[0], false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        _selectedFolder = parts[1];
                    }
                }
                ImGui.EndChildFrame();
            }

            ImGui.SameLine();
            if (ImGui.BeginChildFrame(2, new Vector2(size.X - 224, size.Y - 96), ImGuiWindowFlags.None))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(_selectedFolder);
                if (directoryInfo.Parent != null)
                {
                    if (ImGui.Selectable("..", false, ImGuiSelectableFlags.DontClosePopups))
                    {
                        _selectedFolder = directoryInfo.Parent.FullName;
                    }
                }
                try
                {
                    result |= ProcessChildFolders(directoryInfo.FullName);
                    result |= ProcessChildFiles(directoryInfo.FullName);
                }
                catch 
                {
                    Debug.Print($"Unable to process path '{directoryInfo.FullName}'.");
                }
                ImGui.EndChildFrame();
            }

            ImGui.Spacing();
            ImGui.SetCursorPosX(size.X - 216);
            if (ImGui.Button("Cancel", new Vector2(100, 30)))
            {
                Cancelled = true;
                result = true;
                CloseModal();
            }
            ImGui.SameLine();
            if (ImGui.Button("Open", new Vector2(100, 30)))
            {
                var valid = false;
                valid |= Mode == ModeEnum.File && !string.IsNullOrEmpty(SelectedFile);
                valid |= Mode == ModeEnum.Folder && !string.IsNullOrEmpty(SelectedFolder);
                if (valid)
                {
                    Cancelled = false;
                    result = true;
                    CloseModal();
                }
            }

            ImGui.EndPopup();

            return result;
        }
    }
}
