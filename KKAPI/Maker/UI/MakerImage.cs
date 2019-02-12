using BepInEx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace KKAPI.Maker.UI
{
    public class MakerImage : BaseGuiEntry
    {
        private readonly BehaviorSubject<Texture> _texture;
        private int _width = 100;
        private int _height = 100;

        public Texture Texture
        {
            get => _texture.Value;
            set => _texture.OnNext(value);
        }

        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                _texture.OnNext(_texture.Value);
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                _texture.OnNext(_texture.Value);
            }
        }

        public MakerImage(Texture texture, MakerCategory category, BaseUnityPlugin owner) : base(category, owner)
        {
            _texture = new BehaviorSubject<Texture>(texture);

        }
        protected internal override void Initialize()
        {
        }

        public override void Dispose()
        {
        }

        protected override GameObject OnCreateControl(Transform subCategoryList)
        {
            var go = new GameObject("image" + GuiApiNameAppendix, typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(subCategoryList, false);
            go.layer = 5;
            var le = go.GetComponent<LayoutElement>();
            le.minWidth = 456;

            var ig = new GameObject("img", typeof(RectTransform), typeof(CanvasRenderer));
            ig.transform.SetParent(go.transform, false);
            ig.layer = 5;
            var i = ig.AddComponent<RawImage>();
            var irt = ig.GetComponent<RectTransform>();

            _texture.Subscribe(texture =>
            {
                i.texture = texture;
                le.minHeight = Height + 30;

                irt.offsetMin = new Vector2(-1 * Width / 2f, -1 * Height / 2f);
                irt.offsetMax = new Vector2(Width / 2f, Height / 2f);

                le.enabled = false;
                le.enabled = true;
            });

            return go;
        }
    }
}
