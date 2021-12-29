using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Snowberry {
    public class FormattedText {
        private static readonly Regex commandMatch = new Regex(@"^{((?:[^{}\n])*)}");
        private static readonly Regex colorCmd = new Regex(@"^\s*#?[a-fA-F0-9]{6}\s*");
        private static readonly Regex colorPopCmd = new Regex(@"^\s*#<<\s*");

        private readonly Tuple<char?, Color>[] characters;

        private FormattedText(string expr) {
            Stack<Color> colors = new Stack<Color>();
            List<Tuple<char?, Color>> characters = new List<Tuple<char?, Color>>();

            bool esc = false;
            for (int i = 0; i < expr.Length; i++) {
                char c = expr[i];
                if (c == '\\') {
                    esc = true;
                    continue;
                }

                if (!esc) {
                    Match cmdMatch = commandMatch.Match(expr.Substring(i));
                    if (cmdMatch.Success) {
                        string cmd = cmdMatch.Groups[1].Value;

                        if (colorCmd.IsMatch(cmd)) {
                            colors.Push(Calc.HexToColor(cmd.Trim()));
                        } else if (colorPopCmd.IsMatch(cmd)) {
                            if (colors.Count != 0) colors.Pop();
                        } else
                            characters.Add(Tuple.Create<char?, Color>(null, colors.Count == 0 ? Color.White : colors.Peek()));

                        i += cmdMatch.Length - 1;
                        continue;
                    }
                }

                esc = false;

                characters.Add(Tuple.Create<char?, Color>(c, colors.Count == 0 ? Color.White : colors.Peek()));
            }

            this.characters = characters.ToArray();
        }

        public string Format(out Color[] colors, params object[] values) {
            string formatted = "";
            List<Color> colorList = new List<Color>();
            int v = 0;
            foreach (var pair in characters) {
                if (pair.Item1 == null) {
                    if (v < values.Length) {
                        string insert = values[v]?.ToString() ?? "null";
                        formatted += insert;
                        colorList.AddRange(Enumerable.Repeat(pair.Item2, insert.Length));
                        v++;
                    }
                } else {
                    formatted += pair.Item1;
                    colorList.Add(pair.Item2);
                }
            }

            colors = colorList.ToArray();
            return formatted;
        }

        public static FormattedText Parse(string expression) {
            return new FormattedText((string)expression.Clone());
        }
    }
}