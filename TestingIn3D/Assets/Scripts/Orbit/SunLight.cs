using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunLight : MonoBehaviour
{
    [HideInInspector]
    public Transform followPlanet;
    private Transform[] dirLight;
    private Transform[] planetsPos;
    private int layerStart = 10;
    private int planetAmount;
    //private Transform stars;

    [SerializeField]
    private string[] layerMasks;

    //private OceanEffects oceanEffects;
    //public int[] indecees;

    public CelestialBodyManager[] celestialBodyManagers;

    private StarScript starScript;

    private void Awake()
    {
        starScript = GameObject.Find("Stars").GetComponent<StarScript>();
        planetAmount = GameObject.Find("Celestial Bodies").transform.childCount - 1;
        //oceanEffects = GameObject.Find("Main Camera").GetComponent<OceanEffects>();
        dirLight = new Transform[planetAmount];
        planetsPos = new Transform[planetAmount];
        foreach (Transform child in GameObject.Find("Celestial Bodies").transform)
        {
            if (child != transform.parent)
            {
                int index = layerStart - 10;
                child.GetComponent<CelestialBodyManager>().layer = layerStart;

                GameObject directionalLightComponent = new GameObject();
                directionalLightComponent.AddComponent(typeof(Light));
                directionalLightComponent.GetComponent<Light>().type = LightType.Directional;
                directionalLightComponent.GetComponent<Light>().cullingMask =  1 << layerStart;
                directionalLightComponent.name = "Directional_Light_" + (index);
                directionalLightComponent.transform.parent = transform;
                directionalLightComponent.gameObject.layer = layerStart;

                dirLight[index] = directionalLightComponent.transform;
                planetsPos[index] = child;

                layerStart++;
            }
        }
    }

    public void SetOceansTransforms(CelestialBodyManager[] celestialBodyManagersNew)
    {
        celestialBodyManagers = celestialBodyManagersNew;
    }

    private void Update()
    {
        starScript.RotateStars(transform.position.normalized);

        for (int i = 0; i < dirLight.Length; i++)
        {
            dirLight[i].rotation = Quaternion.LookRotation(planetsPos[i].position - transform.position, planetsPos[i].forward);
            if (followPlanet == planetsPos[i])
            {
                transform.rotation = dirLight[i].rotation;
                //stars.rotation = dirLight[i].rotation;
            }
        }
        /*
        if (celestialBodyManagers == null)
        {
            return;
        }

        
        // set ocean centre and dirtosun
        Vector4[] sunDirs = new Vector4[celestialBodyManagers.Length];
        Vector4[] planetCentre = new Vector4[celestialBodyManagers.Length];
        for (int i = 0; i < celestialBodyManagers.Length; i++)
        {
            sunDirs[i] = -(Quaternion.LookRotation(celestialBodyManagers[i].transform.position - transform.position, celestialBodyManagers[i].transform.forward) * Vector3.forward);
            planetCentre[i] = celestialBodyManagers[i].transform.position;
        }

        oceanEffects.UpdateOceans(sunDirs, planetCentre);
        */
    }

    public void SetCullingMask(Transform planet)
    {
        followPlanet = planet;

        for (int i = 0; i < planetAmount; i++)
        {
            Light light = dirLight[i].GetComponent<Light>();
            if (planetsPos[i] == planet)
            {
                for (int q = 0; q < layerMasks.Length; q++)
                {
                    light.cullingMask |= 1 << LayerMask.NameToLayer(layerMasks[q]);
                }
            }
            else
            {
                for (int q = 0; q < layerMasks.Length; q++)
                {
                    light.cullingMask &= ~(1 << LayerMask.NameToLayer(layerMasks[q]));
                }
            }
        }
    }
}
