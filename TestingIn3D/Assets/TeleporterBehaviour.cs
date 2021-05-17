using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeleporterBehaviour : MonoBehaviour
{
    public GameObject interactTextObject;
    public GameObject objectiveTextObject;
    public GameObject celestialBodies;
    public GameObject player;
    private CelestialBodiesInOrbit celestialBodiesScript;
    private Text objectiveText;
    private Text interactText;
    private bool playerInRange = false;
    public float teleporterTimer = 90;
    private float currentTeleporterTimer;
    private bool teleporterPressed = false;

    void Start()
    {
        interactTextObject = GameObject.Find("InteractText");
        objectiveTextObject = GameObject.Find("ObjectiveText");
        celestialBodies = GameObject.Find("Celestial Bodies");
        player = GameObject.Find("Player");
        currentTeleporterTimer = teleporterTimer;

        interactText = interactTextObject.GetComponent<Text>();
        objectiveText = objectiveTextObject.GetComponent<Text>();
        celestialBodiesScript = celestialBodies.GetComponent<CelestialBodiesInOrbit>();
    }

    private void Update()
    {
        if (currentTeleporterTimer <= 0 || currentTeleporterTimer >= teleporterTimer)
        {
            interactText.enabled = playerInRange;
            interactText.text = "Press 'E' to interact";
            if (teleporterPressed)
            {
                objectiveText.text = "Objective: Proceed through the teleporter";
            }
            else
            {
                objectiveText.text = "Objective: Find the teleporter";
            }
        }
        else
        {
            objectiveText.text = "Objective: Survive for " + Mathf.Ceil(currentTeleporterTimer) + " seconds";
            interactText.enabled = playerInRange;
            interactText.text = "Survive.";
        }
        if(Input.GetKeyDown(KeyCode.E) && playerInRange)
        {
            if (!teleporterPressed) { 
                teleporterPressed = true;
            }
            else if (currentTeleporterTimer <= 0)
            {
                player.transform.position = transform.TransformPoint(new Vector3(0, 11.5f, 0));

                bool has_itterated_parrent = false;
                foreach (Transform child in celestialBodies.transform)
                {
                    if (child == celestialBodies.transform.GetChild(4))
                    {
                        celestialBodiesScript.currentPlanetTransform = celestialBodies.transform.GetChild(0);
                        break;
                    }

                    if (transform.parent == child)
                    {
                        has_itterated_parrent = true;
                    }
                    else if (has_itterated_parrent)
                    {
                        celestialBodiesScript.currentPlanetTransform = child;
                        break;
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentTeleporterTimer >= 0 && teleporterPressed)
        {
            currentTeleporterTimer -= Time.deltaTime;
        }

    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.name == "Player" )
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.name == "Player")
        {
            playerInRange = false;
        }
    }
}
