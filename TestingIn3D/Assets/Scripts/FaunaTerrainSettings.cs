using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FaunaTerrainSettings
{
    public enum FaunaType { Trees };
    public FaunaType faunaType;

    public TreesSettings treesSettings;

    [System.Serializable]
    public class TreesSettings
    {
        public bool enabled = true;
        public int seed = 1337;
        public int treeAmount = 10;
        public float bias = 1;
        public Vector2 minMaxScale;
        public GameObject tree;
        public float plantationDepth;
    }
}
