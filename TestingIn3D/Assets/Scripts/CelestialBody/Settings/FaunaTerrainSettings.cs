using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FaunaTerrainSettings
{
    public enum FaunaType { Trees, Grass };
    public FaunaType faunaType;

    public TreesSettings treesSettings;
    public GrassSettings grassSetting;

    public class SharedSettings
    {
        public bool enabled = true;
        public bool randomizeOnStart;
        public int seed = 1337;
        public int amount = 10;
        public float bias = 1;
        public Vector2 minMaxScale;
        public GameObject fauna;
        public float plantationDepth;
    }

    [System.Serializable]
    public class TreesSettings : SharedSettings
    {
        public Gradient trunkSpectrum;
        public Gradient leafSpectrum;
    }

    [System.Serializable]
    public class GrassSettings : SharedSettings
    {
        public Gradient grassSpectrum;
    }
}
