using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Snowberry.Editor {
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
            foreach (Selection s in Selections) {
                if (s.Index == i) {
                    s.Rect.X = (int)position.X;
                    s.Rect.Y = (int)position.Y;
                    break;
                }
            }

            if (i < 0)
                Entity.SetPosition(position);
            else
                Entity.SetNode(i, position);
        }

        public void SetWidth(int width) {
            foreach (Selection s in Selections) {
                if (s.Index == -1) {
                    s.Rect.Width = width;
                    break;
                }
            }

            Entity.SetWidth(width);
        }

        public void SetHeight(int height) {
            foreach (Selection s in Selections) {
                if (s.Index == -1) {
                    s.Rect.Height = height;
                    break;
                }
            }

            Entity.SetHeight(height);
        }

        public override bool Equals(object obj) {
            return obj != null && obj is EntitySelection s && s.Entity.Equals(Entity) && s.Selections.All(it => Selections.Any(x => x.Index == it.Index));
        }
    }
}