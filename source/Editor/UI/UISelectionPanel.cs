using Microsoft.Xna.Framework;
using Snowberry.Editor.UI.Menus;
using System.Collections.Generic;

namespace Snowberry.Editor.UI {
    public class UISelectionPanel : UIScrollPane {
        public UISelectionPanel() {
            GrabsClick = true;
            TopPadding = 10;
        }

        public void Display(List<EntitySelection> selection) {
            if (selection != null) {
                Clear();
                Vector2 offset = new Vector2(1, 1);
                foreach (EntitySelection s in selection) {
                    UIPluginOptionList entry = AddEntry(s, offset);
                    offset.Y += entry.Height;
                }
            }
        }

        private UIPluginOptionList AddEntry(EntitySelection s, Vector2 offset) {
            UIPluginOptionList entry;
            Add(entry = new UIPluginOptionList(s.Entity) {
                Position = offset
            });
            return entry;
        }
    }
}
