using Celeste;

using Microsoft.Xna.Framework;

using Monocle;

using Snowberry.Editor.UI;

using System.Collections.Generic;

namespace Snowberry.Editor.Tools {
	public class StylegroundsTool : Tool {

        public List<UIButton> StylegroundButtons = new();
        public Dictionary<UIButton, Styleground> Stylegrounds = new();
        public int SelectedStyleground = 0;

        private UIButton Add, Delete, MoveUp, MoveDown;

        public override UIElement CreatePanel() {
            StylegroundButtons.Clear();
            Stylegrounds.Clear();

            UIElement panel = new() {
                Width = 180,
                Background = Calc.HexToColor("202929") * (185 / 255f),
                GrabsClick = true,
                GrabsScroll = true
            };
            UIScrollPane stylegrounds = new() {
                TopPadding = 15,
                Background = null,
                Width = 180,
                Height = 205,
                Tag = "stylegrounds_list"
            };

			var fgLabel = new UILabel(Dialog.Clean("SNOWBERRY_EDITOR_UTIL_FOREGROUND")) {
				FG = Color.DarkKhaki,
				Underline = true
			};
			fgLabel.Position = new Vector2((stylegrounds.Width - fgLabel.Width) / 2, 10);
            stylegrounds.Add(fgLabel);

            int i = 0;
            foreach (var styleground in Editor.Instance.Map.FGStylegrounds) {
                int copy = i;
                UIButton element = new UIButton(styleground.Title(), Fonts.Regular, 4, 2) {
                    Position = new Vector2(10, i * 20 + 30),
                    OnPress = () => { SelectedStyleground = copy; }
                };
                stylegrounds.Add(element);
                StylegroundButtons.Add(element);
                Stylegrounds[element] = styleground;
                i++;
            }

			var bgLabel = new UILabel(Dialog.Clean("SNOWBERRY_EDITOR_UTIL_BACKGROUND")) {
				FG = Color.DarkKhaki,
				Underline = true
			};
			bgLabel.Position = new Vector2((stylegrounds.Width - bgLabel.Width) / 2, i * 20 + 40);
            stylegrounds.Add(bgLabel);

            foreach(var styleground in Editor.Instance.Map.BGStylegrounds) {
                int copy = i;
                UIButton element = new UIButton(styleground.Title(), Fonts.Regular, 4, 2) {
                    Position = new Vector2(10, i * 20 + 60),
                    OnPress = () => { SelectedStyleground = copy; }
                };
                stylegrounds.Add(element);
                StylegroundButtons.Add(element);
                Stylegrounds[element] = styleground;
                i++;
            }

			if(SelectedStyleground >= StylegroundButtons.Count) {
                SelectedStyleground = 0;
			}

            UIElement stylebg = UIElement.Regroup(stylegrounds);
            stylebg.Background = Color.White * 0.1f;
            stylebg.Position = Vector2.Zero;
            stylebg.Height += 10;
            stylegrounds.Position = new Vector2(0, 5);
            panel.Add(stylebg);

            UIElement optionsPanel = new UIElement();
            optionsPanel.AddRight(Add = new UIButton("+", Fonts.Regular, 4, 4) {
                // add new styleground
            }, new Vector2(4));

            optionsPanel.AddRight(Delete = new UIButton("-", Fonts.Regular, 4, 4) {
                OnPress = () => {
					UIButton selected = SelectedButton();
					if(selected != null) { // if there are no stylegrounds
						Bgs().Remove(Stylegrounds[selected]);
						Fgs().Remove(Stylegrounds[selected]);
						selected.RemoveSelf();
						RefreshPanel();
                    }
                }
			}, new Vector2(4));

            optionsPanel.AddRight(MoveUp = new UIButton("↑", Fonts.Regular, 4, 4) {
                OnPress = () => {
					MoveStyleground(-1);
				}
			}, new Vector2(4));

            optionsPanel.AddRight(MoveDown = new UIButton("↓", Fonts.Regular, 4, 4) {
                OnPress = () => {
                    MoveStyleground(1);
                }
            }, new Vector2(4));

            panel.AddBelow(optionsPanel);

            return panel;
        }

		private void MoveStyleground(int by) {
			UIButton selected = SelectedButton();
			if(selected != null) {
				var style = Stylegrounds[selected];
				if(IsFg(style)) {
					int indx = Fgs().IndexOf(style);
					if(indx + by < 0 || indx + by >= Fgs().Count)
						return;
					Fgs().Remove(style);
					Fgs().Insert(indx + by, style);
				} else {
					int indx = Bgs().IndexOf(style);
					if(indx + by < 0 || indx + by >= Bgs().Count)
						return;
					Bgs().Remove(style);
					Bgs().Insert(indx + by, style);
				}
				SelectedStyleground += by;
				RefreshPanel();
			}
		}

		private UIButton SelectedButton() {
			return StylegroundButtons.Count > SelectedStyleground ? StylegroundButtons[SelectedStyleground] : null;
		}

		public override string GetName() {
            return Dialog.Clean("SNOWBERRY_EDITOR_TOOL_STYLEGROUNDS");
        }

        public override void Update(bool canClick) {
            for (int i = 0; i < StylegroundButtons.Count; i++) {
                UIButton item = StylegroundButtons[i];
                if(i == SelectedStyleground) {
                    item.BG = item.HoveredBG = item.PressedBG = LeftSelectedBtnBg;
				} else if(Stylegrounds[item].IsVisible(Editor.SelectedRoom)) {
                    item.BG = item.HoveredBG = item.PressedBG = Color.Lerp(BothSelectedBtnBg, Color.Black, 0.5f);
                } else {
                    item.ResetBgColors();
                }
            }
            if(SelectedButton() != null) {
                var styleground = Stylegrounds[SelectedButton()];
                if(IsFg(styleground) ? Fgs().IndexOf(styleground) > 0 : Bgs().IndexOf(styleground) > 0) {
                    MoveUp.ResetFgColors();
                } else {
                    MoveUp.FG = MoveUp.HoveredFG = MoveUp.PressedFG = Color.DarkSlateGray;
                }
                if(IsFg(styleground) ? Fgs().IndexOf(styleground) < Fgs().Count - 1 : Bgs().IndexOf(styleground) < Bgs().Count - 1) {
                    MoveDown.ResetFgColors();
                } else {
                    MoveDown.FG = MoveDown.HoveredFG = MoveDown.PressedFG = Color.DarkSlateGray;
                }
            } else {
                Delete.FG = Delete.HoveredFG = Delete.PressedFG = Color.DarkSlateGray;
                MoveUp.FG = MoveUp.HoveredFG = MoveUp.PressedFG = Color.DarkSlateGray;
                MoveDown.FG = MoveDown.HoveredFG = MoveDown.PressedFG = Color.DarkSlateGray;
            }
        }

        private void RefreshPanel() {
            // just regenerate the panel and set the scroll again
            var tempScroll = Editor.Instance.ToolPanel.NestedChildWithTag<UIScrollPane>("stylegrounds_list").Scroll;
            Editor.Instance.SwitchTool(Tools.IndexOf(this));
            Editor.Instance.ToolPanel.NestedChildWithTag<UIScrollPane>("stylegrounds_list").Scroll = tempScroll;
        }

        private bool IsFg(Styleground s) {
            return Fgs().Contains(s);
        }

        private List<Styleground> Fgs() {
            return Editor.Instance.Map.FGStylegrounds;
        }

        private List<Styleground> Bgs() {
            return Editor.Instance.Map.BGStylegrounds;
        }
    }
}