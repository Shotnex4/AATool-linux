using System;
using AATool.Configuration;
using AATool.Graphics;
using AATool.Platform.Linux;
using AATool.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AATool.UI.Screens
{
    public abstract class UIScreen : UIControl
    {
        public RenderTarget2D Target { get; protected set; }
        public Main Main { get; private set; }
        public GameWindow Window { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }

        public int FormWidth => this.Window.ClientBounds.Width;
        public int FormHeight => this.Window.ClientBounds.Height;
        public bool HasFocus => this.Main.IsActive;

        public abstract Color FrameBackColor();
        public abstract Color FrameBorderColor();

        public readonly Canvas Canvas = new ();

        protected bool Positioned;

        protected UIScreen(Main main, GameWindow window)
        {
            this.Main = main;
            this.Window = window;
            this.GraphicsDevice = main.GraphicsDevice;
            this.DrawMode = DrawMode.All;
        }

        public void Show() { }
        public void Hide() { }
        public void SetWindowTitle(string title) => this.Window.Title = title ?? string.Empty;
        public Point GetWindowPosition() => this.Window.Position;
        public void BringToFront() { }
        public void SetTopMost(bool topMost) { }
        public bool IsDisposed => false;
        public bool IsVisible { get; private set; } = true;
        public void SetVisible(bool visible) => this.IsVisible = visible;
        public void Close() { }
        public void SetIcon(string name) { }

        public abstract string GetCurrentView();
        public abstract void ReloadView();
        protected abstract void ConstrainWindow();

        public virtual void Click(UIControl sender) { }

        public virtual void Dispose()
        {
            this.Target?.Dispose();
        }

        public virtual void Prepare() => this.GraphicsDevice.SetRenderTarget(this.Target);
        public void Render() => this.DrawRecursive(this.Canvas);

        public virtual void Present()
        {
            this.GraphicsDevice.SetRenderTarget(null);
        }

        public void SetWindowSize(Point point)
        {
            this.ScaleTo(point);
        }

        public virtual bool ConfirmClose() => true;

        public override void MoveTo(Point point) => this.Window.Position = point;
        public override void MoveBy(Point point) => this.Window.Position += point;
        public override void ScaleTo(Point point)
        {
            Main.GraphicsManager.PreferredBackBufferWidth = point.X;
            Main.GraphicsManager.PreferredBackBufferHeight = point.Y;
            Main.GraphicsManager.ApplyChanges();
        }

        public override void ResizeThis(Rectangle parent)
        {
            this.Bounds = new Rectangle(this.Bounds.Location, parent.Size);
            this.Inner = new Rectangle(Point.Zero, parent.Size);
        }

        public override void DrawRecursive(Canvas canvas)
        {
            if (!SpriteSheet.Loading)
            {
                this.Canvas.BeginDraw(this);
                base.DrawRecursive(this.Canvas);
                if (Config.Main.LayoutDebugMode)
                    this.DrawDebugRecursive(this.Canvas);
                this.Canvas.EndDraw(this);
            }
        }

        public override void DrawDebugRecursive(Canvas canvas)
        {
            for (int i = 0; i < this.Children.Count; i++)
                this.Children[i].DrawDebugRecursive(canvas);
        }

        protected void PositionWindow(WindowSnap snap, int monitor, Point lastPosition)
        {
            if (snap is WindowSnap.Remember)
            {
                this.Window.Position = lastPosition;
                return;
            }

            if (snap is WindowSnap.Centered)
                return;

            var bounds = this.Window.ClientBounds;
            var desktop = LinuxScreen.GetPrimaryBounds();
            this.Window.Position = snap switch {
                WindowSnap.TopLeft => new Point(0, 0),
                WindowSnap.TopRight => new Point(Math.Max(0, desktop.width - bounds.Width), 0),
                WindowSnap.BottomLeft => new Point(0, Math.Max(0, desktop.height - bounds.Height)),
                WindowSnap.BottomRight => new Point(Math.Max(0, desktop.width - bounds.Width), Math.Max(0, desktop.height - bounds.Height)),
                _ => this.Window.Position,
            };
        }
    }
}
