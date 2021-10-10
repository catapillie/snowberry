using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LevelEditorMod.Editor {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class EntityPluginAttribute : Attribute {
        internal readonly string Name;

        public EntityPluginAttribute(string entityName) {
            Name = entityName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EntityOptionAttribute : Attribute {
        internal readonly string Name;

        public EntityOptionAttribute(string optionName) {
            Name = optionName;
        }
    }

    public abstract class EntityPlugin {
        protected Room Room { get; private set; }

        protected string Name { get; private set; }

        private Vector2 pos;
        protected Vector2 Position => Room.Position * 8 + pos;
        protected int Width { get; private set; }
        protected int Height { get; private set; }
        protected Vector2 Origin { get; private set; }

        private readonly List<Vector2> nodes = new List<Vector2>();

        public EntityPlugin() { }

        internal EntityPlugin SetPosition(Vector2 position) {
            pos = position - Room.Position * 8;
            return this;
        }

        protected Vector2[] GetNodes() {
            return nodes.Select((Vector2 node) => Room.Position * 8 + node).ToArray();
        }

        internal virtual void Render() { }

        private EntityPlugin Initialize(EntityData entityData) {
            pos = entityData.Position;

            Width = entityData.Width;
            Height = entityData.Height;
            Origin = entityData.Origin;

            nodes.AddRange(entityData.Nodes);

            return Initialize(entityData.Values);
        }

        private EntityPlugin Initialize(Dictionary<string, object> data) {
            if (data != null)
                foreach (FieldInfo f in GetType().GetFields()) {
                    if (f.GetCustomAttribute<EntityOptionAttribute>() is EntityOptionAttribute option) {
                        if (option.Name == null || option.Name == string.Empty) {
                            Module.Log(LogLevel.Warn, $"'{f.Name}' ({f.FieldType.Name}) from entity '{Name}' was ignored because it had a null or empty option name!");
                            continue;
                        } else if (data.TryGetValue(option.Name, out object value))
                            f.SetValue(this, value);
                    }
                }

            return this;
        }

        #region Entity Instantiating

        internal static EntityPlugin Create(string name, Room room) {
            if (Plugins.Entities.TryGetValue(name, out var ctor)) {
                EntityPlugin entity = ctor();

                entity.Name = name;
                entity.Room = room;

                return entity;
            }

            return null;
        }

        internal static EntityPlugin Create(Room room, EntityData entityData) {
            if (Plugins.Entities.TryGetValue(entityData.Name, out var ctor)) {
                EntityPlugin entity = ctor();

                entity.Name = entityData.Name;
                entity.Room = room;

                return entity.Initialize(entityData);
            }

            return null;
        }

        internal static bool TryCreate(Room room, EntityData entityData, out EntityPlugin entity) {
            entity = Create(room, entityData);
            return entity != null;
        }

        #endregion
    }
}
