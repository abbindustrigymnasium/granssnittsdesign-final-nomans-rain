using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProcedularAnimation : MonoBehaviour
{
    [SerializeField]
    private Transform bodyOffset;

    [SerializeField]
    private Leg[] legs = new Leg[4];

    public LayerMask ground;

    [System.Serializable]
    public struct Leg
    {
        // movement
        public Transform fot;
        public Vector3 fotPos;
        public float maxSqrtDist;

        // ray
        public int colliderLayerMask;
        public Transform targetRayTransform;
        //public Vector3 targetRayDirection;
        public RaycastHit raycastHit;
    }

    // TODO:
    // newly added script (FastIKFabric, inte min) pole doesnt rotate upper bone (it should)
    // move legs smoother (over time)
    // only move leg when opposite leg is grounded (i.e not moving)
    // add rotation to body acorting to difference between left and right leg height

    // JANKY AF IDK WHY
    // apply enemycontroller gravity and rotation
    // apply enemycontroller movement

    private void Start()
    {

    }

    private void Update()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            if (CheckTargetFotdistance(ref legs[i]))
            {
                //legs[i].oldFotPos = legs[i].fotPos;
                FixFotToTarget(ref legs[i]);
            }
            UpdateLegPos(ref legs[i]);
        }

        UpdateBodyPos();
    }

    private void UpdateRaycast(ref Leg leg)
    {
        Physics.Raycast(leg.targetRayTransform.position, -transform.up, out leg.raycastHit, 10f, 1 << leg.colliderLayerMask);
    }
    private bool CheckTargetFotdistance(ref Leg leg)
    {
        UpdateRaycast(ref leg);
        return (leg.raycastHit.point - leg.fotPos).sqrMagnitude >= leg.maxSqrtDist && leg.raycastHit.collider != null;
    }
    private void UpdateLegPos(ref Leg leg)
    {
        //leg.sleerp = Mathf.Clamp01(leg.sleerp + Time.deltaTime / leg.sleerpDuration);
        //leg.fot.position = Vector3.Lerp(leg.oldFotPos, leg.fotPos, leg.sleerp);

        leg.fot.position = leg.fotPos;
    }
    private void FixFotToTarget(ref Leg leg)
    {
        //Debug.Log("new fotPos");
        leg.fotPos = leg.raycastHit.point;
    }

    private void UpdateBodyPos()
    {
        // dependent on normal of pos
        return;

        Vector3 posSum = Vector3.zero;
        for (int i = 0; i < legs.Length; i++)
        {
            posSum += legs[i].fot.position;
        }
        transform.position = posSum * 0.25f + bodyOffset.localPosition * 0.03f;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            Gizmos.DrawRay(legs[i].targetRayTransform.position, -transform.up * 10f);
        }
    }
}
