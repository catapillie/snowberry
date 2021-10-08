using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LevelEditorMod.Editor {
    public class Room {
        public string Name { get; private set; }

        public Rectangle Bounds { get; private set; }

        public int X => Bounds.X;
        public int Y => Bounds.Y;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public Vector2 Position => new Vector2(X, Y);
        public Vector2 Size => new Vector2(Width, Height);

        public VirtualMap<char> FgTileMap { get; private set; }
        public VirtualMap<char> BgTileMap { get; private set; }
        public VirtualMap<MTexture> FgTiles, BgTiles;

        public readonly List<Decal> FgDecals = new List<Decal>();
        public readonly List<Decal> BgDecals = new List<Decal>();

        public int LoadSeed {
            get {
                int num = 0;
                string name = Name;
                foreach (char c in name) {
                    num += c;
                }
                return num;
            }
        }

        private readonly Regex tileSplitter = new Regex("\\r\\n|\\n\\r|\\n|\\r");

        public Room(string name, Rectangle bounds) {
            Name = name;
            Bounds = bounds;
            FgTileMap = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
            BgTileMap = new VirtualMap<char>(bounds.Width, bounds.Height, '0');
        }

        public Room(LevelData data)
            : this(data.Name, data.TileBounds) {
            // BgTiles
            string[] array = tileSplitter.Split(data.Bg);
            for (int i = 0; i < array.Length; i++) {
                for (int j = 0; j < array[i].Length; j++) {
                    BgTileMap[j, i] = array[i][j];
                }
            }

            // FgTiles
            string[] array2 = tileSplitter.Split(data.Solids);
            for (int i = 0; i < array2.Length; i++) {
                for (int j = 0; j < array2[i].Length; j++) {
                    FgTileMap[j, i] = array2[i][j];
                }
            }

            Autotile();

            // BgDecals
            foreach (DecalData decal in data.BgDecals) {
                BgDecals.Add(new Decal(this, decal));
            }

            // FgDecals
            foreach (DecalData decal in data.FgDecals) {
                FgDecals.Add(new Decal(this, decal));
            }
        }

        private void Autotile() {
            FgTiles = GFX.FGAutotiler.GenerateMap(FgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid.Tiles;
            BgTiles = GFX.BGAutotiler.GenerateMap(BgTileMap, new Autotiler.Behaviour() { EdgesExtend = true }).TileGrid.Tiles;
        }

        public void Render() {
            Vector2 offset = Position * 8;

            Draw.Rect(offset, Width * 8, Height * 8, Color.White * 0.1f);

            // BgTiles
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (BgTiles[x, y] != null)
                        BgTiles[x, y].Draw(offset + new Vector2(x, y) * 8);

            // BgDecals
            foreach (Decal decal in BgDecals)
                decal.Texture.DrawCentered(offset + decal.Position, Color.White, decal.Scale);

            // FgTiles
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (FgTiles[x, y] != null)
                        FgTiles[x, y].Draw(offset + new Vector2(x, y) * 8);

            // FgDecals
            foreach (Decal decal in FgDecals)
                decal.Texture.DrawCentered(offset + decal.Position, Color.White, decal.Scale);
        }
    }
}
