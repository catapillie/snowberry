using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.UI {
    public class UIElement {
        private readonly List<UIElement> children = new List<UIElement>();

        public Vector2 Position;
        public int Width, Height;

        public virtual void Update(Vector2 position = default) {
            foreach (UIElement element in children)
                element.Update(position + element.Position);
        }

        public virtual void Render(Vector2 position = default) {
            foreach (UIElement element in children)
                element.Render(position + element.Position);
        }

        public void Add(UIElement element)
            => children.Add(element);
    }
}
