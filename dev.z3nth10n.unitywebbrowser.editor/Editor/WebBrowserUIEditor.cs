using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using VoltstroStudios.UnityWebBrowser.Communication;
using Vector2 = UnityEngine.Vector2;
using VoltstroStudios.UnityWebBrowser.Core;
using VoltstroStudios.UnityWebBrowser.Core.Engines;
using VoltstroStudios.UnityWebBrowser.Shared.Core;
using VoltstroStudios.UnityWebBrowser.Shared.Events;
using EventType = UnityEngine.EventType;

public class WebBrowserUIEditor : EditorWindow
{
    private string url;

    public WebBrowserClient browserClient;

    private bool isResizing;
    private Vector2 lastWindowSize;
    private static ListRequest packages;

    public Rect WindowRect => new Rect(new Vector2(0, 30), position.size - Vector2.up * 30f);

    public const int scrollMultiplier = 10;

    [MenuItem("Window/UnityWebBrowser Editor")]
    private static void Init()
    {
        var window = (WebBrowserUIEditor)GetWindow(typeof(WebBrowserUIEditor));
        window.Show();
    }

    private static string GetEnginePath(Platform platform)
    {
        var needle = "unitywebbrowser.engine.cef.";

        switch (platform)
        {
            case Platform.Windows64:
                needle += "win.x64";
                break;

            case Platform.Linux64:
                needle += "linux.x64";
                break;

            case Platform.MacOS:
                needle += "macos.x64";
                break;
        }

        //Debug.Log(string.Join(Environment.NewLine, packages.Result.Select(i => $"{i.packageId}, {i.assetPath}, {i.resolvedPath}")));

        var info = packages.Result.FirstOrDefault(p => p.packageId.Contains(needle));
        var folder = info.resolvedPath;
            // folders.FirstOrDefault(f => f.Contains(needle));

        folder = Path.Combine(folder, "Engine~");
        folder = folder.Replace("\\", "/");

        if (!folder.EndsWith("/"))
            folder += "/";

        return folder;
    }

    private void OnEnable()
    {
        if (browserClient != null)
        {
            return;

            //browserClient.Dispose();
            //browserClient = null;
        }

        packages = Client.List(false);
    }

    private void InitBrowser()
    {
        browserClient = (WebBrowserClient)Activator.CreateInstance(typeof(WebBrowserClient), BindingFlags.NonPublic | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture);

        var engineConfig = CreateInstance<EngineConfiguration>();

        engineConfig.engineAppName = "UnityWebBrowser.Engine.Cef";
        engineConfig.engineFiles = new Engine.EnginePlatformFiles[]
        {
            new Engine.EnginePlatformFiles
            {
                platform = Platform.Windows64,
                engineFileLocation = GetEnginePath(Platform.Windows64)
            },
            //new Engine.EnginePlatformFiles()
            //{
            //    platform = Platform.Linux64,
            //    engineFileLocation = "Packages/dev.voltstro.unitywebbrowser.engine.cef.linux.x64/Engine~/"
            //},
            //new Engine.EnginePlatformFiles()
            //{
            //    platform = Platform.MacOS,
            //    engineFileLocation = "Packages/dev.voltstro.unitywebbrowser.engine.cef.macos.x64/Engine~/"
            //}
        };
        browserClient.engine = engineConfig;

        var communicationConfig = CreateInstance<TCPCommunicationLayer>();

        communicationConfig.connectionTimeout = 20000;
        communicationConfig.inPort = 60666;
        communicationConfig.outPort = 60663;

        browserClient.communicationLayer = communicationConfig;
        //backgroundColor = new Color32(255, 0, 0, 255)

        browserClient.initialUrl =
            //"http://localhost:4200/";
            "https://material.angularjs.org/latest/demo/button";
        //"https://google.com";
        //browserClient.width = (uint)position.size.x;
        //browserClient.height = (uint)position.size.y;

        var init = browserClient.GetType().GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);
        init.Invoke(browserClient, null);
    }

    private void OnDisable()
    {
        browserClient.Dispose();
        browserClient = null;
    }

    private void OnInspectorUpdate()
    {
        if (packages != null && packages.IsCompleted && browserClient == null)
        {
            InitBrowser();
        }

        Repaint();
    }

    private void OnGUI()
    {
        if (browserClient == null || !browserClient.ReadySignalReceived)
        {
            GUI.Label(new Rect(0, 0, position.width, 30), "Waiting for browser...");
            return;
        }

        browserClient.LoadTextureData();

        //Debug.Log("Ready!");

        var e = Event.current;

        // TODO
        //if (!isResizing)
        //{
        //    Debug.Log("resize!");
        //    browserClient.width = (uint)position.size.x;
        //    browserClient.height = (uint)position.size.y;
        //}

        DoMouseControllers(e);

        url = GUI.TextField(new Rect(10f, 5, position.size.x - 50f, 20), url);
        if (GUI.Button(new Rect(position.size.x - 30f, 5, 20, 20), ">"))
        {
            browserClient.LoadUrl(url);
        }
        GUI.DrawTexture(WindowRect, browserClient.BrowserTexture); // , new Rect(0, 0, 1, -1));

        if (position.size != lastWindowSize)
            isResizing = true;
        else if (Event.current.isMouse && Event.current.type == EventType.MouseUp)
            isResizing = false;

        lastWindowSize = position.size;
    }

    private void DoMouseControllers(Event e)
    {
        var mousePosition = TranslateMousePosition(e.mousePosition);
        browserClient.SendMouseMove(mousePosition);

        if (e.isMouse)
        {
            //Debug.Log($"Click at {e.mousePosition}");
            browserClient.SendMouseClick(mousePosition, e.clickCount, GetMouseClickType(e.button), (MouseEventType)e.type);
        }

        if (e.isScrollWheel)
        {
            //Debug.Log(e.delta);
            browserClient.SendMouseScroll(mousePosition, (int)-(e.delta.y * scrollMultiplier));
        }
    }

    private Vector2Int TranslateMousePosition(Vector2 pos)
    {
        var browserSize = new Vector2(browserClient.Resolution.Width, browserClient.Resolution.Height);
        var size = WindowRect.size;
        var aspectRatio = browserSize / size;

        var p = (pos - WindowRect.position) * aspectRatio;
        return new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
    }

    public MouseClickType GetMouseClickType(int type)
    {
        switch (type)
        {
            case 0:
                return MouseClickType.Left;

            case 1:
                return MouseClickType.Right;

            case 2:
                return MouseClickType.Middle;
        }

        return default;
    }
}
