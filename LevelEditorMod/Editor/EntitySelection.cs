using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LevelEditorMod.Editor {
    public class EntitySelection {
        public class Selection {
            public Rectangle Rect;
            public int Index;

            public Selection(Rectangle rect, int i) {
                Rect = rect;
                Index = i;
            }
        }

        public readonly Entity Entity;
        public readonly List<Selection> Selections;

        public EntitySelection(Entity entity, List<Selection> selection) {
            Entity = entity;
            Selections = selection;
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
                    Entity.Move(amount);
                else
                    Entity.MoveNode(s.Index, amount);
            }
        }

        public void SetPosition(Vector2 position, int i) {
            System.Console.WriteLine(i);
            foreach (Selection s in Selections) {
                if (s.Index == i) {
                    s.Rect.X = (int)position.X;
                    s.Rect.Y = (int)position.Y;
                    if (s.Index < 0)
                        Entity.SetPosition(position);
                    else
                        Entity.SetNode(s.Index, position);
                    break;
                }
            }
        }
    }
}
