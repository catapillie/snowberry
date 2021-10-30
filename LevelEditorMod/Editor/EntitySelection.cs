using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LevelEditorMod.Editor {
    internal class EntitySelection {
        public class Selection {
            public Rectangle Rect;
            public int Index;

            public Selection(Rectangle rect, int i) {
                Rect = rect;
                Index = i;
            }
        }

        private readonly Entity entity;
        public readonly List<Selection> Selections;

        public EntitySelection(Entity entity, List<Selection> selection) {
            this.entity = entity;
            this.Selections = selection;
        }

        public bool Contains(Point p) {
            foreach (Selection s in Selections)
                if (s.Rect.Contains(p))
                    return true;
            return false;
        }

        public void Move(Vector2 amount) {
            foreach (Selection s in Selections) {
                s.Rect.X += (int)amount.X;
                s.Rect.Y += (int)amount.Y;
                if (s.Index < 0)
                    entity.Move(amount);
                else
                    entity.MoveNode(s.Index, amount);
            }
        }
    }
}
