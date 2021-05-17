using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanEffect : MonoBehaviour
{
    public Material oceanMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, oceanMaterial);
    }
}
