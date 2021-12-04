using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Text.RegularExpressions;
using static Snowberry.Editor.UI.UISelectionPanel;

namespace Snowberry.Editor.UI {

    class UIRoomSelectionPanel : UIElement {

        public Color BG = Calc.HexToColor("202929");

        public UIRoomSelectionPanel() {
            BG.A = 127;
            GrabsClick = true;
        }

        public override void Render(Vector2 position = default) {
            Draw.Rect(Bounds, BG);
            base.Render(position);
        }

        public void Refresh() {
            Clear();
            UIElement label;

            if (Editor.SelectedRoom == null) {
                if (!RoomTool.PendingRoom.HasValue) {
                    if (Editor.SelectedFillerIndex != -1) {
                        Add(label = new UILabel("Selected filler: " + Editor.SelectedFillerIndex) {
                            FG = Color.DarkKhaki,
                            Underline = true
                        });
                        label.Position = Vector2.UnitX * (Width / 2 - label.Width / 2);

                        AddBelow(new UIButton("delete", Fonts.Regular, 4, 4) {
                            FG = Color.Red,
                            HoveredFG = Color.Crimson,
                            PressedFG = Color.DarkRed,
                            OnPress = () => {
                                Editor.Instance.Map.Fillers.RemoveAt(Editor.SelectedFillerIndex);
                                Editor.SelectedFillerIndex = -1;
                                RoomTool.ScheduledRefresh = true;
                            }
                        }, new Vector2(4, 12));
                    } else {
                        Add(label = new UILabel(Dialog.Clean("SNOWBERRY_EDITOR_TOOL_ROOMTOOL_NONE")) {
                            FG = Color.DarkKhaki,
                            Underline = true
                        });
                        label.Position = Vector2.UnitX * (Width / 2 - label.Width / 2);
                    }
                    return;
                } else {
                    Add(label = new UILabel("Create room") {
                        FG = Color.DarkKhaki,
                        Underline = true
                    });
                    label.Position = Vector2.UnitX * (Width / 2 - label.Width / 2);

                    string newName = "";
                    UILabel newNameInvalid, newNameTaken;
                    UIButton newRoom;

                    AddBelow(new UIOption("name", new UITextField(Fonts.Regular, 90, newName) {
                        OnInputChange = text => newName = text
                    }), new Vector2(4, 3));

                    AddBelow(newRoom = new UIButton("create room", Fonts.Regular, 2, 2) {
                        Position = new Vector2(4, 4),
                    });
                    Add(newNameInvalid = new UILabel("invalid name") {
                        Position = new Vector2(newRoom.Position.X + newRoom.Width + 5, newRoom.Position.Y + 3),
                        FG = Color.Transparent
                    });
                    Add(newNameTaken = new UILabel("name already used") {
                        Position = new Vector2(newRoom.Position.X + newRoom.Width + 5, newRoom.Position.Y + 3),
                        FG = Color.Transparent
                    });
                    newRoom.OnPress = () => {
                        newNameInvalid.FG = newNameTaken.FG = Color.Transparent;
                        // validate room name
                        if (newName.Length <= 0 || Regex.Match(newName, "[0-9a-zA-Z\\-_ ]+").Length != newName.Length)
                            newNameInvalid.FG = Color.Red;
                        else if (Editor.Instance.Map.Rooms.Exists(it => it.Name.Equals(newName)))
                            newNameTaken.FG = Color.Red;
                        else {
                            // add room
                            var b = RoomTool.PendingRoom.Value;
                            var newRoom = new Room(newName, new Rectangle(b.X / 8, b.Y / 8, b.Width / 8, b.Height / 8));
                            Editor.Instance.Map.Rooms.Add(newRoom);
                            Editor.SelectedRoom = newRoom;
                            RoomTool.PendingRoom = null;
                            RoomTool.ScheduledRefresh = true;
                        }
                    };

                    AddBelow(new UIButton("create filler", Fonts.Regular, 2, 2) {
                        Position = new Vector2(4, 4),
                        OnPress = () => {
                            var b = RoomTool.PendingRoom.Value;
                            var newFiller = new Rectangle(b.X / 8, b.Y / 8, b.Width / 8, b.Height / 8);
                            Editor.Instance.Map.Fillers.Add(newFiller);
                            Editor.SelectedFillerIndex = Editor.Instance.Map.Fillers.Count - 1;
                            RoomTool.PendingRoom = null;
                            RoomTool.ScheduledRefresh = true;
                        }
                    });

                    return;
                }
            }

            int spacing = Fonts.Regular.LineHeight + 2;
            Room room = Editor.SelectedRoom;

            Add(label = new UILabel("Selected room:") {
                FG = Color.DarkKhaki,
                Underline = true
            });
            label.Position = Vector2.UnitX * (Width / 2 - label.Width / 2);

            string name = room.Name;
            UILabel nameInvalid, nameTaken;
            UIButton updateName;

            AddBelow(new UIOption("name", new UITextField(Fonts.Regular, 90, room.Name) {
                OnInputChange = text => name = text
            }), new Vector2(4, 3));

            AddBelow(updateName = new UIButton("update name", Fonts.Regular, 2, 2) {
                Position = new Vector2(4, 4),
            });
            Add(nameInvalid = new UILabel("invalid name") {
                Position = new Vector2(updateName.Position.X + updateName.Width + 5, updateName.Position.Y + 3),
                FG = Color.Transparent
            });
            Add(nameTaken = new UILabel("name already used") {
                Position = new Vector2(updateName.Position.X + updateName.Width + 5, updateName.Position.Y + 3),
                FG = Color.Transparent
            });
            updateName.OnPress = () => {
                nameInvalid.FG = nameTaken.FG = Color.Transparent;
                // validate room name
                if (name.Length <= 0 || Regex.Match(name, "[0-9a-zA-Z\\-_ ]+").Length != name.Length)
                    nameInvalid.FG = Color.Red;
                else if (room.Map.Rooms.Exists(it => it.Name.Equals(name)))
                    nameTaken.FG = Color.Red;
                else
                    room.Name = name;
            };


            AddBelow(new UILabel("music options :"), new Vector2(12, 12));

            AddBelow(new UIOption("music", new UITextField(Fonts.Regular, 90, room.Music) {
                OnInputChange = text => room.Music = text
            }), new Vector2(4, 3));

            AddBelow(new UIOption("alt music", new UITextField(Fonts.Regular, 90, room.AltMusic) {
                OnInputChange = text => room.AltMusic = text
            }), new Vector2(4, 3));

            AddBelow(new UIOption("ambience", new UITextField(Fonts.Regular, 90, room.Ambience) {
                OnInputChange = text => room.Ambience = text
            }), new Vector2(4, 3));

            AddBelow(new UIOption("music progress", new UIValueTextField<int>(Fonts.Regular, 30, room.MusicProgress.ToString()) {
                OnValidInputChange = prog => room.MusicProgress = prog
            }), new Vector2(4, 3));

            AddBelow(new UIOption("ambience progress", new UIValueTextField<int>(Fonts.Regular, 30, room.MusicProgress.ToString()) {
                OnValidInputChange = prog => room.AmbienceProgress = prog
            }), new Vector2(4, 3));

            AddBelow(new UILabel("music layers :"), new Vector2(12, 3));
            AddBelow(new UIOption("layer 1", new UICheckBox(-1, room.MusicLayers[0]) { OnPress = val => room.MusicLayers[0] = val }), new Vector2(4, 3));
            AddBelow(new UIOption("layer 2", new UICheckBox(-1, room.MusicLayers[1]) { OnPress = val => room.MusicLayers[1] = val }), new Vector2(4, 0));
            AddBelow(new UIOption("layer 3", new UICheckBox(-1, room.MusicLayers[2]) { OnPress = val => room.MusicLayers[2] = val }), new Vector2(4, 0));
            AddBelow(new UIOption("layer 4", new UICheckBox(-1, room.MusicLayers[3]) { OnPress = val => room.MusicLayers[3] = val }), new Vector2(4, 0));

            AddBelow(new UILabel("camera offset :"), new Vector2(12, 0));

            UIOption cameraOffsetX;

            AddBelow(cameraOffsetX = new UIOption("x", new UIValueTextField<float>(Fonts.Regular, 30, room.CameraOffset.X.ToString()) {
                OnValidInputChange = val => room.CameraOffset.X = val
            }), new Vector2(4, 3));

            Add(new UIOption("y", new UIValueTextField<float>(Fonts.Regular, 30, room.CameraOffset.Y.ToString()) {
                OnValidInputChange = val => room.CameraOffset.Y = val
            }) {
                Position = new Vector2(cameraOffsetX.Position.X + cameraOffsetX.Width + 15, cameraOffsetX.Position.Y),
            });

            AddBelow(new UILabel("other :"), new Vector2(12, 3));
            AddBelow(new UIOption("dark", new UICheckBox(-1, room.Dark) { OnPress = val => room.Dark = val }), new Vector2(4, 3));
            AddBelow(new UIOption("underwater", new UICheckBox(-1, room.Underwater) { OnPress = val => room.Underwater = val }), new Vector2(4, 0));
            AddBelow(new UIOption("space", new UICheckBox(-1, room.Space) { OnPress = val => room.Space = val }), new Vector2(4, 0));
            // TODO: value text field
            AddBelow(new UIOption("wind pattern", new UITextField(Fonts.Regular, 60, room.WindPattern.ToString()) {
                OnInputChange = text => room.WindPattern = Enum.TryParse(text, out WindController.Patterns pattern) ? pattern : room.WindPattern
            }), new Vector2(4, 3));

            AddBelow(new UIButton("delete", Fonts.Regular, 4, 4) {
                FG = Color.Red,
                HoveredFG = Color.Crimson,
                PressedFG = Color.DarkRed,
                OnPress = () => {
                    Editor.Instance.Map.Rooms.Remove(room);
                    Editor.SelectedRoom = null;
                    RoomTool.ScheduledRefresh = true;
                }
            }, new Vector2(4, 12));
        }

        public override void Update(Vector2 position = default) {
            base.Update(position);

        }
    }
}
