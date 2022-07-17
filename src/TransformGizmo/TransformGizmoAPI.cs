using System;
using BepInEx;
using KKAPI.Utilities;
using UnityEngine;

namespace TransformGizmoAPI
{
    [BepInPlugin(GUID, Name, Version)]
    public class TransformGizmoAPI : BaseUnityPlugin
    {
        public const string Name = "Transform Gizmo API";
        public const string GUID = "TransformGizmoAPI";
        public const string Version = "0.1";

        public static TransformGizmo Instance { get; private set; }
        private static Camera _lastCamera;
        internal static Material _lineShader;
        internal static Material _outlineShader;

        void Start()
        {
            //typeof(TransformGizmoAPI).Assembly.GetManifestResourceStream("TransformGizmoAPI.Shader.Resources.runtimegizmoshaders")
            var r = ResourceUtils.GetEmbeddedResource("runtimegizmoshaders", typeof(TransformGizmoAPI).Assembly) ?? throw new Exception("r");
            var ab = AssetBundle.LoadFromMemory(r) ?? throw new Exception("ab");
            foreach (var allAssetName in ab.GetAllAssetNames())
            {
                Console.WriteLine(allAssetName);
            }
            _lineShader = ab.LoadAsset<Material>("Lines") ?? throw new Exception("s1");
            _outlineShader = ab.LoadAsset<Material>("Outline") ?? throw new Exception("s2");

            ab.Unload(false);
        }

        void Update()
        {
            var main = Camera.main;
            if (main != _lastCamera)
            {
                Destroy(Instance);
                _lastCamera = main;
                if (main) Instance = main.gameObject.GetComponent<TransformGizmo>() ?? main.gameObject.AddComponent<TransformGizmo>();
            }
        }

        void OnDestroy()
        {
            Destroy(Instance);
        }
    }
}
