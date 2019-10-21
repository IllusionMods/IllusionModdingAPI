using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace KKAPI.Maker.UI
{
    /// <summary>
    /// Adds a toggle to the bottom of the coordinate/clothes card load window in character maker.
    /// Use to allow user to not load data related to your mod.
    /// Use with <see cref="AddLoadToggle"/>
    /// </summary>
    public class MakerCoordinateLoadToggle : BaseEditableGuiEntry<bool>
    {
        static MakerCoordinateLoadToggle()
        {
            var cloInstalled = Chainloader.Plugins.Where(x => x != null).Select(MetadataHelper.GetMetadata).Any(x => x.GUID == "KK_ClothesLoadOption");
            _verticalOffset = cloInstalled ? 52 : 26;
        }

        private const int TotalWidth = 380 + 292;
        private static readonly int _verticalOffset;
        private static readonly List<MakerCoordinateLoadToggle> Toggles = new List<MakerCoordinateLoadToggle>();
        private static Transform _baseToggle;
        private static GameObject _root;

        private static int _createdCount;
        private static List<RectTransform> _baseToggles;

        /// <summary>
        /// Create a new coordinate load toggle. Create and register it in <see cref="MakerAPI.RegisterCustomSubCategories"/> 
        /// with <see cref="RegisterCustomControlsEvent.AddCoordinateLoadToggle"/>.
        /// </summary>
        /// <param name="text">Text displayed next to the checkbox</param>
        /// <param name="initialValue">Initial value of the toggle</param>
        public MakerCoordinateLoadToggle(string text, bool initialValue = true) : base(null, initialValue, null)
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

            var singleWidth = TotalWidth / (_baseToggles.Count + Toggles.Count);
            for (var index = 0; index < _baseToggles.Count; index++)
            {
                var baseToggle = _baseToggles[index];
                baseToggle.localPosition = new Vector3(_baseToggle.localPosition.x + singleWidth * index, _verticalOffset, 0);
                baseToggle.offsetMax = new Vector2(baseToggle.offsetMin.x + singleWidth, baseToggle.offsetMin.y + 26);
            }
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

            var singleWidth = TotalWidth / (_baseToggles.Count + Toggles.Count);

            var rt = copy.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(_baseToggle.localPosition.x + singleWidth * _createdCount, _verticalOffset, 0);
            rt.offsetMax = new Vector2(rt.offsetMin.x + singleWidth, rt.offsetMin.y + 26);

            copy.gameObject.SetActive(true);
            _createdCount++;

            KoikatuAPI.Instance.StartCoroutine(FixLayout(rt));

            return copy.gameObject;
        }

        private static IEnumerator FixLayout(RectTransform rt)
        {
            yield return new WaitUntil(() => rt == null || rt.gameObject.activeInHierarchy);
            yield return null;

            LayoutRebuilder.MarkLayoutForRebuild(rt);
        }

        /// <inheritdoc />
        protected internal override void Initialize() { }

        internal static MakerCoordinateLoadToggle AddLoadToggle(MakerCoordinateLoadToggle toggle)
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
            _baseToggles = MakerLoadToggle.GetBaseToggles(_root.transform);
            _createdCount = _baseToggles.Count;
            _baseToggle = _baseToggles[0];

            /*var allon = _root.transform.Find("btnAllOn");
            allon.GetComponentInChildren<Button>().onClick.AddListener(OnAllOn);
            var alloff = _root.transform.Find("btnAllOff");
            alloff.GetComponentInChildren<Button>().onClick.AddListener(OnAllOff);*/

            LoadButton = _root.transform.parent.Find("btnLoad").GetComponent<Button>();
        }

        private static GameObject GetRootObject()
        {
            return GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsMenuTree/06_SystemTop/cosFileControl/charaFileWindow/WinRect/CoordinateLoad/Select");
        }

        /*private static void OnAllOff()
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
        }*/
    }
}
