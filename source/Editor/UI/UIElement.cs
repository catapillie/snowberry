using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Monocle;
using System.Collections.Generic;

namespace Snowberry.Editor.UI {
    public class UIElement {
        protected readonly List<UIElement> children = new List<UIElement>();
        private readonly List<UIElement> toRemove = new List<UIElement>();
        private readonly List<UIElement> toAdd = new List<UIElement>();
        private bool canModify = true;

        public UIElement Parent;
        public bool Visible = true;
        public bool RenderChildren = true;

        public Vector2 Position;
        public int Width, Height;
        public Color? Background;

        public bool GrabsScroll = false;
        public bool GrabsClick = false;

        public Rectangle Bounds => new Rectangle((int)(Position.X + (Parent?.Bounds.X ?? 0)), (int)(Position.Y + (Parent?.Bounds.Y ?? 0)), Width, Height);

        public virtual void Update(Vector2 position = default) {
            canModify = false;
            // The last child is rendered last, on top of everything else, and should be the first to consume mouse clicks.
			for(int i = children.Count - 1; i >= 0; i--) {
				UIElement element = children[i];
				element.Update(position + element.Position);
			}

			canModify = true;
            children.RemoveAll(e => e == null);
            toRemove.ForEach(a => children.Remove(a));
            toRemove.Clear();
            toAdd.ForEach(a => Add(a));
            toAdd.Clear();
        }

        public virtual void Render(Vector2 position = default) {
            if (Background.HasValue) {
                Rectangle rect = new Rectangle((int)position.X, (int)position.Y, Width, Height);
                Draw.Rect(rect, Background.Value);
            }
            if (RenderChildren)
                foreach (UIElement element in children)
                    if (element.Visible)
                        element.Render(position + element.Position);
        }

        protected virtual void Initialize() { }

        protected virtual void OnDestroy() { }

        public void Destroy() {
            foreach (UIElement element in children)
                element?.Destroy();
            OnDestroy();
        }

        public void Add(UIElement element) {
            if (canModify) {
                if (element.Parent == null) {
                    children.Add(element);
                    element.Parent = this;
                    element.Initialize();
                }
            } else
                toAdd.Add(element);
        }

        public void AddBelow(UIElement element, Vector2 offset) {
            AddBelow(element);
            element.Position += offset;
        }

        public void AddBelow(UIElement element) {
            UIElement low = null;
            foreach (var item in children)
                if (low == null || item.Position.Y > low.Position.Y) low = item;
            Add(element);
            element.Position += new Vector2(0, (low?.Position.Y + low?.Height) ?? 0);
        }

        public void AddRight(UIElement element) {
            Add(element);
            foreach (UIElement child in children)
                element.Position += new Vector2(child.Width, 0);
        }

        public void Clear() {
            foreach (UIElement element in children)
                element?.Destroy();
            children.Clear();
        }

        public void Remove(UIElement elem) {
            toRemove.Add(elem);
        }

        public void RemoveAll(ICollection<UIElement> elems) {
            foreach (var item in elems)
                Remove(item);
        }

        public bool CanScrollThrough() {
            return !GrabsScroll && !children.Exists(a => !a.CanScrollThrough() && a.Bounds.Contains((int)Editor.Mouse.Screen.X, (int)Editor.Mouse.Screen.Y));
        }

        public bool CanClickThrough() {
            return !GrabsClick && !children.Exists(a => !a.CanClickThrough() && a.Bounds.Contains((int)Editor.Mouse.Screen.X, (int)Editor.Mouse.Screen.Y));
        }

        private bool ConsumeClick() {
            if(!Editor.MouseClicked) {
                Editor.MouseClicked = true;
                return true;
            }
            return false;
        }

        protected bool ConsumeLeftClick(bool pressed = true, bool held = false, bool released = false) {
			if((!pressed || MInput.Mouse.PressedLeftButton) && (!held || MInput.Mouse.CheckLeftButton) && (!released || MInput.Mouse.ReleasedLeftButton)) {
                return ConsumeClick();
            }
            return false;
        }

        protected bool ConsumeAltClick(bool pressed = true, bool held = false, bool released = false) {
            if(Snowberry.Settings.MiddleClickPan) {
                if((!pressed || MInput.Mouse.PressedRightButton) && (!held || MInput.Mouse.CheckRightButton) && (!released || MInput.Mouse.ReleasedRightButton)) {
                    return ConsumeClick();
				}
                return false;
            } else {
                return (MInput.Keyboard.Check(Keys.LeftAlt) || MInput.Keyboard.Check(Keys.RightAlt)) && ConsumeLeftClick(pressed, held, released);
            }
        }

        public static UIElement Regroup(params UIElement[] elems) {
            UIElement group = new UIElement();
            RegroupIn(group, elems);
            return group;
        }

        public static void RegroupIn<T>(T group, params UIElement[] elems) where T : UIElement {
            if (elems != null && elems.Length > 0) {
                int ax = int.MaxValue, ay = int.MaxValue;
                int bx = int.MinValue, by = int.MinValue;

                foreach (UIElement el in elems) {
                    group.Add(el);
                    Rectangle bounds = el.Bounds;
                    if (bounds.Left < ax) ax = bounds.X;
                    if (bounds.Top < ay) ay = bounds.Y;
                    if (bounds.Right > bx) bx = bounds.Right;
                    if (bounds.Bottom > by) by = bounds.Bottom;
                }

                group.Position = new Vector2(ax, ay);
                group.Width = bx - ax;
                group.Height = by - ay;

                foreach (UIElement el in elems)
                    el.Position -= group.Position;
            }
        }
    }
}
