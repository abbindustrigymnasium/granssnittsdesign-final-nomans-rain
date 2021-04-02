using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaunaProjection
{
    private FaunaSettings faunaSettings;
    private IFaunaFilter[] faunaFilters;
    public static float planetRad;
    private GameObject planetGameObject;

    public FaunaProjection(FaunaTerrainSettings faunaTerrainSettings, FaunaSettings faunaSettings, GameObject planetGameObject, float oceanRadSquared)
    {
        this.faunaSettings = faunaSettings;
        this.planetGameObject = planetGameObject;
        faunaFilters = new IFaunaFilter[faunaSettings.faunaLayers.Length];

        for (int i = 0; i < faunaFilters.Length; i++)
        {
            faunaFilters[i] = FaunaFactory.CreateFauna(faunaSettings.faunaLayers[i].faunaTerrainSettings, planetGameObject, i);
        }

        for (int i = 0; i < faunaFilters.Length; i++)
        {
            for (int q = 0; q < 20; q++)
            {
                faunaFilters[i].PlaceFauna(CelestialBodyManager.vectors[q], q, oceanRadSquared);
            }
        }
    }

    public interface IFaunaFilter
    {
        void PlaceFauna(Vector3[] points, int index, float oceanRadSquared);
    }

    public static class FaunaFactory
    {
        public static IFaunaFilter CreateFauna(FaunaTerrainSettings faunaTerrainSettings, GameObject planetGameObject, int filterIndex)
        {
            switch (faunaTerrainSettings.faunaType)
            {
                case FaunaTerrainSettings.FaunaType.Trees:
                    return new CustomTrees(faunaTerrainSettings.treesSettings, planetGameObject, filterIndex);
            }
            return null;
        }
    }

    public class CustomTrees : IFaunaFilter
    {
        FaunaTerrainSettings.TreesSettings customTrees;
        Tree[] trees;
        GameObject planetGameObject;
        GameObject[] planetGameObjectChildren = new GameObject[20];
        int filterIndex;

        public struct Tree
        {
            public Vector3 point;
        }

        public CustomTrees(FaunaTerrainSettings.TreesSettings customTrees, GameObject planetGameObject, int filterIndex)
        {
            this.customTrees = customTrees;
            this.filterIndex = filterIndex;
            this.planetGameObject = planetGameObject.transform.Find("AllMeshes").gameObject;
            
            for (int i = 0; i < 20; i++)
            {
                if (this.planetGameObject.transform.Find("Mesh_" + i).transform.Find("customTrees_" + filterIndex) == null)
                {
                    Transform parent = this.planetGameObject.transform.Find("Mesh_" + i);
//                    parent.transform.position = new Vector3(0, 0, 0);
                    planetGameObjectChildren[i] = new GameObject();
//                    this.planetGameObject.transform.position = new Vector3(0, 0, 0);
//                    planetGameObjectChildren[i].transform.position = new Vector3(0,0,0);
                    planetGameObjectChildren[i].name = "customTrees_" + filterIndex;
                    planetGameObjectChildren[i].transform.parent = parent;
                }
                else
                {
                    planetGameObjectChildren[i] = this.planetGameObject.transform.Find("Mesh_" + i).transform.Find("customTrees_" + filterIndex).gameObject;
                }
            }
        }

        public void PlaceFauna(Vector3[] points, int index, float oceanRadSquared)
        {
            Random.InitState(customTrees.seed);

            float BiasFunction(float t, float bias)
            {
                float k = Mathf.Pow(1.0f - bias, 3);
                return (t * k) / (t * k - t + 1);
            }

            foreach (Transform child in planetGameObjectChildren[index].transform)
            {
                Object.Destroy(child.gameObject);
            }
            if (customTrees.enabled)
            {
                for (int i = 0; i < customTrees.treeAmount; i++)
                {
                    Vector3 point = points[Random.Range(0, points.Length)];

                    if (customTrees.tree.name != "palm" && point.sqrMagnitude < oceanRadSquared + 0.3)
                    {
                        continue;
                    } else if ((customTrees.tree.name == "palm" && point.sqrMagnitude < oceanRadSquared) || (customTrees.tree.name == "palm" && point.sqrMagnitude > oceanRadSquared + 0.3))
                    {
                        continue;
                    }

                    float rand1to0 = Random.Range(0.0f, 1.0f);
                    float scale = BiasFunction(rand1to0, customTrees.bias) * customTrees.minMaxScale.y;
                    scale = Mathf.Clamp(scale, customTrees.minMaxScale.x, customTrees.minMaxScale.y);
                    Vector3 scaleAll = new Vector3(scale, scale, scale);

                    GameObject instancedObj = GameObject.Instantiate(customTrees.tree, point + planetGameObject.transform.position - point.normalized * customTrees.plantationDepth, Quaternion.LookRotation(point.normalized, Vector3.up)) as GameObject;
                    instancedObj.transform.parent = planetGameObjectChildren[index].transform;
                    instancedObj.transform.localScale = scaleAll;

                    // create new material?
                    if (customTrees.tree.name != "palm")
                    {
                        instancedObj.transform.Find("leaf").gameObject.GetComponent<MeshRenderer>().material.SetColor("_LeafColor", new Color(0.08f, Random.Range(0.25f, 0.5f), 0.12f, 1));
                    }
                    //                    instancedObj.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
                    //                    instancedObj.transform.Find("leaf").tag = "lol";
                    //                    instancedObj.transform.Find("leaf").GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
                    //                    planetGameObjectChildren[index].transform.Find("leaf").GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1);
                }
            }
        }
    }
}
