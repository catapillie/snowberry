using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace LevelEditorMod {
    public static class DrawUtil {
        public static void DottedLine(Vector2 start, Vector2 end, Color color, float dot = 2f, float space = 2f) {
            float d = Vector2.Distance(start, end);
            Vector2 dir = (end - start).SafeNormalize();
            float step = dot + space;
            for (float x = 0f; x < d; x += step) {
                Vector2 a = start + dir * Math.Min(x, d);
                Vector2 b = start + dir * Math.Min(x + dot, d);
                Draw.Line(a, b, color);
            }
        }
    }
}
