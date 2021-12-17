﻿using Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Linq;

namespace Snowberry.Editor.UI {
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
                public int W { get; private set; }

                private float lerp, listLerp = 1f;
                private readonly int n;

                private bool hover;

                private readonly bool dropdown;
                private bool open;
                private float openLerp;
                private readonly int h;
                public int H { get; private set; }

                private bool pressing;

                private readonly ModeProperties mode;

                private UILevelRibbon(UILevelSelector selector, ModeProperties mode, int n)
                    : base("", 39) {
                    this.selector = selector;
                    this.n = n;

                    Name = mode.MapData.Area.Mode switch {
                        AreaMode.Normal => "A",
                        AreaMode.BSide => "B",
                        AreaMode.CSide => "C",
                        _ => "X",
                    };
                    raw = mode.MapData.Filename;
                    SetText(Name);

                    w = (int)Fonts.Regular.Measure(raw).X;
                    W = Width + w + 5;

                    this.mode = mode;
                }

                public UILevelRibbon(UILevelSelector selector, AreaData area, int n)
                    : base("", 26) {
                    this.selector = selector;
                    this.n = n;

                    Name = area.Name;
                    raw = Dialog.Has(Name) ? $"» {Name}" : "...";

                    ModeProperties[] modes = area.Mode.Where(m => m != null).ToArray();
                    if (dropdown = modes.Length > 1) {
                        h = modes.Length * 13 + 1;
                        for (int i = 0; i < modes.Length; i++) {
                            ModeProperties m = modes[i];
                            Add(new UILevelRibbon(selector, m, i + 1) {
                                Position = new Vector2(-5, 13 * (i + 1)),
                            });
                        }
                    } else
                        mode = modes[0];

                    SetText($"{(dropdown ? "\uF034" : " ")} {Dialog.Clean(Name)}");

                    w = (int)Fonts.Regular.Measure(raw).X;
                    W = Width + w + 5;

                    RenderChildren = false;
                }

                protected override void Initialize() {
                    base.Initialize();
                    foreach (UIElement child in Children) {
                        if (child is UILevelRibbon lvl) {
                            lvl.FG = FG;
                            lvl.BG = BG;
                            lvl.BGAccent = BGAccent;
                        }
                    }
                }

                private bool HoveringChildren() {
                    foreach (UIElement child in Children)
                        if (child is UILevelRibbon lvl && lvl.hover)
                            return true;
                    return false;
                }

                public override void Update(Vector2 position = default) {
                    base.Update(position);

                    int mouseX = (int)Editor.Mouse.Screen.X;
                    int mouseY = (int)Editor.Mouse.Screen.Y;
                    hover = !Instance.confirm.Shown && Visible &&
                        new Rectangle((int)position.X + 16, (int)position.Y - 1, Width + w, Height + H + 2).Contains(mouseX, mouseY);

                    lerp = Calc.Approach(lerp, (hover || pressing).Bit(), Engine.DeltaTime * 6f);

                    listLerp = Calc.Approach(listLerp, (selector.anim < n).Bit(), Engine.DeltaTime * 4f);

                    if (Visible) {
                        if (!Instance.confirm.Shown && hover && ConsumeLeftClick()) {
                            if (dropdown) {
                                if (!HoveringChildren()) {
                                    openLerp = open.Bit();
                                    open = !open;
                                    SetText((open ? '\uF036' : '\uF034') + Text.Substring(1));
                                }
                            } else if (Parent is not UILevelRibbon lvl || lvl.open) {
                                pressing = true;
                            }
                        }
                        if (pressing && ConsumeLeftClick(pressed: false, released: true) || Instance.confirm.Shown) {
                            pressing = false;
                            if (hover) {
                                if (MInput.Keyboard.CurrentState[Keys.LeftControl] == KeyState.Down || MInput.Keyboard.CurrentState[Keys.RightControl] == KeyState.Down)
                                    Editor.Open(mode.MapData);
                                else
                                    Instance.confirm.Show(ConfirmLoadMessage(), () => Editor.Open(mode.MapData));
                            }
                        }
                    }

                    openLerp = Calc.Approach(openLerp, open.Bit(), Engine.DeltaTime * 2f);
                    float openEase = (open ? Ease.ExpoOut : Ease.ExpoIn)(openLerp);
                    H = (int)(openEase * h);
                }

