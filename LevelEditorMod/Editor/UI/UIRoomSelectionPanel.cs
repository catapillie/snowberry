using Microsoft.Xna.Framework;
using Monocle;
using static LevelEditorMod.Editor.UI.UISelectionPanel;

namespace LevelEditorMod.Editor.UI {

	class UIRoomSelectionPanel : UIElement {

		public Color BG = Calc.HexToColor("202929");

        private bool modified = false;

		public UIRoomSelectionPanel() {
			BG.A = 127;
			GrabsClick = true;
		}

		public override void Render(Vector2 position = default) {
			base.Render(position);
			Draw.Rect(Bounds, BG);
		}

		public void Refresh() {
            Clear();
            modified = false;
            UIElement label;

            if(Editor.SelectedRoom == null) {
				Add(label = new UILabel("No room is selected") {
                    FG = Color.DarkKhaki,
                    Underline = true
                });
                label.Position = Vector2.UnitX * (Width / 2 - label.Width / 2);
                return;
            }

			int spacing = Fonts.Regular.LineHeight + 2;
            Room room = Editor.SelectedRoom;
            
            Add(label = new UILabel("Selected room:") {
                FG = Color.DarkKhaki,
                Underline = true
            });
            label.Position = Vector2.UnitX * (Width / 2 - label.Width / 2);

            AddBelow(new UIOption("music", new UITextField(Fonts.Regular, 90, room.Music)) {
                Position = new Vector2(4, 3),
            });
            AddBelow(new UIOption("alt music", new UITextField(Fonts.Regular, 90, room.AltMusic)) {
                Position = new Vector2(4, 3),
            });
            AddBelow(new UIOption("ambience", new UITextField(Fonts.Regular, 90, room.Ambience)) {
                Position = new Vector2(4, 3),
            });
            AddBelow(new UILabel("music layers :") {
                Position = new Vector2(12, 3),
            });
            AddBelow(new UIOption("layer 1", new UICheckBox(-1, room.MusicLayers[0])) {
                Position = new Vector2(4, 3),
            });
            AddBelow(new UIOption("layer 2", new UICheckBox(-1, room.MusicLayers[1])) {
                Position = new Vector2(4, 0),
            });
            AddBelow(new UIOption("layer 3", new UICheckBox(-1, room.MusicLayers[2])) {
                Position = new Vector2(4, 0),
            });
            AddBelow(new UIOption("layer 4", new UICheckBox(-1, room.MusicLayers[3])) {
                Position = new Vector2(4, 0),
            });
            AddBelow(new UIOption("music progress", new UIValueTextField<int>(Fonts.Regular, 30, room.MusicProgress.ToString())) {
                Position = new Vector2(4, 3),
            });
            AddBelow(new UIOption("ambience progress", new UIValueTextField<int>(Fonts.Regular, 30, room.MusicProgress.ToString())) {
                Position = new Vector2(4, 3),
            });
            AddBelow(new UILabel("camera offset :") {
                Position = new Vector2(12, 3),
            });
            UIOption cameraOffsetX;
            AddBelow(cameraOffsetX = new UIOption("x", new UIValueTextField<float>(Fonts.Regular, 30, room.CameraOffset.X.ToString())) {
                Position = new Vector2(4, 3),
            });
            Add(new UIOption("y", new UIValueTextField<float>(Fonts.Regular, 30, room.CameraOffset.Y.ToString())) {
                Position = new Vector2(cameraOffsetX.Position.X + cameraOffsetX.Width + 15, cameraOffsetX.Position.Y),
            });

            UIButton update, cancel;
            AddBelow(update = new UIButton("Update", Fonts.Regular, 4, 4) {
                Position = new Vector2(8, 4),
                OnPress = () => {
                    // validate room settings
                    // update room
                }
            });
            Add(cancel = new UIButton("Cancel", Fonts.Regular, 4, 4) {
                Position = new Vector2(update.Width + 24, update.Position.Y),
                OnPress = () => RoomTool.ScheduledRefresh = true
            });
        }

		public override void Update(Vector2 position = default) {
			base.Update(position);

		}
	}
}
