using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Adds a toggle to the bottom of the character card load window in character maker.
    /// Use to allow user to not load data related to your mod.
    /// Use with <see cref="AddLoadToggle"/>
    /// </summary>
    public class MakerLoadToggle : BaseEditableGuiEntry<bool>
    {
        private const int TotalWidth = 380 + 292 - 5; // -5 for KKP

        private static readonly List<MakerLoadToggle> Toggles = new List<MakerLoadToggle>();
        private static Transform _baseToggle;
        private static GameObject _root;

        private static int _createdCount;

        /// <summary>
        /// Create a new load toggle. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/> 
        /// with <see cref="RegisterCustomControlsEvent.AddLoadToggle"/>.
        /// </summary>
        /// <param name="text">Text displayed next to the checkbox</param>
        /// <param name="initialValue">Initial value of the toggle</param>
        public MakerLoadToggle(string text, bool initialValue = true) : base(null, initialValue, null)
        {
            Text = text;
        }
        
        /// <summary>
        /// Text displayed next to the toggle
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Check if any of the custom toggles are checked
        /// </summary>
        public static bool AnyEnabled => Toggles.Any(x => x.Value);

        internal static Button LoadButton { get; private set; }

        internal static void CreateCustomToggles()
        {
            foreach (var toggle in Toggles)
                toggle.CreateControl(_root.transform);
        }

        /// <inheritdoc />
        protected override GameObject OnCreateControl(Transform loadBoxTransform)
        {
            var copy = Object.Instantiate(_baseToggle, _root.transform);
            copy.name = "tglItem";

            RemoveLocalisation(copy.gameObject);

            var tgl = copy.GetComponentInChildren<Toggle>();
            tgl.onValueChanged.AddListener(SetValue);
            BufferedValueChanged.Subscribe(b => tgl.isOn = b);
            tgl.image.raycastTarget = true;
            tgl.graphic.raycastTarget = true;

            var txt = copy.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = Text;

            var singleItemWidth = TotalWidth / Toggles.Count;

            var rt = copy.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(_baseToggle.localPosition.x + singleItemWidth * _createdCount, 26);
            rt.offsetMax = new Vector2(rt.offsetMin.x + singleItemWidth, rt.offsetMin.y + 26);

            copy.gameObject.SetActive(true);
            _createdCount++;

            KoikatuAPI.Instance.StartCoroutine(FixLayout(rt));

            return copy.gameObject;
        }

        private static IEnumerator FixLayout(RectTransform rt)
        {
            yield return new WaitUntil(() => rt.gameObject.activeInHierarchy);
            yield return null;

            LayoutRebuilder.MarkLayoutForRebuild(rt);
        }

        /// <inheritdoc />
        protected internal override void Initialize() { }

        internal static MakerLoadToggle AddLoadToggle(MakerLoadToggle toggle)
        {
            if (toggle == null) throw new ArgumentNullException(nameof(toggle));
            if (toggle.IsDisposed) throw new ObjectDisposedException(nameof(toggle), "A new control has to be created every time maker is started");

            Toggles.Add(toggle);
            return toggle;
        }

        internal static void Reset()
        {
            foreach (var toggle in Toggles)
                toggle.Dispose();
            Toggles.Clear();
            _createdCount = 0;
            LoadButton = null;
        }

        internal static void Setup()
        {
            Reset();

            _root = GetRootObject();
            var baseToggles = GetBaseToggles(_root.transform);
            _baseToggle = baseToggles[0]; //.Single(x=>x.name == "tglItem01");

            var singleWidth = TotalWidth / baseToggles.Count;
            for (var index = 0; index < baseToggles.Count; index++)
            {
                var baseToggle = baseToggles[index];
                baseToggle.localPosition = new Vector3(_baseToggle.localPosition.x + singleWidth * index, 52, 0);
                baseToggle.offsetMax = new Vector2(baseToggle.offsetMin.x + singleWidth, baseToggle.offsetMin.y + 26);
            }

            var allon = _root.transform.Find("btnAllOn");
            allon.GetComponentInChildren<Button>().onClick.AddListener(OnAllOn);
            var alloff = _root.transform.Find("btnAllOff");
            alloff.GetComponentInChildren<Button>().onClick.AddListener(OnAllOff);

            LoadButton = _root.transform.parent.Find("btnLoad").GetComponent<Button>();
        }

        internal static List<RectTransform> GetBaseToggles(Transform root)
        {
            var baseToggles = new List<RectTransform>();
            foreach (var child in root.transform.Cast<Transform>().ToList())
            {
                if (child.name.StartsWith("tglHeader"))
                {
                    // Fix for Koikatsu Party, the toggles are now grouped
                    foreach (Transform tglItem in child.Cast<Transform>().ToList())
                    {
                        tglItem.SetParent(root.transform, false);
                        baseToggles.Add(tglItem.GetComponent<RectTransform>());
                    }
                }
                else if (child.name.StartsWith("tglItem"))
                {
                    baseToggles.Add(child.GetComponent<RectTransform>());
                }
            }

            return baseToggles;
        }

        private static GameObject GetRootObject()
        {
            return GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/charaFileControl/charaFileWindow/WinRect/CharaLoad/Select");
        }

        private static void OnAllOff()
        {
            foreach (var toggle in Toggles)
            {
                if (toggle != null)
                    toggle.Value = false;
            }
        }

        private static void OnAllOn()
        {
            foreach (var toggle in Toggles)
            {
                if (toggle != null)
                    toggle.Value = true;
            }
        }
    }
}
