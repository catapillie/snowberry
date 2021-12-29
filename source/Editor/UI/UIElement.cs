using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowberry.Editor.UI {
    public class UIElement {
        private readonly List<UIElement> toRemove = new List<UIElement>();
        private readonly List<UIElement> toAdd = new List<UIElement>();
        private bool canModify = true;

        public UIElement Parent;
        public List<UIElement> Children = new List<UIElement>();
        public bool Visible = true;
        public bool RenderChildren = true;

        public Vector2 Position;
        public int Width, Height;
        public Color? Background;

        public bool GrabsScroll = false;
        public bool GrabsClick = false;

        public string Tag = "";

        public Rectangle Bounds => new Rectangle((int)(Position.X + (Parent?.Bounds.X ?? 0) + (Parent?.BoundsOffset().X ?? 0)), (int)(Position.Y + (Parent?.Bounds.Y ?? 0) + (Parent?.BoundsOffset().Y ?? 0)), Width, Height);

        public virtual void Update(Vector2 position = default) {
            canModify = false;
            // The last child is rendered last, on top of everything else, and should be the first to consume mouse clicks.
            for (int i = Children.Count - 1; i >= 0; i--) {
                UIElement element = Children[i];
                element.Update(position + element.Position);
            }

            canModify = true;
            Children.RemoveAll(e => e == null);
            toRemove.ForEach(a => Children.Remove(a));
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
                foreach (UIElement element in Children)
                    if (element.Visible)
                        element.Render(position + element.Position);
        }

        protected virtual void Initialize() { }

        protected virtual void OnDestroy() { }

        public virtual string Tooltip() => null;

        public virtual Vector2 BoundsOffset() => Vector2.Zero;

        public void Destroy() {
            foreach (UIElement element in Children)
                element?.Destroy();
            OnDestroy();
        }

        public void Add(UIElement element) {
            if (canModify) {
                if (element.Parent == null) {
                    Children.Add(element);
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
            foreach (var item in Children)
                if (low == null || item.Position.Y > low.Position.Y)
                    low = item;
            Add(element);
            element.Position += new Vector2(0, (low?.Position.Y + low?.Height) ?? 0);
        }

        public void AddRight(UIElement element, Vector2 offset) {
            AddRight(element);
            element.Position += offset;
        }

        public void AddRight(UIElement element) {
            UIElement right = null;
            foreach (var item in Children)
                if (right == null || item.Position.X > right.Position.X)
                    right = item;
            Add(element);
            element.Position += new Vector2((right?.Position.X + right?.Width) ?? 0, 0);
        }

        public void Clear() {
            foreach (UIElement element in Children)
                element?.Destroy();
            Children.Clear();
        }

        public void RemoveSelf() {
            Parent?.Remove(this);
        }

        public void Remove(UIElement elem) {
            toRemove.Add(elem);
        }

        public void RemoveAll(ICollection<UIElement> elems) {
            foreach (var item in elems)
                Remove(item);
        }

        public T HoveredChildProperty<T>(Func<UIElement, T> getter, T ignore = default) {
            if (!Equals(getter(this), ignore)) {
                return getter(this);
            }

            foreach (var child in Children) {
                if (child.Bounds.Contains((int)Editor.Mouse.Screen.X, (int)Editor.Mouse.Screen.Y)) {
                    var p = child.HoveredChildProperty(getter, ignore);
                    if (!Equals(p, ignore)) {
                        return p;
                    }
                }
            }

            return ignore;
        }

        public bool CanScrollThrough() {
            return !HoveredChildProperty(k => k.GrabsScroll, false);
        }

        public bool CanClickThrough() {
            return !HoveredChildProperty(k => k.GrabsScroll, false);
        }

        public string HoveredTooltip() {
            return HoveredChildProperty(k => k.Tooltip(), null);
        }

        private bool ConsumeClick() {
            if (!Editor.MouseClicked) {
                Editor.MouseClicked = true;
                return true;
            }

            return false;
        }

        protected bool ConsumeLeftClick(bool pressed = true, bool held = false, bool released = false) {
            if ((!pressed || MInput.Mouse.PressedLeftButton) && (!held || MInput.Mouse.CheckLeftButton) && (!released || MInput.Mouse.ReleasedLeftButton)) {
                return ConsumeClick();
            }

            return false;
        }

        protected bool ConsumeAltClick(bool pressed = true, bool held = false, bool released = false) {
            if (Snowberry.Settings.MiddleClickPan) {
                if ((!pressed || MInput.Mouse.PressedRightButton) && (!held || MInput.Mouse.CheckRightButton) && (!released || MInput.Mouse.ReleasedRightButton)) {
                    return ConsumeClick();
                }

                return false;
            } else {
                return (MInput.Keyboard.Check(Keys.LeftAlt) || MInput.Keyboard.Check(Keys.RightAlt)) && ConsumeLeftClick(pressed, held, released);
            }
        }

        public T ChildWithTag<T>(string tag) where T : UIElement {
            return (T)Children.Where(child => string.Equals(child.Tag, tag)).FirstOrDefault(child => child is T);
        }

        public T NestedChildWithTag<T>(string tag) where T : UIElement {
            var immediate = ChildWithTag<T>(tag);
            if (immediate != null)
                return immediate;

            foreach (var child in Children) {
                var ret = child.NestedChildWithTag<T>(tag);
                if (ret != null)
                    return ret;
            }

            return null;
        }

        public Vector2 GetBoundsPos() {
            return new Vector2(Bounds.X, Bounds.Y);
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