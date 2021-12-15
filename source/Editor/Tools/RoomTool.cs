using Celeste;

using Microsoft.Xna.Framework;

using Monocle;

using Snowberry.Editor.UI;

using System;

namespace Snowberry.Editor.Tools {
	public class RoomTool : Tool {

        private Room lastSelected = null;
        private int lastFillerSelected = -1;
        public static bool ScheduledRefresh = false;

        private Vector2? lastRoomOffset = null;
        private static bool resizingX, resizingY;
        private static int newWidth, newHeight;
        private static Rectangle oldRoomBounds;
        private static bool justSwitched = false;

        public static Rectangle? PendingRoom = null;

        public override UIElement CreatePanel() {
            // room selection panel containing room metadata
            var ret = new UIRoomSelectionPanel() {
                Width = 160
            };
            ret.Refresh();
            return ret;
        }

        public override string GetName() {
            return Dialog.Clean("SNOWBERRY_EDITOR_TOOL_ROOMTOOL");
        }

        public override void Update(bool canClick) {
            // refresh the display
            if (lastSelected != Editor.SelectedRoom || lastFillerSelected != Editor.SelectedFillerIndex || ScheduledRefresh) {
                justSwitched = true;
                ScheduledRefresh = false;
                lastSelected = Editor.SelectedRoom;
                lastFillerSelected = Editor.SelectedFillerIndex;
                if (Editor.Instance.ToolPanel is UIRoomSelectionPanel selectionPanel)
                    selectionPanel.Refresh();
                if (Editor.SelectedRoom != null) {
                    lastRoomOffset = Editor.SelectedRoom.Position - (Editor.Mouse.World / 8);
                    oldRoomBounds = Editor.SelectedRoom.Bounds;
                }
            }

            // move, resize, add rooms
            if (canClick && Editor.SelectedRoom != null && !justSwitched) {
                if (MInput.Mouse.PressedLeftButton) {
                    lastRoomOffset = Editor.SelectedRoom.Position - (Editor.Mouse.World / 8);
                    // check if the mouse is 8 pixels from the room's borders
                    resizingX = resizingY = false;
                    if (Math.Abs(Editor.Mouse.World.X / 8f - (Editor.SelectedRoom.Position.X + Editor.SelectedRoom.Width)) < 1)
                        resizingX = true;
                    if (Math.Abs(Editor.Mouse.World.Y / 8f - (Editor.SelectedRoom.Position.Y + Editor.SelectedRoom.Height)) < 1)
                        resizingY = true;
                    oldRoomBounds = Editor.SelectedRoom.Bounds;
                } else if (MInput.Mouse.CheckLeftButton) {
                    Vector2 world = Editor.Mouse.World / 8;
                    var offset = lastRoomOffset ?? Vector2.Zero;
                    var newX = (int)(world + offset).X;
                    var newY = (int)(world + offset).Y;
                    var diff = new Vector2(newX - Editor.SelectedRoom.Bounds.X, newY - Editor.SelectedRoom.Bounds.Y);
                    if (!resizingX && !resizingY) {
                        Editor.SelectedRoom.Bounds.X = (int)(world + offset).X;
                        Editor.SelectedRoom.Bounds.Y = (int)(world + offset).Y;
                        foreach (var e in Editor.SelectedRoom.AllEntities) {
                            e.Move(diff * 8);
                            for (int i = 0; i < e.Nodes.Length; i++) {
                                e.MoveNode(i, diff * 8);
                            }
                        }
                    } else {
                        if (resizingX) {
                            newWidth = (int)Math.Ceiling(world.X - Editor.SelectedRoom.Bounds.X);
                            Editor.SelectedRoom.Bounds.Width = Math.Max(newWidth, 1);
                        }
                        if (resizingY) {
                            newHeight = (int)Math.Ceiling(world.Y - Editor.SelectedRoom.Bounds.Y);
                            Editor.SelectedRoom.Bounds.Height = Math.Max(newHeight, 1);
                        }
                    }
                } else {
                    lastRoomOffset = null;
                    if (!oldRoomBounds.Equals(Editor.SelectedRoom.Bounds)) {
                        oldRoomBounds = Editor.SelectedRoom.Bounds;
                        Editor.SelectedRoom.UpdateBounds();
                    }
                    resizingX = resizingY = false;
                    newWidth = newHeight = 0;
                }
            }

            if (MInput.Mouse.ReleasedLeftButton) {
                justSwitched = false;
            }

            // room creation
            if (canClick) {
                if (Editor.SelectedRoom == null) {
                    if (MInput.Mouse.CheckLeftButton) {
                        var lastPress = (Editor.Instance.worldClick / 8).Ceiling() * 8;
                        var mpos = (Editor.Mouse.World / 8).Ceiling() * 8;
                        int ax = (int)Math.Min(mpos.X, lastPress.X);
                        int ay = (int)Math.Min(mpos.Y, lastPress.Y);
                        int bx = (int)Math.Max(mpos.X, lastPress.X);
                        int by = (int)Math.Max(mpos.Y, lastPress.Y);
                        var newRoom = new Rectangle(ax, ay, bx - ax, by - ay);
                        if (newRoom.Width > 0 || newRoom.Height > 0) {
                            newRoom.Width = Math.Max(newRoom.Width, 8);
                            newRoom.Height = Math.Max(newRoom.Height, 8);
                            if (!PendingRoom.HasValue)
                                ScheduledRefresh = true;
                            PendingRoom = newRoom;
                        } else {
                            ScheduledRefresh = true;
                            PendingRoom = null;
                        }
                    }
                } else {
                    if (PendingRoom.HasValue) {
                        PendingRoom = null;
                        ScheduledRefresh = true;
                    }
                }
            }
        }

        public override void RenderWorldSpace() {
            base.RenderWorldSpace();
            if (PendingRoom.HasValue) {
                var prog = (float)Math.Abs(Math.Sin(Engine.Scene.TimeActive * 3));
                Draw.Rect(PendingRoom.Value, Color.Lerp(Color.White, Color.Cyan, prog) * 0.6f);
                Draw.HollowRect(PendingRoom.Value.X, PendingRoom.Value.Y, 40 * 8, 23 * 8, Color.Lerp(Color.Orange, Color.White, prog) * 0.6f);
            }
        }
    }
}