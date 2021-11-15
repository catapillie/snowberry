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
        public Room Room { get; private set; }

        public string Name { get; private set; }

        public int EntityID { get; private set; }

        public Vector2 Position { get; private set; }
        public int X => (int)Position.X;
        public int Y => (int)Position.Y;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2 Center => Position + new Vector2(Width, Height) / 2f;
        public Vector2 Origin { get; private set; }
        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        private bool nodesChanged;
        private readonly List<Vector2> nodes = new List<Vector2>();
        private Vector2[] nodeArray;
        public Vector2[] Nodes {
            get {
                if (nodeArray == null || nodesChanged) {
                    nodeArray = nodes.ToArray();
                    nodesChanged = false;
                }
                return nodeArray;
            }
        }

        private bool updateSelection = true;
        private Rectangle[] selectionRectangles;
        internal Rectangle[] SelectionRectangles {
            get {
                if (updateSelection) {
                    selectionRectangles = Select();
                    updateSelection = false;
                }
                return selectionRectangles;
            }
        }

        public PluginInfo plugin { get; private set; }

        internal Entity SetPosition(Vector2 position) {
            Position = position;
            updateSelection = true;
            return this;
        }

        internal void Move(Vector2 amount) {
            Position += amount;
            updateSelection = true;
        }

        internal void MoveNode(int i, Vector2 amount) {
            if (i >= 0 && i < Nodes.Length) {
                Nodes[i] += amount;
                updateSelection = true;
            }
        }

        public virtual void ChangeDefault() { }
        public virtual void Initialize() => ChangeDefault();
		protected virtual Rectangle[] Select() {
            List<Rectangle> ret = new List<Rectangle>();
			ret.Add(Bounds.Width < 6 ? new Rectangle(X - 3, Y - 3, 6, 6) : Bounds );
			foreach(var node in nodes) {
                ret.Add(new Rectangle((int)node.X - 3, (int)node.Y - 3, 6, 6));
			}
            return ret.ToArray();
		}

		public virtual void Render() { }

        #region Entity Instantiating

        private Entity InitializeData(EntityData entityData) {
            Vector2 offset = Room.Position * 8;

            Position = entityData.Position + offset;
            Width = entityData.Width;
            Height = entityData.Height;
            Origin = entityData.Origin;
            EntityID = entityData.ID;

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
