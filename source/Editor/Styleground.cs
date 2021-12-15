
using Celeste;
using Celeste.Mod;

using Microsoft.Xna.Framework;

using Monocle;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using static Celeste.BinaryPacker;

namespace Snowberry.Editor {

    public class Styleground : Plugin {

        public Map Map;

        public HashSet<string> Tags = new();

        public Vector2 Position;

        public Vector2 Scroll = Vector2.One;

        public Vector2 Speed;

        public float WindMultiplier;

        public Color Color = Color.White;

        public bool LoopX = true;

        public bool LoopY = true;

        public bool? DreamingOnly;

        public bool FlipX;

        public bool FlipY;

        public string OnlyIn = "*";

        public string ExcludeFrom = "";

        public string Flag = "";

        public string NotFlag = "";

        public string ForceFlag = "";

        public bool InstantIn = true;

        public bool InstantOut;

        public virtual bool Additive => false;

        // Render on the Snowberry background
        public virtual void Render() { }

        public virtual string Title() {
            return Name;
        }

        public bool IsVisible(Room room) {
            string roomName = room?.Name ?? "";

            if (ExcludeFrom != null && MatchRoomName(ExcludeFrom, roomName)) {
                return false;
            }

            if (OnlyIn != null && !MatchRoomName(OnlyIn, roomName)) {
                return false;
            }

            return true;
        }

        public static bool MatchRoomName(string predicate, string roomName) {
            string[] array = predicate.Split(',');
            foreach(string text in array) {
                if(text.Equals(roomName)) {
                    return true;
                } else if(text.Contains("*")) {
                    string pattern = "^" + Regex.Escape(text).Replace("\\*", ".*") + "$";
                    return Regex.IsMatch(roomName, pattern);
                }
            }
            return false;
        }

