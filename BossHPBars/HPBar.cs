using UE = UnityEngine;
using UI = UnityEngine.UI;
using MAPI = Modding;
using IO = System.IO;

namespace Haiku.BossHPBars
{
    internal class HPBar : UE.MonoBehaviour
    {
        public Func<Settings>? modSettings;
        public HP? bossHP;

        private UE.GameObject? barCanvas;
        private UE.GameObject? barPanel;
        private UE.GameObject? filledPortionPanel;
        private UE.GameObject? numberPanel;
        private UI.Text? numberText;
        private UI.Image? filledPortion;

        public void Start()
        {
            barCanvas = MAPI.CanvasUtil.CreateCanvas(1);
            barCanvas.name = "HPBarCanvas";
            barCanvas.transform.SetParent(gameObject.transform);

            var barSprite = LoadSprite("bar.png");
            var barRect = new MAPI.CanvasUtil.RectData(
                new(barSprite.rect.width, barSprite.rect.height), new(0, 90)
            );
            barPanel = MAPI.CanvasUtil.CreateImagePanel(barCanvas, barSprite, barRect);
            var filledBarSprite = LoadSprite("filled_bar.png");
            filledPortionPanel = MAPI.CanvasUtil.CreateImagePanel(barCanvas, filledBarSprite, new(
                new(filledBarSprite.rect.width, filledBarSprite.rect.height), new(0, 90)
            ));
            numberPanel = MAPI.CanvasUtil.CreateTextPanel(barCanvas, "", 7, UE.TextAnchor.MiddleCenter, barRect, MAPI.CanvasUtil.GameFont);
            numberText = numberPanel.GetComponent<UI.Text>();
            var img = filledPortionPanel.GetComponent<UI.Image>();
            img.type = UI.Image.Type.Filled;
            img.fillMethod = UI.Image.FillMethod.Horizontal;
            img.fillOrigin = 0;
            filledPortion = img;
        }

        public void Update()
        {
            if (bossHP is HP hp)
            {
                var barActive = modSettings!().ShowBar.Value;
                var numsActive = modSettings!().ShowNumbers.Value;
                if (!(barActive || numsActive))
                {
                    barCanvas!.SetActive(false);
                    return;
                }
                barCanvas!.SetActive(true);

                var chp = hp.Current();
                barPanel!.SetActive(barActive);
                filledPortionPanel!.SetActive(barActive);
                if (barActive)
                {
                    filledPortion!.fillAmount = (float)chp / hp.Max;
                }
                
                numberPanel!.SetActive(numsActive);
                if (numsActive)
                {
                    numberText!.text = $"{chp} / {hp.Max}";
                }
            }
            else
            {
                barCanvas!.SetActive(false);
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