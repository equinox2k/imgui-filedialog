using System;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

//using static ImGuiNET.ImGuiNative;

namespace Eq2k.FileDialog.Example
{
    class Program
    {
        private static UiManager _uiManager;
        private static Sdl2Window _window;
        private static GraphicsDevice _gd;
        private static CommandList _cl;
        private static ImGuiRenderer _controller;

        static void Main(string[] args)
        {
            _uiManager = new UiManager();

            // Create window, GraphicsDevice, and all resources necessary for the demo.
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(50, 50, 1000, 720, WindowState.Normal, "FileDialog Demo"),
                new GraphicsDeviceOptions(true, null, true),
                out _window,
                out _gd);

            _window.Resized += _window_Resized;

            _cl = _gd.ResourceFactory.CreateCommandList();

            _controller = new ImGuiRenderer(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);

            ImGui.StyleColorsLight();

            // Main application loop
            while (_window.Exists)
            {
                InputSnapshot snapshot = _window.PumpEvents();
                if (!_window.Exists)
                {
                    break;
                }
                _controller.Update(1f / 60f, snapshot); 

                SubmitUI();

                _cl.Begin();
                _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
                _cl.ClearColorTarget(0, new RgbaFloat(0.5f, 0.5f, 0.5f, 1f));
                _controller.Render(_gd, _cl);
                _cl.End();
                _gd.SubmitCommands(_cl);
                _gd.SwapBuffers(_gd.MainSwapchain);
            }

            // Clean up Veldrid resources
            _gd.WaitForIdle();
            _controller.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        static void _window_Resized()
        {
            _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
            _controller.WindowResized(_window.Width, _window.Height);
        }

        private static void SubmitUI()
        {
            _uiManager.Render(_window.Width, _window.Height);
        }
    }
}
