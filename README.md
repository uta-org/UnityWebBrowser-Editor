# Unity Web Browser Editor

## Instalation

Add all this to your manifest.json inside Package folder (root project folder) before adding package.

```
	"com.cysharp.unitask": "https://github.com/uta-org/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
	"org.nuget.voltrpc": "3.0.0",
	"org.nuget.voltrpc.extension.memory": "1.1.0",
	"org.nuget.voltrpc.extension.vectors": "1.1.0",
	"dev.voltstro.nativearrayspanextensions": "https://github.com/Voltstro-Studios/NativeArraySpanExtensions.git",
	"dev.voltstro.unitywebbrowser": "https://github.com/uta-org/UnityWebBrowser.git?path=src/Packages/UnityWebBrowser",
	"dev.voltstro.unitywebbrowser.engine.cef": "https://github.com/uta-org/UnityWebBrowser.git?path=src/Packages/UnityWebBrowser.Engine.Cef",
	"dev.z3nth10n.unitywebbrowser.engine.cef.win.x64": "https://github.com/uta-org/UnityWebBrowser-Win64.git",
	"com.unity.editorcoroutines": "1.0.0",
	"com.unity.inputsystem": "1.4.2",
	"dev.z3nth10n.unitywebbrowser.dependencies": "https://github.com/uta-org/UnityWebBrowser-Dependencies.git",
```

Then, you can add this git package on the package manager window: https://github.com/uta-org/UnityWebBrowser-Editor.git?path=dev.z3nth10n.unitywebbrowser.editor

Also, you must add scopedRegistries to your manifest.json:

```json
	"scopedRegistries": [
		{
			"name": "Unity NuGet",
			"url": "https://unitynuget-registry.azurewebsites.net",
			"scopes": [
				"org.nuget"
			]
		}
	]
```

"dev.z3nth10n.unitywebbrowser.editor": "file:../../Packages/dev.z3nth10n.unitywebbrowser.editor",