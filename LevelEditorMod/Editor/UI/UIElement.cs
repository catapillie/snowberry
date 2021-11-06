using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.UI {
    public class UIElement {
        protected readonly List<UIElement> children = new List<UIElement>();
        private readonly List<UIElement> toRemove = new List<UIElement>();
        private readonly List<UIElement> toAdd = new List<UIElement>();
        private bool canModify = true;
        protected UIElement Parent;

        public Vector2 Position;
        public int Width, Height;

        public bool GrabsScroll = false;
        public bool GrabsClick = false;

        public Rectangle Bounds => new Rectangle((int)(Position.X + (Parent?.Bounds.X ?? 0)), (int)(Position.Y + (Parent?.Bounds.Y ?? 0)), Width, Height);

        public virtual void Update(Vector2 position = default) {
            canModify = false;
            foreach (UIElement element in children)
                element.Update(position + element.Position);
            canModify = true;
            children.RemoveAll(e => e == null);
            toRemove.ForEach(a => children.Remove(a));
            toRemove.Clear();
            toAdd.ForEach(a => Add(a));
            toAdd.Clear();
        }

        public virtual void Render(Vector2 position = default) {
            foreach (UIElement element in children)
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
            if(canModify) {
                if(element.Parent == null) {
                    children.Add(element);
                    element.Parent = this;
                    element.Initialize();
                }
            } else
                toAdd.Add(element);
        }

        public void AddBelow(UIElement element) {
            Add(element);
            foreach (UIElement child in children)
                element.Position += new Vector2(0, child.Height);
        }

        public void AddRight(UIElement element) {
            Add(element);
            foreach(UIElement child in children)
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
			foreach(var item in elems) 
                Remove(item);
        }

        public bool CanScrollThrough() {
            return !GrabsScroll && !children.Exists(a => !a.CanScrollThrough() && a.Bounds.Contains((int)Editor.Mouse.Screen.X, (int)Editor.Mouse.Screen.Y));
        }

        public bool CanClickThrough() {
            return !GrabsClick && !children.Exists(a => !a.CanClickThrough() && a.Bounds.Contains((int)Editor.Mouse.Screen.X, (int)Editor.Mouse.Screen.Y));
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
