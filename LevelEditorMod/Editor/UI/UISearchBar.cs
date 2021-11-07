using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace LevelEditorMod.Editor.UI {
    public class UISearchBar<T> : UITextField {
        public T[] Entries;
        public T[] Found { get; private set; }

        public delegate bool TermMatcher(T entry, string term);
        private readonly TermMatcher termMatcher;
        private readonly Dictionary<char, TermMatcher> specialChars = new Dictionary<char, TermMatcher>();

        public UISearchBar(int width, TermMatcher termMatcher)
            : base(Fonts.Regular, width) {
            Line = Color.Transparent;
            this.termMatcher = termMatcher;
        }

        public void AddSpecialMatcher(char mode, TermMatcher specialMatcher) {
            specialChars.Add(mode, specialMatcher);
        }

        protected override void OnInputUpdate(string input) {
            base.OnInputUpdate(input);

            if (Entries == null || input.Length == 0) {
                Found = null;
                return;
            }

            // search pattern
            List<List<Tuple<char?, string>>> search = new();
            foreach (string termList in input.Split(',')) {
                if (termList.Length != 0) {
                    List<Tuple<char?, string>> l = new();

                    foreach (string term in termList.Split(' ')) {
                        string t = term.Trim();
                        if (t.Length != 0) {
                            char c = t[0];
                            if (t.Length > 1 && specialChars.ContainsKey(c))
                                l.Add(Tuple.Create<char?, string>(c, t.Substring(1)));
                            else
                                l.Add(Tuple.Create<char?, string>(null, t));
                        }
                    }

                    if (l.Count != 0)
                        search.Add(l);
                }
            }

            // matching
            List<T> found = new List<T>();
            foreach (T entry in Entries) {
                bool matched = false;
                foreach (var terms in search) {
                    // try 'AND' seq. of terms 
                    bool m = true;
                    foreach (var term in terms) {
                        bool check = term.Item1 == null ?
                            termMatcher(entry, term.Item2) :
                            specialChars[term.Item1.Value](entry, term.Item2);
                        if (!check) {
                            m = false;
                            break;
                        }
                    }

                    if (m) {
                        matched = true;
                        break; // this entry matches with this 'AND' seq., don't care about the others
                    }
                }

                if (matched)
                    found.Add(entry); // the search matched this entry
            }
            Found = found.ToArray();
        }
    }
}
