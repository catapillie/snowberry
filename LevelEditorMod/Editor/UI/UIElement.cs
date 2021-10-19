using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.UI {
    public class UIElement {
        private readonly List<UIElement> children = new List<UIElement>();

        public Vector2 Position;

        public virtual void Update() {
            foreach (UIElement element in children)
                element.Update();
        }

        public virtual void Render(Vector2 position) {
            foreach (UIElement element in children)
                element.Render(Position + element.Position);
        }

        public void Add(UIElement element)
            => children.Add(element);
    }
}
