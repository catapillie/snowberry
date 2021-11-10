using Monocle;
using Microsoft.Xna.Framework;
using Celeste;
using System;
using System.Linq;

namespace LevelEditorMod.Editor.UI {
    public class UIMainMenu : UIElement {
        private class UIMainButtons : UIElement {
            public Color Color = Util.Colors.White;

            public override void Render(Vector2 position = default) {
                UIMainMenu parent = (UIMainMenu)Parent;
                int right = (int)position.X + Width + 16;
                float ease = Ease.CubeInOut(parent.stateLerp[2]);

                Draw.Rect(0, 0, right, parent.Height, Util.Colors.DarkGray);
                Draw.Rect(right, 8 + parent.Height / 2 * (1 - ease), 1, ease * (parent.Height - 16), Color * ease);

                base.Render(position);
            }
        }

        private class UILevelSelector : UIElement {
            private class UILevelRibbon : UIRibbon {
                private readonly UILevelSelector selector;

                private readonly string raw;
                public readonly string Name;

                private readonly int w;
                public readonly int W;

                private float lerp, listLerp = 1f;
                private readonly int n;

                private readonly ModeProperties[] modes;

                public UILevelRibbon(UILevelSelector selector, AreaData area, int n)
                    : base($" • {Dialog.Clean(area.Name)}", 24) {
                    this.selector = selector;
                    this.n = n;
                    modes = area.Mode;

                    raw = Dialog.Has(area.Name) ? $"» {area.Name}" : "...";
                    Name = area.Name;

                    w = (int)Fonts.Regular.Measure(raw).X;
                    W = Width + w + 5;
                }

                public override void Update(Vector2 position = default) {
                    base.Update(position);
                    int mouseX = (int)Editor.Mouse.Screen.X;
                    int mouseY = (int)Editor.Mouse.Screen.Y;
                    bool hover = Visible && new Rectangle((int)position.X + 16, (int)position.Y - 2, Width + w, Height + 4).Contains(mouseX, mouseY);
                    lerp = Calc.Approach(lerp, hover.Bit(), Engine.DeltaTime * 5f);
                    listLerp = Calc.Approach(listLerp, (selector.anim < n).Bit(), Engine.DeltaTime * 4f);

                    if (hover && MInput.Mouse.PressedLeftButton) {
                        selector.mainButtons.Color = BG;
                        Console.WriteLine("-----");
                        for (int i = 0; i < modes.Length; i++) {
                            Console.WriteLine(((AreaMode)i).ToString() + " : " + modes[i]?.MapData.Filename ?? "-");
                        }
                    }
                }

                public override void Render(Vector2 position = default) {
                    float ease = Ease.CubeOut(lerp);
                    float listEase = Ease.ExpoIn(listLerp);
                    position.X += (int)(ease * 16 - Width * listEase);

                    float sin = Settings.Instance.DisableFlashes || lerp == 0f ? 0f : ((float)Math.Sin(Engine.Scene.TimeActive * 12f) * 0.1f);
                    Fonts.Regular.Draw(raw, position + Vector2.UnitX * (Width + 5), Vector2.One, Color.Lerp(Util.Colors.CloudGray, Util.Colors.White, ease * (0.9f + sin)) * (1 - listEase));

                    base.Render(position);
                }
            }

            private float anim;
            private int lvlCount;

            private UISearchBar<UILevelRibbon> searchBar;
            private UILevelRibbon[] levels;

            private readonly UIMainButtons mainButtons;

            public UILevelSelector(UIMainButtons uIMainButtons) {
                mainButtons = uIMainButtons;
            }

            public void Reload() {
                Clear();

                anim = 0f;

                UIScrollPane levelScrollPane = new UIScrollPane() {
                    Height = Parent.Height - 30,
                    Position = new Vector2(-16, 22),
                    BG = Color.Transparent,
                    ShowScrollBar = false,
                };

                levels = new UILevelRibbon[lvlCount = AreaData.Areas.Count];
                int y = 0;
                Width = 0;
                for (int i = 0; i < lvlCount; i++) {
                    AreaData area = AreaData.Areas[i];
                    UILevelRibbon lvl;
                    levelScrollPane.Add(lvl = new UILevelRibbon(this, area, i) {
                        Position = new Vector2(-8, y),
                        FG = area.TitleTextColor,
                        BG = area.TitleBaseColor,
                        BGAccent = area.TitleAccentColor,
                    });

                    if (lvl.W > Width)
                        Width = lvl.W;
                    y += 15;

                    levels[i] = lvl;
                }
                Add(levelScrollPane);
                levelScrollPane.Width = Width;

                static bool lvlMatcher(UILevelRibbon entry, string term)
                    => entry.Text.ToLower().Contains(term.ToLower());

                static bool lvlMatcherByMod(UILevelRibbon entry, string term)
                    => entry.Name.ToLower().Contains(term.ToLower());

                string infonone = Dialog.Clean("LEVELEDITORMOD_MAINMENU_LOADSEARCHBAR_NONE");
                string infoone = Dialog.Clean("LEVELEDITORMOD_MAINMENU_LOADSEARCHBAR_ONE");
                string infomore = Dialog.Clean("LEVELEDITORMOD_MAINMENU_LOADSEARCHBAR_MORE");
                Add(searchBar = new UISearchBar<UILevelRibbon>(Width / 2, lvlMatcher) {
                    Position = Vector2.UnitY * 8,
                    Entries = levels,
                    OnInputChange = input => {
                        if (levels != null && searchBar != null) {
                            int y = 0;
                            foreach (UILevelRibbon level in levels) {
                                level.Visible = searchBar.Found == null || searchBar.Found.Contains(level);
                                if (level.Visible) {
                                    level.Position.Y = y;
                                    y += 15;
                                } else {
                                    level.Position.Y = 0;
                                }
                            }
                        }
                    },
                    InfoText = Dialog.Clean("LEVELEDITORMOD_MAINMENU_LOADSEARCH"),
                    SearchInfo = count => {
                        return count switch {
                            0 => $"{infonone}",
                            1 => $"{infoone}",
                            _ => $"{count} {infomore}",
                        };
                    }
                });
                searchBar.AddSpecialMatcher('@', lvlMatcherByMod, Calc.HexToColor("1b6dcc"));
            }

