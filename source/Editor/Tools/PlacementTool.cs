using Celeste;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Monocle;

using Snowberry.Editor.UI;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Snowberry.Editor.Tools {
	public class PlacementTool : Tool {

        Placements.Placement curLeftSelection = null, curRightSelection = null;
        Dictionary<Placements.Placement, UIButton> placementButtons = new Dictionary<Placements.Placement, UIButton>();
        Entity preview = null;
        Vector2? lastPress = null;
        Placements.Placement lastPlacement = null;

        public override UIElement CreatePanel() {
            placementButtons.Clear();
            var ret = new UIScrollPane();
            ret.Width = 180;
            ret.TopPadding = 10;
            foreach (var item in Placements.All.OrderBy(k => k.Name)) {
                UIButton b;
                ret.AddBelow(b = new UIButton(item.Name, Fonts.Regular, 4, 4) {
                    OnPress = () => curLeftSelection = curLeftSelection != item ? item : null,
                    OnRightPress = () => curRightSelection = curRightSelection != item ? item : null
                });
                placementButtons[item] = b;
            }
            return ret;
        }

        public override string GetName() {
            return Dialog.Clean("SNOWBERRY_EDITOR_TOOL_ENTITIES");
        }

        public override void Update(bool canClick) {
            Rectangle area;
            if (lastPress != null) {
                var mpos = (Editor.Mouse.World / 8).Round() * 8;
                int ax = (int)Math.Min(mpos.X, lastPress.Value.X);
                int ay = (int)Math.Min(mpos.Y, lastPress.Value.Y);
                int bx = (int)Math.Max(mpos.X, lastPress.Value.X);
                int by = (int)Math.Max(mpos.Y, lastPress.Value.Y);
                area = new Rectangle(ax, ay, bx - ax, by - ay);
            } else
                area = Rectangle.Empty;

            bool middlePan = Snowberry.Settings.MiddleClickPan;

            Placements.Placement selection = (middlePan && (MInput.Mouse.CheckRightButton || (middlePan && MInput.Mouse.ReleasedRightButton)) || !middlePan && MInput.Keyboard.Check(Keys.LeftAlt)) ? curRightSelection : curLeftSelection;
            if ((MInput.Mouse.ReleasedLeftButton || (middlePan && MInput.Mouse.ReleasedRightButton)) && canClick && selection != null && Editor.SelectedRoom != null && Editor.SelectedRoom.Bounds.Contains((int)Editor.Mouse.World.X / 8, (int)Editor.Mouse.World.Y / 8)) {
                Entity toAdd = selection.Build(Editor.SelectedRoom);
                UpdateEntity(toAdd, area);
                // TODO: find lowest unoccupied ID
                int highestID = 0;
                foreach (var item in Editor.Instance.Map.Rooms.SelectMany(k => k.AllEntities)) {
                    if (item.EntityID > highestID)
                        highestID = item.EntityID;
                }
                if (toAdd.Name != "player")
                    toAdd.EntityID = highestID + 1;
                Editor.SelectedRoom.AddEntity(toAdd);
            }

            RefreshPreview(lastPlacement != selection);
            lastPlacement = selection;
            if (preview != null)
                UpdateEntity(preview, area);

            if (MInput.Mouse.PressedLeftButton || (middlePan && MInput.Mouse.PressedRightButton))
                lastPress = Editor.Mouse.World;
            else if (!MInput.Mouse.CheckLeftButton && !(middlePan && MInput.Mouse.CheckRightButton))
                lastPress = null;

            foreach (var item in placementButtons) {
                var button = item.Value;
                if (item.Key.Equals(curLeftSelection) && item.Key.Equals(curRightSelection))
                    button.BG = button.PressedBG = button.HoveredBG = BothSelectedBtnBg;
                else if (item.Key.Equals(curLeftSelection))
                    button.BG = button.PressedBG = button.HoveredBG = LeftSelectedBtnBg;
                else if (item.Key.Equals(curRightSelection))
                    button.BG = button.PressedBG = button.HoveredBG = RightSelectedBtnBg;
                else {
                    button.BG = UIButton.DefaultBG;
                    button.HoveredBG = UIButton.DefaultHoveredBG;
                    button.PressedBG = UIButton.DefaultPressedBG;
                }
            }
        }

        private void RefreshPreview(bool changedPlacement) {
            bool middlePan = Snowberry.Settings.MiddleClickPan;

            Placements.Placement selection = (middlePan && (MInput.Mouse.CheckRightButton ||  MInput.Mouse.ReleasedRightButton) || !middlePan && MInput.Keyboard.Check(Keys.LeftAlt)) ? curRightSelection : curLeftSelection;
            if ((preview == null || changedPlacement) && selection != null) {
                preview = selection.Build(Editor.SelectedRoom);
            } else if (selection == null)
                preview = null;
        }

        private void UpdateEntity(Entity e, Rectangle area) {
            UpdateSize(e, area);
            var mpos = (MInput.Keyboard.Check(Keys.LeftControl) || MInput.Keyboard.Check(Keys.RightControl)) ? Editor.Mouse.World : (Editor.Mouse.World / 8).Round() * 8;
            if (lastPress != null)
                e.SetPosition(new Vector2(e.Width > 0 ? (area.Left / 8) * 8 : mpos.X, e.Height > 0 ? (area.Top / 8) * 8 : mpos.Y));
            else
                e.SetPosition(mpos);
            e.ResetNodes();
            while (e.Nodes.Length < e.MinNodes)
                e.AddNode((e.Nodes.Length > 0 ? e.Nodes.Last() : e.Position) + Vector2.UnitX * 24);
            e.ApplyDefaults();
            e.Initialize();
        }

        private void UpdateSize(Entity e, Rectangle area) {
            if (MInput.Mouse.CheckLeftButton || MInput.Mouse.CheckRightButton || MInput.Mouse.ReleasedLeftButton || MInput.Mouse.ReleasedRightButton) {
                if (e.MinWidth > -1)
                    e.SetWidth(Math.Max((int)Math.Ceiling(area.Width / 8f) * 8, e.MinWidth));
                if (e.MinHeight > -1)
                    e.SetHeight(Math.Max((int)Math.Ceiling(area.Height / 8f) * 8, e.MinHeight));
            } else {
                e.SetWidth(e.MinWidth != -1 ? e.MinWidth : 0);
                e.SetHeight(e.MinWidth != -1 ? e.MinWidth : 0);
            }
        }

        public override void RenderWorldSpace() {
            base.RenderWorldSpace();
            if (preview != null) {
                Calc.PushRandom(preview.GetHashCode());
                preview.Render();
                Calc.PopRandom();
            }
        }
    }
}