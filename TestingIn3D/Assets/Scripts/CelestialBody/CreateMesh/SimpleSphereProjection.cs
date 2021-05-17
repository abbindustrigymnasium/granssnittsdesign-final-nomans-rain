using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSphereProjection
{
    public SimpleSphereProjection()
    {
        for (int q = 0; q < 20; q++)
        {
            for (int i = 0; i < CelestialBodyManager.vectors[q].Length; i++)
            {
                CelestialBodyManager.vectors[q][i] = PointToRadius(CelestialBodyManager.vectors[q][i]);
            }
        }
    }

    private Vector3 PointToRadius(Vector3 point)
    {
        return point.normalized;
    }
}
