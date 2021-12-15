
using Microsoft.Xna.Framework;

using Monocle;

using Snowberry.Editor.Tools;
using Snowberry.Editor.UI;

using System.Collections.Generic;

namespace Snowberry.Editor {

	public abstract class Tool {

        public static IList<Tool> Tools = new List<Tool>() { new SelectionTool(), new DecalSelectionTool(), new TileBrushTool(), new RoomTool(), new PlacementTool(), new StylegroundsTool() };

        public static readonly Color LeftSelectedBtnBg = Calc.HexToColor("274292");
        public static readonly Color RightSelectedBtnBg = Calc.HexToColor("922727");
        public static readonly Color BothSelectedBtnBg = Calc.HexToColor("7d2792");

        public abstract string GetName();

        public abstract UIElement CreatePanel();

        public abstract void Update(bool canClick);

        public virtual void RenderScreenSpace() { }

        public virtual void RenderWorldSpace() { }
    }
}