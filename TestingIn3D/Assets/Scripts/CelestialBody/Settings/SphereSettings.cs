using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SphereSettings : ScriptableObject
{
    public enum Resolution { MaxRes, MediumRes, LowRes, customResolution }
    public Resolution resolution;

    [Range(1, 400)]
    public int customResolution = 1;
    public float planetRadius = 1.0f;
    public float mass = 1;
    public Vector3 initialVelocity;
    public Vector3 worldPos;
}
