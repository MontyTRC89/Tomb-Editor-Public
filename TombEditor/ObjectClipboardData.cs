using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombEditor
{
    [Serializable]
    public class ObjectClipboardData
    {
        private readonly byte[] _data;
        private readonly string _levelPath;

        public ObjectClipboardData(Editor editor)
        {
            // Write data
            _levelPath = editor.Level.Settings.LevelFilePath ?? "";
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(stream);
                var objectInstances = new List<ObjectInstance> { editor.SelectedObject };
                if (editor.SelectedObject is ObjectGroup og)
                {
                    objectInstances.AddRange(og);
                }
                Prj2Writer.SaveToPrj2OnlyObjects(stream, editor.Level, objectInstances);
                _data = stream.GetBuffer();
            }
        }

        public Prj2Loader.LoadedObjects CreateObjects()
        {
            using (var stream = new MemoryStream(_data, false))
            {
                var loadedObjects = Prj2Loader.LoadFromPrj2OnlyObjects(_levelPath, stream,
                    new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });
                return loadedObjects;
            }
        }

        public ObjectInstance MergeGetSingleObject(Editor editor)
        {
            LevelSettings newLevelSettings = editor.Level.Settings.Clone();
            editor.UpdateLevelSettings(newLevelSettings);

            Prj2Loader.LoadedObjects loadedObjects = CreateObjects();

            var unpackedObjects = loadedObjects.Objects
                .Select(obj =>
                {
                    obj.CopyDependentLevelSettings(
                        new Room.CopyDependentLevelSettingsArgs(null, newLevelSettings, loadedObjects.Settings, true));

                    // A little workaround to detect script id collisions already
                    if (obj is IHasScriptID)
                    {
                        Room testRoom = editor.SelectedRoom;
                        try
                        {
                            testRoom.AddObject(editor.Level, obj);
                            testRoom.RemoveObject(editor.Level, obj);
                        }
                        catch (ScriptIdCollisionException)
                        {
                            ((IHasScriptID)obj).ScriptId = null;
                        }
                    }

                    return obj;
                })
                .ToList();

            if (unpackedObjects.Count <= 1)
            {
                return unpackedObjects.FirstOrDefault();
            }
            else
            {
                var unpackedChildren = unpackedObjects.OfType<ItemInstance>().ToList();

                return new ObjectGroup(unpackedChildren);
            }
        }
    }
}