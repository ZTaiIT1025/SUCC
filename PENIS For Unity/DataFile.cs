﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PENIS
{
    public class DataFile
    {
        /// <summary> The absolute path of the file this object corresponds to. </summary>
        public readonly string FilePath;

        /// <summary> creates a new TextFile object, which corresponds to a text file in storage and can be used for easy reference. </summary>
        /// <param name="path"> the path of the file. Can be either absolute or relative to the default path. </param>
        public DataFile(string path, string DefaultFile = null)
        {
            path = Utilities.AbsolutePath(path);
            path = Path.ChangeExtension(path, Utilities.FileExtension);
            this.FilePath = path;

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (!File.Exists(path))
            {
                if (DefaultFile != null)
                {
                    var defaultFile = Resources.Load<TextAsset>(DefaultFile);
                    if (defaultFile == null)
                        throw new Exception("The default file you specified doesn't exist in Resources :(");

                    File.WriteAllBytes(path, defaultFile.bytes);
                    Resources.UnloadAsset(defaultFile);
                }
                else
                {
                    File.Create(path).Close();
                }
            }

            this.ReloadAllData();
        }

        public List<Line> TopLevelLines { get; private set; }
        public Dictionary<string, KeyNode> TopLevelNodes { get; private set; }


        /// <summary> Reloads the data stored on disk into this object. </summary>
        public void ReloadAllData()
        {
            string[] lines = File.ReadAllLines(FilePath);
            var data = DataConverter.DataStructureFromPENIS(lines);
            TopLevelLines = data.Item1;
            TopLevelNodes = data.Item2;
        }

        /// <summary> Serializes the data in this object to the file on disk. </summary>
        public void SaveAllData()
        {
            string PENIS = DataConverter.PENISFromDataStructure(TopLevelLines);
            File.WriteAllText(FilePath, PENIS);
        }


        /// <summary> get some data from the file, saving a new value if the data does not exist </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="DefaultValue"> if the key does not exist in the file, this value is saved there and returned </param>
        public T Get<T>(string key, T DefaultValue)
        {
            if (!KeyExists(key))
            {
                Set<T>(key, DefaultValue);
                return DefaultValue;
            }

            var node = TopLevelNodes[key];
            return (T)NodeManager.GetNodeData(node, typeof(T));
        }

        /// <summary> save data to the file </summary>
        /// <param name="key"> what the data is labeled as within the file </param>
        /// <param name="value"> the value to save </param>
        public void Set<T>(string key, T value)
        {
            if (!KeyExists(key))
            {
                if (string.IsNullOrEmpty(key))
                    throw new FormatException("PENIS keys must contain at least one character");
                if (key[0] == '-')
                    throw new FormatException("PENIS keys may not begin with the character '-'");
                if (key.Contains(":"))
                    throw new FormatException("PENIS keys may not contain the character ':'");
                if (key.Contains("#"))
                    throw new FormatException("PENIS keys may not contain the character '#'");
                if (key.Contains("\n"))
                    throw new FormatException("PENIS keys cannot contain a newline");
                if (key[0] == ' ' || key[key.Length - 1] == ' ')
                    throw new FormatException("PENIS keys may not start of end with a space");

                var newnode = new KeyNode() { RawText = key + ':' };
                TopLevelNodes.Add(key, newnode);
                TopLevelLines.Add(newnode);
            }
            
            var node = TopLevelNodes[key];
            NodeManager.SetNodeData(node, value, typeof(T));
        }

        /// <summary> whether a top-level key exists in the file </summary>
        public bool KeyExists(string key)
        {
            return TopLevelNodes.ContainsKey(key);
        }

        /// <summary> Remove a top-level key and all its data from the file </summary>
        public void DeleteKey(string key)
        {
            if (!KeyExists(key))
                return;

            Node node = TopLevelNodes[key];
            TopLevelNodes.Remove(key);
            TopLevelLines.Remove(node);
        }
    }
}