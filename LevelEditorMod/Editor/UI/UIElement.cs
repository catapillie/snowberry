using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.UI {
    public class UIElement {
        private readonly List<UIElement> children = new List<UIElement>();
        protected UIElement Parent;

        public Vector2 Position;
        public int Width, Height;

        public virtual void Update(Vector2 position = default) {
            foreach (UIElement element in children)
                element.Update(position + element.Position);
            children.RemoveAll(e => e == null);
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
            if (element.Parent == null) {
                children.Add(element);
                element.Parent = this;
                element.Initialize();
            }
        }

        public void AddBelow(UIElement element) {
            Add(element);
            foreach(UIElement child in children)
                element.Position += new Vector2(0, child.Height);
        }
    }
}
