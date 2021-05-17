using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class StarSettings : ScriptableObject
{
    public int seed = 1337;
    public int numStars = 500;
    public float distanceRadius = 100f;
    public Vector2 starRadiusMinMax = new Vector2(1f, 2f);
    public float starRadiusDistribution = 1f;
    public Material starMaterial;
    public Gradient spectrum;
}
