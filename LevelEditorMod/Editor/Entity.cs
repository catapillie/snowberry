using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace LevelEditorMod.Editor {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PluginAttribute : Attribute {
        internal readonly string Name;

        public PluginAttribute(string entityName) {
            Name = entityName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OptionAttribute : Attribute {
        internal readonly string Name;

        public OptionAttribute(string optionName) {
            Name = optionName;
        }
    }

    public abstract class Entity {
        protected Room Room { get; private set; }

        public string Name { get; private set; }

        protected Vector2 Position { get; private set; }
        protected int X => (int)Position.X;
        protected int Y => (int)Position.Y;
        protected int Width { get; private set; }
        protected int Height { get; private set; }
        protected Vector2 Center => Position + new Vector2(Width, Height) / 2f;
        protected Vector2 Origin { get; private set; }
        protected Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        private bool nodesChanged;
        private readonly List<Vector2> nodes = new List<Vector2>();
        private Vector2[] nodeArray;
        protected Vector2[] Nodes {
            get {
                if (nodeArray == null || nodesChanged) {
                    nodeArray = nodes.ToArray();
                    nodesChanged = false;
                }
                return nodeArray;
            }
        }

        private PluginInfo plugin;

        internal Entity SetPosition(Vector2 position) {
            Position = position;
            return this;
        }

        protected Rectangle[] NodesToRectangles(int squareLength)
            => NodesToRectangles(squareLength, squareLength, -squareLength / 2, -squareLength / 2);

        protected Rectangle[] NodesToRectangles(int width, int height, int offsetX = 0, int offsetY = 0) {
            Rectangle[] rects = new Rectangle[Nodes.Length];
            for (int i = 0; i < rects.Length; i++) {
                Vector2 node = Nodes[i];
                rects[i] = new Rectangle((int)node.X + offsetX, (int)node.Y + offsetY, width, height);
            }
            return rects;
        }

        public virtual void ChangeDefault() { }
        public virtual void Initialize() => ChangeDefault();
        public virtual void Render() { }
        public virtual Selection Select() => default;

        #region Entity Instantiating

        private Entity InitializeData(EntityData entityData) {
            Vector2 offset = Room.Position * 8;

            Position = entityData.Position + offset;
            Width = entityData.Width;
            Height = entityData.Height;
            Origin = entityData.Origin;

            foreach (Vector2 node in entityData.Nodes)
                nodes.Add(node + offset);

            return InitializeData(entityData.Values);
        }

        private Entity InitializeData(Dictionary<string, object> data) {
            if (data != null)
                foreach (KeyValuePair<string, object> pair in data)
                    plugin[this, pair.Key] = pair.Value;

            Initialize();
            return this;
        }

        internal static Entity Create(string name, Room room) {
            if (PluginInfo.All.TryGetValue(name, out PluginInfo plugin)) {
                Entity entity = plugin.Instantiate();
                entity.plugin = plugin;

                entity.Name = name;
                entity.Room = room;

                entity.Initialize();
                return entity;
            }

            return null;
        }

        internal static Entity Create(Room room, EntityData entityData) {
            if (PluginInfo.All.TryGetValue(entityData.Name, out PluginInfo plugin)) {
                Entity entity = plugin.Instantiate();
                entity.plugin = plugin;

                entity.Name = entityData.Name;
                entity.Room = room;

                return entity.InitializeData(entityData);
            }

            return null;
        }

        internal static bool TryCreate(Room room, EntityData entityData, out Entity entity) {
            entity = Create(room, entityData);
            return entity != null;
        }

        #endregion
    }
}