                public override void Render(Vector2 position = default) {
                    Vector2 from = position;

                    float ease = Ease.CubeOut(lerp);
                    float listEase = Ease.ExpoIn(listLerp);
                    position.X += (int)(ease * 16 - Width * listEase + (pressing ? 4 : 0));

                    float sin = Settings.Instance.DisableFlashes || lerp == 0f ? 0f : ((float)Math.Sin(Engine.Scene.TimeActive * 12f) * 0.1f);
                    Fonts.Regular.Draw(raw, position + Vector2.UnitX * (Width + 5), Vector2.One, Color.Lerp(Util.Colors.CloudGray, Util.Colors.White, ease * (0.9f + sin)) * (1 - listEase));

                    base.Render(position);

                    if (dropdown) {
                        foreach (UIElement child in Children)
                            if (child is UILevelRibbon lvl)
                                lvl.Render(from + lvl.Position);
                        Draw.Rect(new Vector2(from.X, position.Y + Height + H + 2), Parent.Width, h - H, Util.Colors.DarkGray);
                        Draw.Rect(new Vector2(from.X, position.Y + Height), 24, H + 2, Util.Colors.DarkGray);
                        Draw.Rect(new Vector2(from.X + 24, position.Y + Height), 1, H + 2, BG);
                    }
                }

