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
        public bool GrabsClick = true;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

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
    }
}
