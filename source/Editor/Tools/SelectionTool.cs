using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using Snowberry.Editor.UI;
using System;

namespace Snowberry.Editor.Tools {
    public class SelectionTool : Tool {
        static bool canSelect;

        public override string GetName() {
            return Dialog.Clean("SNOWBERRY_EDITOR_TOOL_ENTITYSELECT");
        }

        public override UIElement CreatePanel() {
            return new UIEntitySelection() {
                Width = 160,
            };
        }

        public override void Update(bool canClick) {
            var editor = Editor.Instance;

            if (MInput.Mouse.CheckLeftButton && canClick) {
                if (MInput.Mouse.PressedLeftButton) {
                    Point mouse = new Point((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y);

                    canSelect = true;
                    if (Editor.SelectedEntities != null) {
                        foreach (EntitySelection s in Editor.SelectedEntities) {
                            if (s.Contains(mouse)) {
                                canSelect = false;
                                break;
                            }
                        }
                    }
                }

                if (canSelect && Editor.SelectedRoom != null) {
                    int ax = (int)Math.Min(Editor.Mouse.World.X, editor.worldClick.X);
                    int ay = (int)Math.Min(Editor.Mouse.World.Y, editor.worldClick.Y);
                    int bx = (int)Math.Max(Editor.Mouse.World.X, editor.worldClick.X);
                    int by = (int)Math.Max(Editor.Mouse.World.Y, editor.worldClick.Y);
                    Editor.Selection = new Rectangle(ax, ay, bx - ax, by - ay);

                    Editor.SelectedEntities = Editor.SelectedRoom.GetSelectedEntities(Editor.Selection.Value);
                } else if (Editor.SelectedEntities != null) {
                    bool noSnap = (MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl));
                    Vector2 worldSnapped = noSnap ? Editor.Mouse.World : (Editor.Mouse.World / 8).Round() * 8;
                    Vector2 worldLastSnapped = noSnap ? Editor.Mouse.WorldLast : (Editor.Mouse.WorldLast / 8).Round() * 8;
                    Vector2 move = worldSnapped - worldLastSnapped;
                    foreach (EntitySelection s in Editor.SelectedEntities)
                        s.Move(move);
                }
            } else
                Editor.Selection = null;

            bool entitiesRemoved = false;
            if (MInput.Keyboard.Check(Keys.Delete)) {
                foreach (var item in Editor.SelectedEntities) {
                    entitiesRemoved = true;
                    item.Entity.Room.RemoveEntity(item.Entity);
                }

                Editor.SelectedEntities.Clear();
            }

            if ((MInput.Mouse.ReleasedLeftButton && canClick) || entitiesRemoved) {
                if (canSelect && editor.ToolPanel is UIEntitySelection selectionPanel)
                    selectionPanel.Display(Editor.SelectedEntities);
            }
        }

        public override void RenderWorldSpace() {
            base.RenderWorldSpace();
            if (Editor.SelectedRoom != null)
                foreach (var item in Editor.SelectedRoom.GetSelectedEntities(new Rectangle((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y, 0, 0)))
                    if (Editor.SelectedEntities == null || !Editor.SelectedEntities.Contains(item))
                        foreach (var s in item.Selections)
                            Draw.Rect(s.Rect, Color.Blue * 0.15f);
        }
    }
}