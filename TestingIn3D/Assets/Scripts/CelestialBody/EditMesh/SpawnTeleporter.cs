using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTeleporter
{
    private float oceanRadSquared;
    private float plantationDepth = 0.1f;

    public SpawnTeleporter(float oceanRadSquared, int layer, Transform planetTransform, Vector3[][] vectors)
    {
        this.oceanRadSquared = oceanRadSquared;
        Random.InitState(System.DateTime.Now.Millisecond);

        while (true)
        {
            int meshIndex = Random.Range(0, 20);
            int pointIndex = Random.Range(0, vectors[meshIndex].Length);
            Vector3 point = vectors[meshIndex][pointIndex];

            if (point.sqrMagnitude < oceanRadSquared)
            {
                continue;
            }

            if (planetTransform.Find("TeleporterRange(Clone)"))
            {
                GameObject.Destroy(planetTransform.Find("TeleporterRange(Clone)").gameObject);
            }
            GameObject teleporter = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Interactables/TeleporterRange"));
            teleporter.transform.parent = planetTransform;
            teleporter.transform.position = Vector3.zero;
            teleporter.transform.localPosition = Vector3.zero;
            teleporter.transform.position += point - (point.normalized * plantationDepth);

            teleporter.transform.rotation = Quaternion.LookRotation(point.normalized, Vector3.up);
            teleporter.transform.Find("Teleporter").gameObject.layer = layer;
            teleporter.GetComponent<TeleporterBehaviour>().enabled = false;
            break;
        }
    }
}