        public static Styleground Create(string name, Map map, Element data, Element applyData = null) {
            if (PluginInfo.Stylegrounds.TryGetValue(name, out PluginInfo plugin)) {
                Styleground styleground = plugin.Instantiate<Styleground>();
                styleground.Map = map;

                if (data != null && data.Attributes != null) {
                    applyData ??= new();
                    applyData.Attributes ??= new();

                    foreach (var item in applyData.Attributes)
                        styleground.Set(item.Key, item.Value);

                    foreach (var item in data.Attributes)
                        styleground.Set(item.Key, item.Value);

                    // currently this is just the same thing map data does
                    // its terrible

                    if (data.HasAttr("tag"))
                        styleground.Tags.Add(data.Attr("tag"));

                    if (applyData != null && applyData.HasAttr("tag"))
                        styleground.Tags.Add(applyData.Attr("tag"));

                    if (data.HasAttr("x"))
                        styleground.Position.X = data.AttrFloat("x");
                    else if (applyData != null && applyData.HasAttr("x"))
                        styleground.Position.X = applyData.AttrFloat("x");

                    if (data.HasAttr("y"))
                        styleground.Position.Y = data.AttrFloat("y");
                    else if (applyData != null && applyData.HasAttr("y"))
                        styleground.Position.Y = applyData.AttrFloat("y");

                    if (data.HasAttr("scrollx"))
                        styleground.Scroll.X = data.AttrFloat("scrollx");
                    else if (applyData != null && applyData.HasAttr("scrollx"))
                        styleground.Scroll.X = applyData.AttrFloat("scrollx");

                    if (data.HasAttr("scrolly"))
                        styleground.Scroll.Y = data.AttrFloat("scrolly");
                    else if (applyData != null && applyData.HasAttr("scrolly"))
                        styleground.Scroll.Y = applyData.AttrFloat("scrolly");

                    if (data.HasAttr("speedx"))
                        styleground.Speed.X = data.AttrFloat("speedx");
                    else if (applyData != null && applyData.HasAttr("speedx"))
                        styleground.Speed.X = applyData.AttrFloat("speedx");

                    if (data.HasAttr("speedy"))
                        styleground.Speed.Y = data.AttrFloat("speedy");
                    else if (applyData != null && applyData.HasAttr("speedy"))
                        styleground.Speed.Y = applyData.AttrFloat("speedy");

                    styleground.Color = Color.White;
                    if (data.HasAttr("color"))
                        styleground.Color = Calc.HexToColor(data.Attr("color"));
                    else if (applyData != null && applyData.HasAttr("color"))
                        styleground.Color = Calc.HexToColor(applyData.Attr("color"));

                    if (data.HasAttr("alpha"))
                        styleground.Color *= data.AttrFloat("alpha");
                    else if (applyData != null && applyData.HasAttr("alpha"))
                        styleground.Color *= applyData.AttrFloat("alpha");

                    if (data.HasAttr("flipx"))
                        styleground.FlipX = data.AttrBool("flipx");
                    else if (applyData != null && applyData.HasAttr("flipx"))
                        styleground.FlipX = applyData.AttrBool("flipx");

                    if (data.HasAttr("flipy"))
                        styleground.FlipY = data.AttrBool("flipy");
                    else if (applyData != null && applyData.HasAttr("flipy"))
                        styleground.FlipY = applyData.AttrBool("flipy");

                    if (data.HasAttr("loopx"))
                        styleground.LoopX = data.AttrBool("loopx");
                    else if (applyData != null && applyData.HasAttr("loopx"))
                        styleground.LoopX = applyData.AttrBool("loopx");

                    if (data.HasAttr("loopy"))
                        styleground.LoopY = data.AttrBool("loopy");
                    else if (applyData != null && applyData.HasAttr("loopy"))
                        styleground.LoopY = applyData.AttrBool("loopy");

                    if (data.HasAttr("wind"))
                        styleground.WindMultiplier = data.AttrFloat("wind");
                    else if (applyData != null && applyData.HasAttr("wind"))
                        styleground.WindMultiplier = applyData.AttrFloat("wind");

                    string exclude = null;
                    if (data.HasAttr("exclude"))
                        exclude = data.Attr("exclude");
                    else if (applyData != null && applyData.HasAttr("exclude"))
                        exclude = applyData.Attr("exclude");

                    if (exclude != null)
                        styleground.ExcludeFrom = exclude;

                    string only = null;
                    if (data.HasAttr("only"))
                        only = data.Attr("only");
                    else if (applyData != null && applyData.HasAttr("only"))
                        only = applyData.Attr("only");

                    if (only != null)
                        styleground.OnlyIn = only;

                    string flag = null;
                    if (data.HasAttr("flag"))
                        flag = data.Attr("flag");
                    else if (applyData != null && applyData.HasAttr("flag"))
                        flag = applyData.Attr("flag");

                    if (flag != null)
                        styleground.Flag = flag;

                    string notFlag = null;
                    if (data.HasAttr("notflag"))
                        notFlag = data.Attr("notflag");
                    else if (applyData != null && applyData.HasAttr("notflag"))
                        notFlag = applyData.Attr("notflag");

                    if (notFlag != null)
                        styleground.NotFlag = notFlag;

                    string alwaysFlag = null;
                    if (data.HasAttr("always"))
                        alwaysFlag = data.Attr("always");
                    else if (applyData != null && applyData.HasAttr("always"))
                        alwaysFlag = applyData.Attr("always");

                    if (alwaysFlag != null)
                        styleground.ForceFlag = alwaysFlag;

                    bool? dreaming = null;
                    if (data.HasAttr("dreaming"))
                        dreaming = data.AttrBool("dreaming");
                    else if (applyData != null && applyData.HasAttr("dreaming"))
                        dreaming = applyData.AttrBool("dreaming");

                    if (dreaming.HasValue)
                        styleground.DreamingOnly = dreaming;

                    if (data.HasAttr("instantIn"))
                        styleground.InstantIn = data.AttrBool("instantIn");
                    else if (applyData != null && applyData.HasAttr("instantIn"))
                        styleground.InstantIn = applyData.AttrBool("instantIn");

                    if (data.HasAttr("instantOut"))
                        styleground.InstantOut = data.AttrBool("instantOut");
                    else if (applyData != null && applyData.HasAttr("instantOut"))
                        styleground.InstantOut = applyData.AttrBool("instantOut");
                }

                return styleground;
            }

            return null;
        }

        public Element ToElement() {
            Element elem = new();
            elem.Attributes = new();
            elem.Children = new();

            elem.Name = Name;
            //elem.Attributes["name"] 

            return elem;
        }
    }
}
