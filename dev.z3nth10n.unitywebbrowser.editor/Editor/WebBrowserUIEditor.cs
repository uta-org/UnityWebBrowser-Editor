using System.Reflection;
using UnityEngine;
using UnityEditor;
using VoltstroStudios.UnityWebBrowser.Communication;
using Vector2 = UnityEngine.Vector2;
using VoltstroStudios.UnityWebBrowser.Core;
using VoltstroStudios.UnityWebBrowser.Core.Engines;
using VoltstroStudios.UnityWebBrowser.Logging;
using VoltstroStudios.UnityWebBrowser.Shared.Core;
using VoltstroStudios.UnityWebBrowser.Shared.Events;
using EventType = UnityEngine.EventType;

public class WebBrowserUIEditor : EditorWindow
{
    private string url;

    [Tooltip("The browser client, what handles the communication between the CEF process and Unity")]
    public WebBrowserClient browserClient;

    private bool isResizing;
    private Vector2 lastWindowSize;

    public Rect WindowRect => new Rect(new Vector2(0, 30), position.size - Vector2.up * 30f);

    public const int scrollMultiplier = 10;

    [MenuItem("Window/UnityWebBrowser Editor")]
    private static void Init()
    {
        var window = (WebBrowserUIEditor)GetWindow(typeof(WebBrowserUIEditor));
        window.Show();
    }

    private void OnEnable()
    {
        if (browserClient != null)
        {
            return;

            //browserClient.Dispose();
            //browserClient = null;
        }

        browserClient = new WebBrowserClient()
        {
            engine = new EngineConfiguration()
            {
                engineAppName = "UnityWebBrowser.Engine.Cef",
                engineFiles = new Engine.EnginePlatformFiles[]
                {
                    new Engine.EnginePlatformFiles()
                    {
                        platform = Platform.Windows64,
                        engineFileLocation = "Assets/Dependencies/.UnityWebBrowser.Engine.Cef.Win-x64/Engine/"
                            // "Packages/dev.voltstro.unitywebbrowser.engine.cef.win.x64/Engine~/"
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
                }
            },
            communicationLayer = new TCPCommunicationLayer()
            {
                connectionTimeout = 20000,
                inPort = 60666,
                outPort = 60663
            },
            //backgroundColor = new Color32(255, 0, 0, 255)
        };
        browserClient.initialUrl =
        //"http://localhost:4200/";
         "https://material.angularjs.org/latest/demo/button";
        //"https://google.com";
        //browserClient.width = (uint)position.size.x;
        //browserClient.height = (uint)position.size.y;

        var init = browserClient.GetType().GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance);
        init.Invoke(browserClient, null);

        //browserClient.Init();
    }

    private void OnDisable()
    {
        browserClient.Dispose();
        browserClient = null;
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        if (!browserClient.ReadySignalReceived)
        {
            GUI.Label(new Rect(0, 0, position.width, 30), "Waiting for browser...");
            return;
        }

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