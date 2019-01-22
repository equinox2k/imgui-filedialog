using System;
using ImGuiNET;

namespace Eq2k.FileDialog.Example
{
    public class MenuBar
    {
        public bool DarkMode { get; private set; }
        public bool DemoMode { get; private set; }

        public MenuBar()
        {
            DarkMode = true;
            DemoMode = false;
        }

        public void Render(out float menuHeight)
        {
            if (DarkMode)
            {
                ImGui.StyleColorsDark();
            }
            else
            {
                ImGui.StyleColorsLight();
            }

            menuHeight = 0.0f;
            if (!ImGui.BeginMainMenuBar())
            {
                return;
            }

            if (ImGui.BeginMenu("Theme"))
            {
                if (ImGui.MenuItem("Light", "", !DarkMode, DarkMode))
                {
                    DarkMode = false;
                }
                if (ImGui.MenuItem("Dark", "", DarkMode, !DarkMode))
                {
                    DarkMode = true;
                }
                if (ImGui.MenuItem("Demo", "", DemoMode))
                {
                    DemoMode = !DemoMode;
                }
                ImGui.EndMenu();
            }
            menuHeight = ImGui.GetWindowHeight();
            ImGui.EndMainMenuBar();
        }
    }
}
