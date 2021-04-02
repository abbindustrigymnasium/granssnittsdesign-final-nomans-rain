using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class FaunaSettings : ScriptableObject
{
    public FaunaLayer[] faunaLayers;

    [System.Serializable]
    public class FaunaLayer
    {
        public FaunaTerrainSettings faunaTerrainSettings;
    }
}
