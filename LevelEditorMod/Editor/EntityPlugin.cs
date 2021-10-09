using Celeste;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LevelEditorMod.Editor {
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityPluginAttribute : Attribute {
        internal readonly string Name;

        public EntityPluginAttribute(string entityName) {
            Name = entityName;
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EntityOptionAttribute : Attribute {
        internal readonly string Name;

        public EntityOptionAttribute([CallerMemberName] string optionName = null) {
            Name = optionName;
        }
    }

    public abstract class EntityPlugin {
        private Room room;

        private Vector2 pos;
        private int width;
        private int height;
        private Vector2 origin; // unused

        private readonly List<Vector2> nodes = new List<Vector2>();

        public EntityPlugin() { }

        private EntityPlugin Initialize(EntityData entityData) {
            pos = entityData.Position;
            width = entityData.Width;
            height = entityData.Height;
            origin = entityData.Origin;
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
