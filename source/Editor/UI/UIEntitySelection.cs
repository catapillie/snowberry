using Microsoft.Xna.Framework;
using Snowberry.Editor.UI.Menus;
using System.Collections.Generic;

namespace Snowberry.Editor.UI {
    public class UIEntitySelection : UIScrollPane {
        public UIEntitySelection() {
            GrabsClick = true;
            TopPadding = 10;
        }

        public void Display(List<EntitySelection> selection) {
            if (selection != null) {
                Clear();
                int y = 0;
                foreach (EntitySelection s in selection) {
                    UIElement entry = AddEntry(s);
                    entry.Position.Y = y;
                    y += entry.Height + 8;
                }
            }
        }

        private UIElement AddEntry(EntitySelection s) {
            UIRibbon name = new UIRibbon(s.Entity.Name, 8, 8, true, false) {
                BG = Util.Colors.DarkGray,
                BGAccent = s.Entity.Info.Module.Color,
            };
            name.Position.X += Width - name.Width;

            UILabel id = new UILabel($"#{s.Entity.EntityID}") {
                FG = Util.Colors.White * 0.5f,
            };
            id.Position.X = name.Position.X - id.Width - 4;

            UIPluginOptionList options = new UIPluginOptionList(s.Entity) {
                Position = new Vector2(1, name.Height + 1),
            };

            UIElement entry = Regroup(id, name, options);
            Add(entry);
            return entry;
        }
    }
}
