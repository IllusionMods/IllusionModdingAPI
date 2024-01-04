using System.Text;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Utilities;
using UnityEngine;

namespace KKAPI
{
    internal partial class CardAuthorData
    {
        [JetBrains.Annotations.UsedImplicitly]
        private sealed class CardAuthorDataWindow : ImguiWindow<CardAuthorDataWindow>
        {
            private readonly StringBuilder _sb = new StringBuilder();
            private CardAuthorDataController _controller;
            private Vector2 _scrollPos;

            public static void ToggleShowWindow()
            {
                if (!MakerAPI.InsideAndLoaded)
                    return;

                // Attach to maker base so that this is destroyed when exiting maker
                var makerBase = MakerAPI.GetMakerBase().gameObject;
                var window = makerBase.GetComponent<CardAuthorDataWindow>();

                if (!window) window = makerBase.AddComponent<CardAuthorDataWindow>();

                // Newly created windows are disabled by default
                window.enabled = !window.enabled;
            }

            private void Awake()
            {
                Title = "Author history";
            }

            protected override Rect GetDefaultWindowRect(Rect screenRect)
            {
                return new Rect(80, 80, 350, 350);
            }

            protected override void OnEnable()
            {
                _controller = MakerAPI.GetCharacterControl().GetComponent<CardAuthorDataController>();
                base.OnEnable();
            }

            private void Update()
            {
                if (!MakerAPI.InsideAndLoaded)
                    DestroyImmediate(this);
            }

            protected override void DrawContents()
            {
                GUILayout.Label("Character: " + _controller.ChaFileControl.GetFancyCharacterName(), GUI.skin.box, IMGUIUtils.EmptyLayoutOptions);

                GUILayout.Label("This card was modified or resaved by following people:\n(in chronological order, starting from oldest edits)", IMGUIUtils.EmptyLayoutOptions);

                _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true, IMGUIUtils.EmptyLayoutOptions);
                GUILayout.BeginVertical(IMGUIUtils.EmptyLayoutOptions);
                {
                    var i = 0;
                    foreach (var author in _controller.Authors)
                    {
                        i++;
                        GUILayout.BeginHorizontal(GUI.skin.box, IMGUIUtils.EmptyLayoutOptions);
                        {
                            _sb.Length = 0;
                            _sb.Append('#');
                            _sb.Append(i);
                            if (i < 10) _sb.Append(' ');
                            _sb.Append(' ');
                            _sb.Append('\t');
                            _sb.Append(author);
                            GUILayout.Label(_sb.ToString(), GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();
                    }

                    if (i == 0)
                        GUILayout.Label("This card has no author history", GUILayout.ExpandWidth(true));
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }
    }
}
