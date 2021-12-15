using Celeste;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Monocle;

using Snowberry.Editor.UI;

using System;
using System.Collections.Generic;

namespace Snowberry.Editor.Tools {
	public class DecalSelectionTool : Tool {

        static List<Decal> SelectedDecals = new List<Decal>();
        static bool fg = false;
        static bool bg = true;
        static bool canSelect;

        public override string GetName() {
            return Dialog.Clean("SNOWBERRY_EDITOR_TOOL_DECALSELECT");
        }

        public override UIElement CreatePanel() {
            var panel = new UIElement() {
                Width = 80
            };
            panel.AddBelow(new UISelectionPanel.UIOption(Dialog.Clean("SNOWBERRY_EDITOR_UTIL_FOREGROUND"), new UICheckBox(-1, fg) {
                OnPress = val => fg = val
            }), Vector2.UnitY * 4);
            panel.AddBelow(new UISelectionPanel.UIOption(Dialog.Clean("SNOWBERRY_EDITOR_UTIL_BACKGROUND"), new UICheckBox(-1, bg) {
                OnPress = val => bg = val
            }), Vector2.UnitY * 4);
            return panel;
        }

        public override void Update(bool canClick) {
            var editor = Editor.Instance;

            if (MInput.Mouse.CheckLeftButton && canClick) {
                if (MInput.Mouse.PressedLeftButton) {
                    Point mouse = new Point((int)Editor.Mouse.World.X, (int)Editor.Mouse.World.Y);

                    canSelect = true;
                    if (SelectedDecals != null) {
                        foreach (var s in SelectedDecals) {
                            if (s.Bounds.Contains(mouse)) {
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

                    SelectedDecals = new List<Decal>();
                    var selectFrom = new List<Decal>();
                    if (fg) selectFrom.AddRange(Editor.SelectedRoom.FgDecals);
                    if (bg) selectFrom.AddRange(Editor.SelectedRoom.BgDecals);
                    foreach (var item in selectFrom) {
                        if (item.Bounds.Intersects(Editor.Selection.Value))
                            SelectedDecals.Add(item);
                    }
                } else if (SelectedDecals != null) {
                    Vector2 worldSnapped = (Editor.Mouse.World / 8).Floor() * 8;
                    Vector2 worldLastSnapped = (Editor.Mouse.WorldLast / 8).Floor() * 8;
                    Vector2 move = worldSnapped - worldLastSnapped;
                    foreach (Decal s in SelectedDecals)
                        s.Position += move;
                }
            } else
                Editor.Selection = null;

            if (MInput.Keyboard.Check(Keys.Delete)) {
                foreach (var item in SelectedDecals) {
                    item.Room.FgDecals.Remove(item);
                    item.Room.BgDecals.Remove(item);
                }
                SelectedDecals.Clear();
            }
        }

        public override void RenderWorldSpace() {
            base.RenderWorldSpace();
            foreach (var item in SelectedDecals)
                Draw.Rect(item.Bounds, Color.Blue * 0.25f);
        }
    }
}