using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LevelEditorMod.Editor {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EntityPluginAttribute : Attribute {
        internal readonly string Name;

        public EntityPluginAttribute(string entityName) {
            Name = entityName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class EntityOptionAttribute : Attribute {
        internal readonly string Name;

        public EntityOptionAttribute([CallerMemberName] string optionName = null) {
            Name = optionName;
        }
    }

    public abstract class EntityPlugin {
        private Room room;

        private Vector2 pos;
        protected Vector2 Position => room.Position * 8 + pos;
        protected int Width { get; private set; }
        protected int Height { get; private set; }
        protected Vector2 Origin { get; private set; }

        private readonly List<Vector2> nodes = new List<Vector2>();

        public EntityPlugin() { }

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
            return this;
        }

        internal static EntityPlugin Create(Room room, EntityData entityData) {
            if (Plugins.Entities.TryGetValue(entityData.Name, out var ctor)) {
                EntityPlugin entity = ctor();
                entity.room = room;
                return entity.Initialize(entityData);
            }

            return null;
        }

        internal static bool TryCreate(Room room, EntityData entityData, out EntityPlugin entity) {
            entity = Create(room, entityData);
            return entity != null;
        }
    }
}
