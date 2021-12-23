using Monocle;
using Microsoft.Xna.Framework;
using Celeste;
using System;

namespace LevelEditorMod.Editor.UI {
    public class UIMainMenu : UIElement {
        private class UIMainButtons : UIElement {
            public override void Render(Vector2 position = default) {
                UIMainMenu parent = (UIMainMenu)Parent;
                int right = (int)position.X + Width + 16;
                float ease = Ease.CubeInOut(parent.stateLerp[2]);

                Draw.Rect(0, 0, right, parent.Height, Util.Colors.DarkGray);
                Draw.Rect(right, 8 + parent.Height / 2 * (1 - ease), 1, ease * (parent.Height - 16), Color.LimeGreen * ease);

                base.Render(position);
            }
        }

        private class UILevelSelector : UIElement {
            private class UILevelRibbon : UIRibbon {
                private readonly UILevelSelector selector;

                private readonly string raw;
                private readonly int w;
                public readonly int W;

                private float lerp, listLerp = 1f;
                private readonly int n;

                public UILevelRibbon(UILevelSelector selector, AreaData area, int n)
                    : base($" • {Dialog.Clean(area.Name)}", 24) {
                    this.selector = selector;
                    this.n = n;

                    Accent = 1;
                    raw = Dialog.Has(area.Name) ? $"» {area.Name}" : "...";
                    w = (int)Fonts.Regular.Measure(raw).X;
                    W = Width + w + 5;
                }

                public override void Update(Vector2 position = default) {
                    base.Update(position);
                    int mouseX = (int)Editor.Mouse.Screen.X;
                    int mouseY = (int)Editor.Mouse.Screen.Y;
                    bool hover = new Rectangle((int)position.X + 16, (int)position.Y - 2, Width + w, Height + 4).Contains(mouseX, mouseY);
                    lerp = Calc.Approach(lerp, hover.Bit(), Engine.DeltaTime * 5f);
                    listLerp = Calc.Approach(listLerp, (selector.anim < n).Bit(), Engine.DeltaTime * 4f);
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

            public override void Update(Vector2 position = default) {
                base.Update(position);
                anim = Calc.Approach(anim, lvlCount, Engine.DeltaTime * 60f);
            }

            public void Reload() {
                Clear();

                anim = 0f;

                // search bar
                //Add(new UILabel("\u22C4 all maps :") {
                //    Position = Vector2.UnitY * 8,
                //    Underline = true,
                //});

                UIScrollPane levels = new UIScrollPane() {
                    Height = Parent.Height - 30,
                    Position = new Vector2(-16, 22),
                    BG = Color.Transparent,
                };

                int y = 0;
                Width = 0;
                lvlCount = AreaData.Areas.Count;
                for (int i = 0; i < lvlCount; i++) {
                    AreaData area = AreaData.Areas[i];
                    UILevelRibbon lvl;
                    levels.Add(lvl = new UILevelRibbon(this, area, i) {
                        Position = new Vector2(-8, y),
                        FG = area.TitleTextColor,
                        BG = area.TitleBaseColor,
                        BGAccent = area.TitleAccentColor,
                    });

                    if (lvl.W > Width)
                        Width = lvl.W;
                    y += 15;
                }
                levels.Width = Width;
                Add(levels);
            }
        }

        public enum States {
            Start, Create, Load, Exiting
        }
        private States state = States.Start;
        private int fadeIn;
        private float fadeTimer;
        private readonly float[] stateLerp = new float[4] { 1f, 0f, 0f, 0f };

        private readonly UIRibbon authors, version;
        private readonly UIButton settings;
        private readonly UIMainButtons buttons;
        private readonly UILevelSelector levelSelector;

        private float fade;

        public UIMainMenu(int width, int height, bool fadeIn = false) {
            Width = width;
            Height = height;

            Add(levelSelector = new UILevelSelector());

            //UILabel title = new UILabel("Level Editor") {
            //    Underline = true,
            //    FG = Util.Colors.White,
            //};
            UIButton create = new UIButton("+ new level", Fonts.Regular, 16, 24) {
                FG = Util.Colors.White,
                BG = Util.Colors.Cyan,
                PressedBG = Util.Colors.White,
                PressedFG = Util.Colors.Cyan,
                HoveredBG = Util.Colors.DarkCyan,
            };
            UIButton load = new UIButton("↓ load existing level ", Fonts.Regular, 5, 4) {
                OnPress = () => {
                    state = States.Load;
                    levelSelector.Reload();
                },
            };
            UIButton exit = new UIButton("← exit", Fonts.Regular, 10, 4) {
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

            Add(buttons = new UIMainButtons());
            RegroupIn(buttons, create, load, exit);
            buttons.Position = new Vector2(width - buttons.Width, height - buttons.Height) / 2;

            settings = new UIButton("settings", Fonts.Regular, 4, 8);
            Add(settings);
            settings.Position = Vector2.UnitX * (Width - settings.Width) + new Vector2(-8, 8);

            Add(version = new UIRibbon($"ver{Module.Instance.Metadata.VersionString}") {
                Position = new Vector2(0, 22),
            });
            Add(authors = new UIRibbon($"by catapillie, leppa, and viv") {
                Position = new Vector2(0, 8),
            });
            this.fadeIn = fadeIn ? 2 : 0;
            if (fadeIn)
            {
                stateLerp[0] = 0f;
                fadeTimer = 0.125f;
            }
        }

        public override void Update(Vector2 position = default) {
            base.Update(position);
            //If we're fading in we dont want the ability to click yet, so we delay this.
            if (fadeIn != 0)
            {
                if(fadeIn == 2)
                {
                    fadeTimer -= Engine.DeltaTime;
                    if(fadeTimer <= 0)
                    {
                        fadeTimer = 0;
                        fadeIn = 1;
                    }
                }
                if (fadeIn == 1)
                {
                    stateLerp[0] = Calc.Approach(stateLerp[0], 1, Engine.DeltaTime * 2f);
                    if(stateLerp[0] == 1)
                    {
                        fadeIn = 0;
                    }
                }
            }
            else
            {
                for (int i = 0; i < stateLerp.Length; i++)
                    stateLerp[i] = Calc.Approach(stateLerp[i], ((int)state == i).Bit(), Engine.DeltaTime * 2f);
                switch (state)
                {
                    case States.Exiting:
                        fade = Calc.Approach(fade, 1f, Engine.DeltaTime * 2f);
                        if (fade == 1f)
                        {
                            if (SaveData.Instance == null)
                            {
                                SaveData.InitializeDebugMode();
                                SaveData.Instance.CurrentSession_Safe = new Session(AreaKey.Default);
                            }
                            Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu);
                        }
                        break;

                    default:
                        break;
                }
            }

            float startEase = 1 - Ease.CubeInOut(stateLerp[0]);
            authors.Position.X = startEase * -authors.Width;
            version.Position.X = startEase * -version.Width;
            settings.Position.Y = (int)Math.Round(startEase * (-settings.Height - 16) + 8);

            float loadEase = Ease.CubeInOut(stateLerp[2]);
            buttons.Position.X = (int)Math.Round((Width - buttons.Width) / 2 - Width / 3 * loadEase);
            levelSelector.Position.X = (int)Math.Round(buttons.Position.X + buttons.Width + 24 - levelSelector.Width * (1 - loadEase));
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            if (fade != 0f)
                Draw.Rect(Bounds, Color.Black * fade);
        }
    }
}
