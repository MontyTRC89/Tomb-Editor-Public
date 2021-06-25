﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.LevelData
{
    /// <summary>
    /// Represents a group of objects multi-selected by ctrl-clicking.
    /// </summary>
    public class ObjectGroup : PositionBasedObjectInstance, IRotateableY
    {
        public ObjectGroup(ItemInstance initialObject)
        {
            Room = initialObject.Room;
            Position = initialObject.Position;

            Objects.Add(initialObject);
        }

        public HashSet<ItemInstance> Objects { get; } = new HashSet<ItemInstance>();

        public void Add(ItemInstance objectInstance) => Objects.Add(objectInstance);
        public void Remove(ItemInstance objectInstance) => Objects.Remove(objectInstance);
        public bool Contains(ItemInstance obInstance) => Objects.Contains(obInstance);
        public bool Any() => Objects.Any();
        public List<ObjectInstance> ToObjectInstances() => Objects.OfType<ObjectInstance>().ToList();

        public override void SetPosition(Vector3 position)
        {
            var difference = position - Position;
            base.SetPosition(position);

            foreach (var i in Objects)
                i.SetPosition(i.Position + difference);
        }


        public override void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            base.Transform(transformation, oldRoomSize);
            foreach (var oi in Objects)
                oi.Transform(transformation, oldRoomSize);
        }

        private float _rotationY;

        public float RotationY
        {
            get => _rotationY;
            set
            {
                var difference = value - _rotationY;

                _rotationY = value;

                foreach (var i in Objects)
                    i.RotationY += difference;
            }
        }

        public void RotateAsGroup(float targetRotationDeg)
        {
            RotationY = targetRotationDeg;

            var rotationDifferenceRad = (targetRotationDeg - RotationY) * Math.PI / 180.0f;

            // The formula used goes counterclockwise - using negative angle to go clockwise
            var sin = (float)Math.Sin(-rotationDifferenceRad);
            var cos = (float)Math.Cos(-rotationDifferenceRad);

            foreach (var i in Objects)
            {
                var distance = i.Position - Position;

                i.SetPosition(new Vector3(
                    distance.X * cos - distance.Z * sin + Position.X,
                    i.Position.Y,
                    distance.X * sin + distance.Z * cos + Position.Z
                ));
            }
        }
    }
}