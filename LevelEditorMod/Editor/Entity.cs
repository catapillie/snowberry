using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        protected string Name { get; private set; }

        protected Vector2 Position { get; private set; }
        protected int Width { get; private set; }
        protected int Height { get; private set; }
        protected Vector2 Origin { get; private set; }

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

        internal Entity SetPosition(Vector2 position) {
            Position = position;
            return this;
        }

        public virtual void Initialize() { }
        public virtual void Render() { }

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
                foreach (FieldInfo f in GetType().GetFields()) {
                    if (f.GetCustomAttribute<OptionAttribute>() is OptionAttribute option) {
                        if (option.Name == null || option.Name == string.Empty) {
                            Module.Log(LogLevel.Warn, $"'{f.Name}' ({f.FieldType.Name}) from entity '{Name}' was ignored because it had a null or empty option name!");
                            continue;
                        } else if (data.TryGetValue(option.Name, out object value))
                            f.SetValue(this, value);
                    }
                }

            Initialize();
            return this;
        }

        internal static Entity Create(string name, Room room) {
            if (Plugins.Entities.TryGetValue(name, out var ctor)) {
                Entity entity = ctor();

                entity.Name = name;
                entity.Room = room;

                entity.Initialize();
                return entity;
            }

            return null;
        }

        internal static Entity Create(Room room, EntityData entityData) {
            if (Plugins.Entities.TryGetValue(entityData.Name, out var ctor)) {
                Entity entity = ctor();

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
