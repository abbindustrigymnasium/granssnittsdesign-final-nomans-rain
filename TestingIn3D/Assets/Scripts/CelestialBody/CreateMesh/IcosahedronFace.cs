using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcosahedronFace
{
    private List<Vector3> vectorsDetailedSingle = new List<Vector3>();
    private List<int> trianglesDetailedSingle = new List<int>();

    public void CreateNewDetailedFace(Vector3 v0, Vector3 v1, Vector3 v2, int index, int r) {
        vectorsDetailedSingle.Clear();
        trianglesDetailedSingle.Clear();

        Vector3 distance = new Vector3(
            ((r - 1) * (v0.x)) / (r) + (v1.x) / (r),
            ((r - 1) * (v0.y)) / (r) + (v1.y) / (r),
            ((r - 1) * (v0.z)) / (r) + (v1.z) / (r)
        );
        Vector3 distanceMirrored = new Vector3(
            ((r - 1) * (v1.x)) / (r) + (v2.x) / (r),
            ((r - 1) * (v1.y)) / (r) + (v2.y) / (r),
            ((r - 1) * (v1.z)) / (r) + (v2.z) / (r)
        );

        Vector3 distanceDiff = new Vector3(
            distance.x - v0.x,
            distance.y - v0.y,
            distance.z - v0.z
        );
        Vector3 distanceMirroredDiff = new Vector3(
            distanceMirrored.x - v1.x,
            distanceMirrored.y - v1.y,
            distanceMirrored.z - v1.z
        );
        
        for (int y = 0; y < r; y++)
        {
            Vector3 currentPoint = v0 + distanceDiff * y;

            for (int x = 0; x < r - y; x++)
            {
                for (int q = 0; q < 3; q++)
                {
                    trianglesDetailedSingle.Add((vectorsDetailedSingle.Count) + q + (index));
                }

                vectorsDetailedSingle.Add(currentPoint);
                currentPoint += distanceDiff;
                vectorsDetailedSingle.Add(currentPoint);
                currentPoint += distanceMirroredDiff;
                vectorsDetailedSingle.Add(currentPoint);

                if (x != r - y - 1)
                {
                    trianglesDetailedSingle.Add((vectorsDetailedSingle.Count - 1) + (index) + 1);
                    trianglesDetailedSingle.Add((vectorsDetailedSingle.Count - 1) + (index) - 1);
                    trianglesDetailedSingle.Add((vectorsDetailedSingle.Count - 1) + (index) + 2);
                }
            }
        }
    }

    public Vector3[] GetVectors()
    {
        return vectorsDetailedSingle.ToArray();
    }
        
    public int[] GetTriangles()
    {
        return trianglesDetailedSingle.ToArray();
    }
}