            public override void Update(Vector2 position = default) {
                base.Update(position);
                anim = Calc.Approach(anim, lvlCount, Engine.DeltaTime * 60f);
            }
        }

        public enum States {
            Start, Create, Load, Exiting,
        }
        private States state = States.Start;
        private readonly float[] stateLerp = new float[4] { 1f, 0f, 0f, 0f };

        private readonly UIRibbon authors, version;
        private readonly UIButton settings;
        private readonly UIMainButtons buttons;
        private readonly UILevelSelector levelSelector;

        private float fade;

        public UIMainMenu(int width, int height) {
            Width = width;
            Height = height;

            UIMainButtons buttons = new UIMainButtons();
            Add(levelSelector = new UILevelSelector(buttons));

            string mainmenucancel = Dialog.Clean("LEVELEDITORMOD_MAINMENU_CANCEL");

            UIButton create = new UIButton(Dialog.Clean("LEVELEDITORMOD_MAINMENU_CREATE"), Fonts.Regular, 16, 24) {
                FG = Util.Colors.White,
                BG = Util.Colors.Cyan,
                PressedBG = Util.Colors.White,
                PressedFG = Util.Colors.Cyan,
                HoveredBG = Util.Colors.DarkCyan,
            };
            string mainmenuload = Dialog.Clean("LEVELEDITORMOD_MAINMENU_LOAD");
            UIButton load = null;
            load = new UIButton(mainmenuload, Fonts.Regular, 5, 4) {
                OnPress = () => {
                    if (state == States.Load) {
                        state = States.Start;
                        load.SetText(mainmenuload, stayCentered: true);
                    } else {
                        state = States.Load;
                        levelSelector.Reload();
                        load.SetText(mainmenucancel, stayCentered: true);
                    }
                },
            };
            UIButton exit = new UIButton(Dialog.Clean("LEVELEDITORMOD_MAINMENU_EXIT"), Fonts.Regular, 10, 4) {
                FG = Util.Colors.White,
                BG = Util.Colors.Red,
                PressedBG = Util.Colors.White,
                PressedFG = Util.Colors.Red,
                HoveredBG = Util.Colors.DarkRed,
                OnPress = () => state = States.Exiting,
            };

            create.Position = new Vector2(-create.Width / 2, 0);
            load.Position = new Vector2(-load.Width / 2, create.Position.Y + create.Height + 4);
            exit.Position = new Vector2(-exit.Width / 2, load.Position.Y + load.Height + 4);

            Add(this.buttons = buttons);
            RegroupIn(buttons, create, load, exit);
            buttons.Position = new Vector2(width - buttons.Width, height - buttons.Height) / 2;

            settings = new UIButton(Dialog.Clean("LEVELEDITORMOD_MAINMENU_SETTINGS"), Fonts.Regular, 4, 8);
            Add(settings);
            settings.Position = Vector2.UnitX * (Width - settings.Width) + new Vector2(-8, 8);

            Add(authors = new UIRibbon(Dialog.Clean("LEVELEDITORMOD_MAINMENU_CREDITS")) {
                Position = new Vector2(0, 8),
            });
            Add(version = new UIRibbon($"ver{Module.Instance.Metadata.VersionString}") {
                Position = new Vector2(0, 23),
            });
        }

        public override void Update(Vector2 position = default) {
            base.Update(position);

            for (int i = 0; i < stateLerp.Length; i++)
                stateLerp[i] = Calc.Approach(stateLerp[i], ((int)state == i).Bit(), Engine.DeltaTime * 2f);

            switch (state) {
                case States.Exiting:
                    fade = Calc.Approach(fade, 1f, Engine.DeltaTime * 2f);
                    if (fade == 1f) {
                        if (SaveData.Instance == null) {
                            SaveData.InitializeDebugMode();
                            SaveData.Instance.CurrentSession_Safe = new Session(AreaKey.Default);
                        }
                        Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu);
                    }
                    break;

                default:
                    break;
            }

            float startEase = 1 - Ease.CubeInOut(stateLerp[0]);
            authors.Position.X = (int)Math.Round(startEase * (-authors.Width - 2));
            version.Position.X = (int)Math.Round(startEase * (-version.Width - 2));
            settings.Position.Y = (int)Math.Round(startEase * (-settings.Height - 16) + 8);

            float loadEase = Ease.CubeInOut(stateLerp[2]);
            buttons.Position.X = (int)Math.Round((Width - buttons.Width) / 2 - Width / 3 * loadEase);
            levelSelector.Position.X = (int)Math.Round(buttons.Position.X + buttons.Width + 24 - levelSelector.Width * (1 - loadEase));
            levelSelector.Visible = stateLerp[2] != 0f; 
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            if (fade != 0f)
                Draw.Rect(Bounds, Color.Black * fade);
        }
    }
}