                private UIElement ConfirmLoadMessage() {
                    UIRibbon ribbon = new UIRibbon(Dialog.Clean(mode.MapData.Data.Name), 8, 8, true, true) {
                        FG = FG,
                        BG = BG,
                        BGAccent = BGAccent,
                    };
                    ribbon.Position = new Vector2(-ribbon.Width / 2, 0);

                    UILabel msg = new UILabel(Dialog.Clean("SNOWBERRY_MAINMENU_LOAD_CONFIRM"));
                    msg.Position = new Vector2(-msg.Width / 2, ribbon.Position.Y + ribbon.Height + 4);

                    UILabel warn = new UILabel(Dialog.Clean("SNOWBERRY_MAINMENU_LOAD_UNSAVED")) {
                        FG = Util.Colors.CloudLightGray,
                    };
                    warn.Position = new Vector2(-warn.Width / 2, msg.Position.Y + msg.Height);

                    UILabel tip = new UILabel(Dialog.Clean("SNOWBERRY_MAINMENU_LOAD_TIP")) {
                        FG = Util.Colors.CloudLightGray,
                    };
                    tip.Position = new Vector2(-tip.Width / 2, warn.Position.Y + warn.Height);

                    return Regroup(ribbon, msg, warn, tip);
                }
            }

            private float anim;
            private int lvlCount;

            private UISearchBar<UILevelRibbon> searchBar;
            private UILevelRibbon[] levels;

            public void Reload() {
                Clear();

                anim = 0f;

                UIScrollPane levelScrollPane = new UIScrollPane() {
                    Height = Parent.Height - 30,
                    Position = new Vector2(-16, 22),
                    BG = Color.Transparent,
                    Background = Color.Transparent,
                    ShowScrollBar = false,
                };

                levels = new UILevelRibbon[lvlCount = AreaData.Areas.Count];
                int y = 0;
                Width = 0;
                for (int i = 0; i < lvlCount; i++) {
                    AreaData area = AreaData.Areas[i];
                    UILevelRibbon lvl;
                    levelScrollPane.Add(lvl = new UILevelRibbon(this, area, i) {
                        Position = new Vector2(-10, y),
                        FG = area.TitleTextColor,
                        BG = area.TitleBaseColor,
                        BGAccent = area.TitleAccentColor,
                    });

                    if (lvl.W > Width)
                        Width = lvl.W;
                    y += 13;

                    levels[i] = lvl;
                }
                Add(levelScrollPane);
                levelScrollPane.Width = Width;

                static bool lvlMatcher(UILevelRibbon entry, string term) {
                    return entry.Text.ToLower().Contains(term.ToLower());
                }

                static bool lvlMatcherByMod(UILevelRibbon entry, string term) {
                    return entry.Name.ToLower().Contains(term.ToLower());
                }

                string infonone = Dialog.Clean("SNOWBERRY_MAINMENU_LOADSEARCHBAR_NONE");
                string infoone = Dialog.Clean("SNOWBERRY_MAINMENU_LOADSEARCHBAR_ONE");
                string infomore = Dialog.Clean("SNOWBERRY_MAINMENU_LOADSEARCHBAR_MORE");
                Add(searchBar = new UISearchBar<UILevelRibbon>(Width / 2, lvlMatcher) {
                    Position = Vector2.UnitY * 8,
                    Entries = levels,
                    InfoText = Dialog.Clean("SNOWBERRY_MAINMENU_LOADSEARCH"),
                    SearchInfo = count => {
                        return count switch {
                            0 => $"{infonone}",
                            1 => $"{infoone}",
                            _ => $"{count} {infomore}",
                        };
                    },
                    OnInputChange = s => {
                        if (levels != null)
                            levels[0].Position.Y = 0;
                    }
                });
                searchBar.AddSpecialMatcher('@', lvlMatcherByMod, Calc.HexToColor("1b6dcc"));
            }

            public override void Update(Vector2 position = default) {
                base.Update(position);

                anim = Calc.Approach(anim, lvlCount, Engine.DeltaTime * 60f);

                if (levels != null) {
                    int y = (int)levels[0].Position.Y;
                    foreach (UILevelRibbon lvl in levels) {
                        lvl.Visible = searchBar.Found == null || searchBar.Found.Contains(lvl);
                        if (lvl.Visible) {
                            lvl.Position.Y = y;
                            y += 13 + lvl.H;
                        }
                    }
                }
            }
        }

        public class UIConfirmMessage : UIElement {
            private UIElement display;
            private readonly UIElement buttons;

            private Action confirm;

            private float lerp;
            public bool Shown { get; private set; }

            public UIConfirmMessage() {
                UIButton yes = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_YES"), Fonts.Regular, 4, 6) {
                    FG = Util.Colors.White,
                    BG = Util.Colors.Blue,
                    PressedBG = Util.Colors.White,
                    PressedFG = Util.Colors.Blue,
                    HoveredBG = Util.Colors.DarkBlue,
                    OnPress = () => confirm?.Invoke(),
                };
                UIButton no = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_NO"), Fonts.Regular, 4, 6) {
                    FG = Util.Colors.White,
                    BG = Util.Colors.Red,
                    PressedBG = Util.Colors.White,
                    PressedFG = Util.Colors.Red,
                    HoveredBG = Util.Colors.DarkRed,
                    Position = new Vector2(yes.Position.X + yes.Width + 4, yes.Position.Y),
                    OnPress = Hide,
                };

                Add(buttons = Regroup(yes, no));
                buttons.Visible = false;
            }

            protected override void Initialize() {
                base.Initialize();
                buttons.Position.X = (Width - buttons.Width) / 2;
            }

            public void Show(UIElement display, Action onConfirm = null) {
                if (this.display != null)
                    Remove(this.display);

                confirm = onConfirm;
                Add(this.display = display);
                display.Position.X = (Width - display.Width) / 2;

                if (!Shown) {
                    lerp = 0f;
                    Shown = true;
                }
            }

            public void Hide() {
                if (Shown) {
                    lerp = 1f;
                    Shown = false;
                }
            }

            public override void Update(Vector2 position = default) {
                base.Update(position);

                lerp = Calc.Approach(lerp, Shown.Bit(), Engine.DeltaTime * 2f);
                float ease = (Shown ? Ease.ExpoOut : Ease.ExpoIn)(lerp);

                int h = 0;
                buttons.Visible = lerp > 0;
                if (display != null) {
                    display.Visible = buttons.Visible;
                    h = display.Height;
                    display.Position.Y = (int)(((Height - display.Height) / 2 + h * 2) * ease - h * 2);
                }

                buttons.Position.Y = (int)(Height + buttons.Height - ((Height - h - 6) / 2 + buttons.Height) * ease);
            }

            public override void Render(Vector2 position = default) {
                Draw.Rect(position, Width, Height, Color.Black * lerp * 0.75f);
                base.Render(position);
            }
        }

        public static UIMainMenu Instance { get; private set; }

        public enum States {
            Start, Create, Load, Exiting, Settings
        }
        private States state = States.Start;
        private readonly float[] stateLerp = new float[] { 1f, 0f, 0f, 0f, 0f };

        private readonly UIRibbon authors, version;
        private readonly UIButton settings;
        private readonly UIMainButtons buttons;
        private readonly UILevelSelector levelSelector;

        private readonly UIConfirmMessage confirm;

        private float fade;

        public UIMainMenu(int width, int height) {
            Instance = this;

            Width = width;
            Height = height;

            UIMainButtons buttons = new UIMainButtons();
            Add(levelSelector = new UILevelSelector());

            string mainmenuload = Dialog.Clean("SNOWBERRY_MAINMENU_LOAD");
            string mainmenucreate = Dialog.Clean("SNOWBERRY_MAINMENU_CREATE");
            string mainmenuclose = Dialog.Clean("SNOWBERRY_MAINMENU_CLOSE");

            UIButton create = null, load = null, exit = null;

            create = new UIButton(mainmenucreate, Fonts.Regular, 16, 24) {
                FG = Util.Colors.White,
                BG = Util.Colors.Cyan,
                PressedBG = Util.Colors.White,
                PressedFG = Util.Colors.Cyan,
                HoveredBG = Util.Colors.DarkCyan,
                OnPress = () => {
                    if (state == States.Create) {
                        state = States.Start;
                        create.SetText(mainmenucreate, stayCentered: true);
                    } else {
                        state = States.Create;
                        load.SetText(mainmenuload, stayCentered: true);
                        create.SetText(mainmenuclose, stayCentered: true);
                    }
                },
            };

            load = new UIButton(mainmenuload, Fonts.Regular, 5, 4) {
                OnPress = () => {
                    if (state == States.Load) {
                        state = States.Start;
                        load.SetText(mainmenuload, stayCentered: true);
                    } else {
                        state = States.Load;
                        levelSelector.Reload();
                        create.SetText(mainmenucreate, stayCentered: true);
                        load.SetText(mainmenuclose, stayCentered: true);
                    }
                },
            };

            exit = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_EXIT"), Fonts.Regular, 10, 4) {
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

            settings = new UIButton(Dialog.Clean("SNOWBERRY_MAINMENU_SETTINGS"), Fonts.Regular, 4, 8) {
                OnPress = () => {
                    if (state == States.Start) {
                        state = States.Settings;
                    }
                }
            };
            Add(settings);
            settings.Position = Vector2.UnitX * (Width - settings.Width) + new Vector2(-8, 8);

            Add(authors = new UIRibbon(Dialog.Clean("SNOWBERRY_MAINMENU_CREDITS")) {
                Position = new Vector2(0, 8),
            });
            Add(version = new UIRibbon($"ver{Snowberry.Instance.Metadata.VersionString}") {
                Position = new Vector2(0, 23),
            });

            Add(confirm = new UIConfirmMessage() {
                Width = width,
                Height = height,
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

            float createEase = Ease.CubeInOut(stateLerp[1]);
            float loadEase = Ease.CubeInOut(stateLerp[2]);
            buttons.Position.X = (int)Math.Round((Width - buttons.Width) / 2 - Width / 3 * loadEase + Width / 3 * createEase);

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