using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateDetailedMesh
{
    private SphereSettings sphereSettings;

    private CreateSampleIcosahedron createSampleIcosahedron = new CreateSampleIcosahedron();
    private IcosahedronFace icosahedronFace = new IcosahedronFace();

    private Vector3[] vectorsSimple;
    private int[] trianglesSimple;

    private Vector3[][] vectorsDetailed = new Vector3[20][];
    private int[][] trianglesDetailed = new int[20][];

    private int resolution;

    public CreateDetailedMesh(SphereSettings sphereSettings)
    {
        this.sphereSettings = sphereSettings;

        vectorsSimple = createSampleIcosahedron.getVectors();
        trianglesSimple = createSampleIcosahedron.getTriangles();

        CreateDetailedTriangles();
    }

    public void CreateDetailedTriangles()
    {
        for (int i = 0; i < 20; i++)
        {
            int q = i * 3;
            Vector3 v0 = vectorsSimple[trianglesSimple[q]];
            Vector3 v1 = vectorsSimple[trianglesSimple[q + 1]];
            Vector3 v2 = vectorsSimple[trianglesSimple[q + 2]];

            resolution = sphereSettings.resolution; // change this r dependent on how far away you are and if you are able to see this face

            icosahedronFace.CreateNewDetailedFace(v0, v1, v2, 0, resolution);

            vectorsDetailed[i] = icosahedronFace.GetVectors();
            trianglesDetailed[i] = icosahedronFace.GetTriangles();
        }
    }

    public Vector3[][] GetVectors()
    {
        return vectorsDetailed;
    }

    public int[][] GetTriangles()
    {
        return trianglesDetailed;
    }
}
