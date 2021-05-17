using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanEffects : MonoBehaviour
{
    public Material materialToRender;

    private void Start()
    {
        Debug.Log("dont use getcomponent nor create variables in update or fixedupdate; rather, cashe it\nuse fixed update sparingly (only for collisions not even generall fysics)");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, materialToRender);
    }
    
    public void SetMaterial(CelestialBodyManager[] celestialBodyManagers, SunLight sunLight)
    {
        materialToRender = new Material(Shader.Find("Planet/Atmosphere And Ocean"));

        List<Vector4> _PlanetCentre = new List<Vector4>();
        List<Color> _OceanDeep = new List<Color>();
        List<Color> _OceanShallow = new List<Color>();
        List<Color> _SpecularCol = new List<Color>();
        List<float> _OceanRadius = new List<float>();
        List<float> _DepthMultiplier = new List<float>();
        List<float> _AlphaMultiplier = new List<float>();
        List<float> _Smoothness = new List<float>();
        List<float> _SelfGlow = new List<float>();

        List<float> _AtmosphereRadius = new List<float>();
        List<float> _NumInScatteringPoints = new List<float>();
        List<float> _NumOpticalDepthPoints = new List<float>();
        List<float> _Intensity = new List<float>();
        List<Vector4> _ScatteringCoefficients = new List<Vector4>();
        List<float> _ScatteringStrength = new List<float>();
        List<float> _DensityFalloff = new List<float>();


        List<float> _WaveSpeed = new List<float>();
        Texture2D _WaveNormalA = new Texture2D(128, 128);
        Texture2D _WaveNormalB = new Texture2D(128, 128);
        List<float> _WaveNormalScale = new List<float>();
        List<float> _WaveStrength = new List<float>();

        List<CelestialBodyManager> celestialBodyManagersList = new List<CelestialBodyManager>();

        for (int i = 0; i < celestialBodyManagers.Length; i++)
        {
            if (celestialBodyManagers[i].oceanAtmosphere.hasShader)
            {
                /*
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.planetCentre);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.oceanDeep);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.oceanShallow);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.specularCol);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.oceanRadius);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.depthMultiplier);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.alphaMultiplier);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.smoothness);
                Debug.Log(System.Convert.ToSingle(celestialBodyManagers[i].oceanAtmosphere.selfGlow));

                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.atmosphereRadius);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.numInScatteringPoints);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.numOpticalDepthPoints);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.scatteringCoefficients);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.scatteringStrength);
                Debug.Log(celestialBodyManagers[i].oceanAtmosphere.densityFalloff);
                */
                _PlanetCentre.Add(celestialBodyManagers[i].oceanAtmosphere.planetCentre);
                _OceanDeep.Add(celestialBodyManagers[i].oceanAtmosphere.oceanDeep);
                _OceanShallow.Add(celestialBodyManagers[i].oceanAtmosphere.oceanShallow);
                _SpecularCol.Add(celestialBodyManagers[i].oceanAtmosphere.specularCol);
                _OceanRadius.Add(celestialBodyManagers[i].oceanAtmosphere.oceanRadius);
                _DepthMultiplier.Add(celestialBodyManagers[i].oceanAtmosphere.depthMultiplier);
                _AlphaMultiplier.Add(celestialBodyManagers[i].oceanAtmosphere.alphaMultiplier);
                _Smoothness.Add(celestialBodyManagers[i].oceanAtmosphere.smoothness);
                _SelfGlow.Add(System.Convert.ToSingle(celestialBodyManagers[i].oceanAtmosphere.selfGlow));

                _AtmosphereRadius.Add(celestialBodyManagers[i].oceanAtmosphere.atmosphereRadius);
                _NumInScatteringPoints.Add(celestialBodyManagers[i].oceanAtmosphere.numInScatteringPoints);
                _NumOpticalDepthPoints.Add(celestialBodyManagers[i].oceanAtmosphere.numOpticalDepthPoints);
                _ScatteringCoefficients.Add(celestialBodyManagers[i].oceanAtmosphere.scatteringCoefficients);
                _ScatteringStrength.Add(celestialBodyManagers[i].oceanAtmosphere.scatteringStrength);
                _DensityFalloff.Add(celestialBodyManagers[i].oceanAtmosphere.densityFalloff);

                _WaveSpeed.Add(celestialBodyManagers[i].oceanAtmosphere.waveSpeed);
                if (celestialBodyManagers[i].oceanAtmosphere.waveSpeed != 0)
                {
                    _WaveNormalA = celestialBodyManagers[i].oceanAtmosphere.waveNormalA;
                    _WaveNormalA = celestialBodyManagers[i].oceanAtmosphere.waveNormalB;
                }
                _WaveNormalScale.Add(celestialBodyManagers[i].oceanAtmosphere.waveNormalScale);
                _WaveStrength.Add(celestialBodyManagers[i].oceanAtmosphere.waveStrength);

                celestialBodyManagersList.Add(celestialBodyManagers[i]);
            }
        }

        //Debug.Log(_PlanetCentre.Count);

        materialToRender.SetInt("numPlanetsToRender", _PlanetCentre.Count);
        materialToRender.SetVectorArray("planetCentre", _PlanetCentre);
        materialToRender.SetColorArray("oceanDeep", _OceanDeep);
        materialToRender.SetColorArray("oceanShallow", _OceanShallow);
        materialToRender.SetFloatArray("oceanRadius", _OceanRadius);
        materialToRender.SetFloatArray("depthMultiplier", _DepthMultiplier);
        materialToRender.SetFloatArray("alphaMultiplier", _AlphaMultiplier);
        materialToRender.SetFloatArray("smoothness", _Smoothness);
        materialToRender.SetFloatArray("selfGlow", _SelfGlow);
        materialToRender.SetColorArray("specularCol", _SpecularCol);

        materialToRender.SetFloatArray("atmosphereRadius", _AtmosphereRadius);
        materialToRender.SetFloatArray("numInScatteringPoints", _NumInScatteringPoints);
        materialToRender.SetFloatArray("numOpticalDepthPoints", _NumOpticalDepthPoints);
        materialToRender.SetVectorArray("scatteringCoefficients", _ScatteringCoefficients);
        materialToRender.SetFloatArray("scatteringStrength", _ScatteringStrength);
        materialToRender.SetFloatArray("densityFalloff", _DensityFalloff);

        materialToRender.SetFloatArray("waveSpeed", _WaveSpeed);
        materialToRender.SetTexture("waveNormalA", _WaveNormalA);
        materialToRender.SetTexture("waveNormalB", _WaveNormalB);
        materialToRender.SetFloatArray("waveNormalScale", _WaveNormalScale);
        materialToRender.SetFloatArray("waveStrength", _WaveStrength);

        sunLight.SetOceansTransforms(celestialBodyManagersList.ToArray());
    }

    public void ChangeMaterialToRender(Transform transform)
    {
        /*
        Material mat = transform.GetComponent<CelestialBodyManager>().oceanMaterial;
        if (mat != materialToRender)
        {
            materialToRender = mat;
        }
        */
    }

    public void UpdateOceans(Vector4[] sunDirs, Vector4[] planetCentre)
    {
        materialToRender.SetVectorArray("planetCentre", planetCentre);
        materialToRender.SetVectorArray("dirToSun", sunDirs);
    }
}
