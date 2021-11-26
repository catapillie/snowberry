using Celeste;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Snowberry.Editor {
    public abstract class Entity : Plugin {
        public Room Room { get; private set; }

        public int EntityID = 0;

        public Vector2 Position { get; private set; }
        public int X => (int)Position.X;
        public int Y => (int)Position.Y;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2 Center => Position + new Vector2(Width, Height) / 2f;
        public Vector2 Origin { get; private set; }
        public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

        public bool Tracked { get; protected set; }

        // -1 = not resizable in that direction
        public virtual int MinWidth => -1;
        public virtual int MinHeight => -1;

        public virtual int MinNodes => 0;

        // -1 = unlimited nodes
        public virtual int MaxNodes => 0;

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

        public Entity SetPosition(Vector2 position) {
            Position = position;
            updateSelection = true;
            Room.MarkTrackedEntityDirty(this);
            return this;
        }

        public void Move(Vector2 amount) {
            Position += amount;
            updateSelection = true;
            Room.MarkTrackedEntityDirty(this);
        }

        public void SetNode(int i, Vector2 position) {
            if (i >= 0 && i < Nodes.Length) {
                Nodes[i] = position;
                updateSelection = true;
            }
            Room.MarkTrackedEntityDirty(this);
        }

        public void MoveNode(int i, Vector2 amount) {
            if (i >= 0 && i < Nodes.Length) {
                nodes[i] += amount;
                updateSelection = nodesChanged = true;
            }
            Room.MarkTrackedEntityDirty(this);
        }

        public void AddNode(Vector2 position) {
            nodes.Add(position);
            nodesChanged = true;
            Room.MarkTrackedEntityDirty(this);
        }

        internal void ResetNodes() {
            nodes.Clear();
            nodesChanged = true;
            Room.MarkTrackedEntityDirty(this);
        }

        public void SetWidth(int width) {
            Width = width;
            updateSelection = true;
            Room.MarkTrackedEntityDirty(this);
        }

        public void SetHeight(int heigth) {
            Height = heigth;
            updateSelection = true;
            Room.MarkTrackedEntityDirty(this);
        }

        public virtual void ChangeDefault() {}
        public virtual void Initialize() => ChangeDefault();
        public virtual void InitializeAfter() { }
		protected virtual Rectangle[] Select() {
            List<Rectangle> ret = new List<Rectangle>();
			ret.Add(new Rectangle(Width < 6 ? X - 3 : X, Height < 6 ? Y - 3 : Y, Width < 6 ? 6 : Width, Height < 6 ? 6 : Height));
			foreach(var node in nodes) {
                ret.Add(new Rectangle((int)node.X - 3, (int)node.Y - 3, 6, 6));
			}
            return ret.ToArray();
		}

		public virtual void Render() { }

        public virtual void RenderBefore() { }

        #region Entity Instantiating

        public virtual void ApplyDefaults() {}

        private Entity InitializeData(EntityData entityData) {
            Vector2 offset = Room.Position * 8;

            Position = entityData.Position + offset;
            Width = entityData.Width;
            Height = entityData.Height;
            Origin = entityData.Origin;
            EntityID = entityData.ID;

            nodes.Clear();
            foreach (Vector2 node in entityData.Nodes)
                nodes.Add(node + offset);

            return InitializeData(entityData.Values);
        }

        private Entity InitializeData(Dictionary<string, object> data) {
            if (data != null)
                foreach (KeyValuePair<string, object> pair in data)
                    Set(pair.Key, pair.Value);

            Initialize();
            return this;
        }

        public static Entity Create(string name, Room room) {
            if (PluginInfo.All.TryGetValue(name, out PluginInfo plugin)) {
                Entity entity = plugin.Instantiate<Entity>();

                entity.Room = room;

                entity.ApplyDefaults();
                entity.Initialize();
                return entity;
            }

            return null;
        }

        internal static Entity Create(Room room, EntityData entityData) {
            if (PluginInfo.All.TryGetValue(entityData.Name, out PluginInfo plugin)) {
                Entity entity = plugin.Instantiate<Entity>();

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
