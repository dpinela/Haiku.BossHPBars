using UE = UnityEngine;
using UI = UnityEngine.UI;
using MAPI = Modding;
using IO = System.IO;

namespace Haiku.BossHPBars
{
    internal class HPBar : UE.MonoBehaviour
    {
        public Func<bool>? modEnabled;
        public HP? bossHP;

        private UE.GameObject? barCanvas;
        private UE.GameObject? barPanel;
        private UE.GameObject? filledPortionPanel;
        private UI.Image? filledPortion;

        public void Start()
        {
            barCanvas = MAPI.CanvasUtil.CreateCanvas(1);
            barCanvas.name = "HPBarCanvas";
            barCanvas.transform.SetParent(gameObject.transform);

            var barSprite = LoadSprite("bar.png");
            barPanel = MAPI.CanvasUtil.CreateImagePanel(barCanvas, barSprite, new(
                new(barSprite.rect.width, barSprite.rect.height), new(0, 90)
            ));
            var filledBarSprite = LoadSprite("filled_bar.png");
            filledPortionPanel = MAPI.CanvasUtil.CreateImagePanel(barCanvas, filledBarSprite, new(
                new(filledBarSprite.rect.width, filledBarSprite.rect.height), new(0, 90)
            ));
            var img = filledPortionPanel.GetComponent<UI.Image>();
            img.type = UI.Image.Type.Filled;
            img.fillMethod = UI.Image.FillMethod.Horizontal;
            img.fillOrigin = 0;
            filledPortion = img;
        }

        public void Update()
        {
            var active = modEnabled!() && bossHP != null;
            barCanvas!.SetActive(active);
            if (active)
            {
                filledPortion!.fillAmount = (float)bossHP!.Current() / bossHP!.Max;
            }
        }

        private static UE.Sprite LoadSprite(string name)
        {
            var loc = IO.Path.Combine(IO.Path.GetDirectoryName(typeof(HPBar).Assembly.Location), name);
            var imageData = IO.File.ReadAllBytes(loc);
            var tex = new UE.Texture2D(1, 1, UE.TextureFormat.RGBA32, false);
            UE.ImageConversion.LoadImage(tex, imageData, true);
            tex.filterMode = UE.FilterMode.Point;
            return UE.Sprite.Create(tex, new UE.Rect(0, 0, tex.width, tex.height), new UE.Vector2(.5f, .5f), 100);
        }
    }
}