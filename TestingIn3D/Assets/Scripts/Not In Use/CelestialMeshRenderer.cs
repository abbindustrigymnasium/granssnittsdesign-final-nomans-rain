using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CelestialMeshRenderer : MonoBehaviour
{
    Mesh mesh;

    private void Start()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void Render(Vector3[] vectors, int[] triangles)
    {
        mesh.Clear();

        mesh.vertices = vectors;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}
