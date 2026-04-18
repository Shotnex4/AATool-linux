using AATool.Configuration;
using Microsoft.Xna.Framework;

namespace AATool.UI.Screens
{
    public sealed class UIOverlayScreen : UIScreen
    {
        public bool FastForwarding { get; set; }

        public UIOverlayScreen(Main main) : base(main, main.Window) { }

        public override Color FrameBackColor() => Config.Overlay.FrameStyle != "Custom Theme"
            ? Config.Main.BackColor
            : Config.Overlay.CustomBackColor;

        public override Color FrameBorderColor() => Config.Overlay.FrameStyle != "Custom Theme"
            ? Config.Main.BorderColor
            : Config.Overlay.CustomBorderColor;

        public override string GetCurrentView() => string.Empty;
        public override void ReloadView() { }
        protected override void ConstrainWindow() { }
        public void ShowObjectiveTray() { }
        public void HideObjectiveTray() { }
        public void PinnedObjectivesSaved() { }
    }
}
