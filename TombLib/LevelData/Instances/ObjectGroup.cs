using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.LevelData
{
    /// <summary>
    /// Represents a group of objects multi-selected by ctrl-clicking.
    /// </summary>
    public class ObjectGroup : PositionBasedObjectInstance, IRotateableY, IEnumerable<ItemInstance>
    {
        private readonly HashSet<ItemInstance> _objects = new HashSet<ItemInstance>();

        public ObjectGroup(ItemInstance initialObject)
        {
            Room = initialObject.Room;
            Position = initialObject.Position;

            _objects.Add(initialObject);
        }

        public ObjectGroup(IReadOnlyList<ItemInstance> objects)
        {
            var initialObject = objects.First();

            Room = initialObject.Room;
            Position = initialObject.Position;

            foreach (var obj in objects)
            {
                _objects.Add(obj);
            }
        }

        public void Add(ItemInstance objectInstance) => _objects.Add(objectInstance);
        public void Remove(ItemInstance objectInstance) => _objects.Remove(objectInstance);
        public bool Contains(ItemInstance obInstance) => _objects.Contains(obInstance);
        public bool Any() => _objects.Any();

        public override void SetPosition(Vector3 position)
        {
            var difference = position - Position;
            base.SetPosition(position);

            foreach (var i in _objects)
                i.SetPosition(i.Position + difference);
        }

        private float _rotationY;

        public float RotationY
        {
            get => _rotationY;
            set
            {
                var difference = value - _rotationY;

                _rotationY = value;

                foreach (var i in _objects)
                    i.RotationY += difference;
            }
        }

        public void RotateAsGroup(float targetRotationDeg)
        {
            var rotationDifferenceRad = (targetRotationDeg - RotationY) * Math.PI / 180.0f;

            RotationY = targetRotationDeg;

            var sin = (float)Math.Sin(-rotationDifferenceRad);
            var cos = (float)Math.Cos(-rotationDifferenceRad);

            foreach (var i in _objects)
            {
                var distance = i.Position - Position;

                var x = distance.X * cos - distance.Z * sin + Position.X;
                var z = distance.X * sin + distance.Z * cos + Position.Z;

                i.SetPosition(new Vector3(x, i.Position.Y, z));
            }
        }

        public ObjectGroup SetRoom(Room room)
        {
            Room = room;
            return this;
        }

        public string ShortName() => $"Group of {_objects.Count} objects";

        public IEnumerator<ItemInstance> GetEnumerator() => _objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();
    }
}