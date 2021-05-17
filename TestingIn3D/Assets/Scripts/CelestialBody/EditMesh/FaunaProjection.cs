using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaunaProjection
{
    private FaunaSettings faunaSettings;
    private IFaunaFilter[] faunaFilters;
    public static float planetRad;
    private GameObject planetGameObject;
    private Vector3[] concatinatedPoints;

    public FaunaProjection(FaunaTerrainSettings faunaTerrainSettings, FaunaSettings faunaSettings, GameObject planetGameObject, float oceanRadSquared, int layer)
    {
        this.faunaSettings = faunaSettings;
        this.planetGameObject = planetGameObject;
        faunaFilters = new IFaunaFilter[faunaSettings.faunaLayers.Length];

        for (int i = 0; i < faunaFilters.Length; i++)
        {
            faunaFilters[i] = FaunaFactory.CreateFauna(faunaSettings.faunaLayers[i].faunaTerrainSettings, planetGameObject, i);
        }

        if (!faunaSettings.enabled)
        {
            return;
        }

        Vector3[][] copiedPoints = new Vector3[20][];
        for (int i = 0; i < 20; i++)
        {
            copiedPoints[i] = (Vector3[])CelestialBodyManager.vectors[i].Clone();
        }

        for (int i = 0; i < faunaFilters.Length; i++)
        {
            faunaFilters[i].PlaceFauna(copiedPoints, oceanRadSquared, layer, faunaSettings.enabled);
        }
    }

    public interface IFaunaFilter
    {
        void PlaceFauna(Vector3[][] points, float oceanRadSquared, int layer, bool enabled);
    }

    public static class FaunaFactory
    {
        public static IFaunaFilter CreateFauna(FaunaTerrainSettings faunaTerrainSettings, GameObject planetGameObject, int filterIndex)
        {
            switch (faunaTerrainSettings.faunaType)
            {
                case FaunaTerrainSettings.FaunaType.Trees:
                    return new CustomTrees(faunaTerrainSettings.treesSettings, planetGameObject, filterIndex);
                /*case FaunaTerrainSettings.FaunaType.Grass:
                    return new CustomGrass(faunaTerrainSettings.grassSetting, planetGameObject, filterIndex);*/
            }
            return null;
        }
    }

    public class CustomTrees : IFaunaFilter
    {
        FaunaTerrainSettings.TreesSettings customTrees;
        GameObject planetGameObject;
        GameObject[] planetGameObjectChildren = new GameObject[20];
        int filterIndex;
        int dateTimeSeed = 0;

        public CustomTrees(FaunaTerrainSettings.TreesSettings customTrees, GameObject planetGameObject, int filterIndex)
        {
            this.customTrees = customTrees;
            this.filterIndex = filterIndex;
            this.planetGameObject = planetGameObject.transform.Find("AllMeshes").gameObject;
            dateTimeSeed = System.DateTime.Now.Millisecond;

            for (int i = 0; i < 20; i++)
            {
                if (this.planetGameObject.transform.Find("Mesh_" + i).transform.Find("customTrees_" + filterIndex) == null)
                {
                    Transform parent = this.planetGameObject.transform.Find("Mesh_" + i);
                    planetGameObjectChildren[i] = new GameObject();
                    planetGameObjectChildren[i].name = "customTrees_" + filterIndex;
                    planetGameObjectChildren[i].transform.parent = parent;
                    planetGameObjectChildren[i].transform.localPosition = Vector3.zero;
                }
                else
                {
                    Object.Destroy(this.planetGameObject.transform.Find("Mesh_" + i).transform.Find("customTrees_" + filterIndex).gameObject);

                    Transform parent = this.planetGameObject.transform.Find("Mesh_" + i);
                    planetGameObjectChildren[i] = new GameObject();
                    planetGameObjectChildren[i].name = "customTrees_" + filterIndex;
                    planetGameObjectChildren[i].transform.parent = parent;
                    planetGameObjectChildren[i].transform.localPosition = Vector3.zero;

                    //planetGameObjectChildren[i] = this.planetGameObject.transform.Find("Mesh_" + i).transform.Find("customTrees_" + filterIndex).gameObject;
                }
            }
        }

        public void PlaceFauna(Vector3[][] points, float oceanRadSquared, int layer, bool enabled)
        {
            /*
            for (int i = 0; i < 20; i++)
            {
                foreach (Transform child in planetGameObjectChildren[i].transform)
                {
                    Object.Destroy(child.gameObject);
                }
            }
            */

            if (!customTrees.enabled || !enabled)
            {
                return;
            }

            Random.InitState(customTrees.randomizeOnStart ? dateTimeSeed : customTrees.seed);

            float BiasFunction(float t, float bias)
            {
                float k = Mathf.Pow(1.0f - bias, 3);
                return (t * k) / (t * k - t + 1);
            }

            for (int i = 0; i < customTrees.amount; i++)
            {
                int meshIndex = Random.Range(0, 20);
                int pointIndex = Random.Range(0, points[meshIndex].Length);
                Vector3 point = points[meshIndex][pointIndex];
                
                if (point.sqrMagnitude < oceanRadSquared || point == Vector3.zero)
                {
                    continue;
                }

                points[meshIndex][pointIndex] = Vector3.zero;

                float rand1to0 = Random.Range(0.0f, 1.0f);
                float scale = BiasFunction(rand1to0, customTrees.bias) * customTrees.minMaxScale.y;
                scale = Mathf.Clamp(scale, customTrees.minMaxScale.x, customTrees.minMaxScale.y);
                Vector3 scaleAll = new Vector3(scale, scale, scale);

                GameObject instancedObj = GameObject.Instantiate(customTrees.fauna);
                instancedObj.transform.localPosition = point + planetGameObject.transform.position - (point.normalized * customTrees.plantationDepth);
                instancedObj.transform.localRotation = Quaternion.LookRotation(point.normalized, Vector3.up);
                instancedObj.layer = layer;
                instancedObj.transform.Find("leaf").gameObject.layer = layer;
                instancedObj.transform.parent = planetGameObjectChildren[meshIndex].transform;
                instancedObj.transform.localScale = scaleAll;

                Color randTrunkColor = customTrees.trunkSpectrum.Evaluate(Random.Range(0f, 1f));
                instancedObj.GetComponent<MeshRenderer>().material.SetColor("_TrunkColor", randTrunkColor);

                Color randLeafColor = customTrees.leafSpectrum.Evaluate(Random.Range(0f, 1f));
                instancedObj.transform.Find("leaf").gameObject.GetComponent<MeshRenderer>().material.SetColor("_LeafColor", randLeafColor);
            }
        }
    }
}
