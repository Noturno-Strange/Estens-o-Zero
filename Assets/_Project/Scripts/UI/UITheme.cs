using ExtensaoZero.Telephone;
using UnityEngine;

namespace ExtensaoZero.UI
{
    /// <summary>
    /// Paleta e tipografia da interface. Contraste alto, poucas cores, nada de efeitos
    /// que atrapalhem a leitura das legendas.
    /// </summary>
    public static class UITheme
    {
        public static readonly Color Paper = new Color(0.85f, 0.81f, 0.70f);
        public static readonly Color Amber = new Color(0.95f, 0.66f, 0.24f);
        public static readonly Color Green = new Color(0.48f, 0.85f, 0.56f);
        public static readonly Color Red = new Color(0.88f, 0.35f, 0.30f);
        public static readonly Color Dim = new Color(0.55f, 0.58f, 0.55f);
        public static readonly Color Ink = new Color(0.04f, 0.05f, 0.045f);
        public static readonly Color Panel = new Color(0.035f, 0.042f, 0.038f, 0.94f);
        public static readonly Color PanelSoft = new Color(0.05f, 0.06f, 0.055f, 0.82f);
        public static readonly Color Backdrop = new Color(0.015f, 0.02f, 0.018f, 0.88f);
        public static readonly Color Line = new Color(0.95f, 0.66f, 0.24f, 0.55f);

        public static Color LayerColor(CallLayer layer)
        {
            switch (layer)
            {
                case CallLayer.Ambience: return new Color(0.55f, 0.82f, 0.72f);
                case CallLayer.Signals: return new Color(0.62f, 0.78f, 0.98f);
                case CallLayer.Interference: return new Color(0.9f, 0.48f, 0.42f);
                default: return Paper;
            }
        }

        public static Font Font { get; set; }
    }
}
