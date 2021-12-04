using Monocle;
using Microsoft.Xna.Framework;
using Celeste;
using System;

namespace Snowberry.Editor.UI.Menus {
    public class UIMainMenu : UIElement {
        public static UIMainMenu Instance { get; private set; }

        public enum States {
            Start, Create, Load, Exiting, Settings
        }
        private States state = States.Start;
        private readonly float[] stateLerp = new float[] { 1f, 0f, 0f, 0f, 0f };

        private readonly UIRibbon authors, version;
        private readonly UIButton settings;
        private readonly UIMainMenuButtons buttons;
        private readonly UILevelSelector levelSelector;

        private float fade;

        public UIMainMenu(int width, int height) {
            Instance = this;

            Width = width;
            Height = height;

            UIMainMenuButtons buttons = new UIMainMenuButtons();
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

            Color rib = Calc.HexToColor("20212e"), acc = Calc.HexToColor("3889d9");
            Add(authors = new UIRibbon(Dialog.Clean("SNOWBERRY_MAINMENU_CREDITS")) {
                Position = new Vector2(0, 8),
                BG = rib,
                BGAccent = acc,
            });
            Add(version = new UIRibbon($"ver{Snowberry.Instance.Metadata.VersionString}") {
                Position = new Vector2(0, 23),
                BG = rib,
                BGAccent = acc,
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

            float settingsEase = Ease.CubeInOut(stateLerp[4]);
            buttons.Position.Y = (int)Math.Round((Height - buttons.Height) / 2 - (Height / 2 + buttons.Height) * settingsEase);
        }

        public override void Render(Vector2 position = default) {
            base.Render(position);

            if (fade != 0f)
                Draw.Rect(Bounds, Color.Black * fade);
        }

        public float StateLerp(States state)
            => stateLerp[(int)state];
    }
}
