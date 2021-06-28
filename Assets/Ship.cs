using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField]
    private GameObject sail;
    [SerializeField]
    private float launchForceMultiplier = 2.5f;
    [SerializeField]
    private float gravitationalConstant = 45f;
    [SerializeField]
    private float minTimeScaleDistance = 0f;
    [SerializeField]
    private float maxTimeScaleDistance = 3f;
    [SerializeField]
    private float minTimeScale = 0.5f;
    [SerializeField]
    private float maxTimeScale = 1f;
    [SerializeField]
    private float dampWaitTime;

    private bool sailing = false;
    private bool aiming = false;
    private Vector2 aimPos;
    private Vector2 mousePos;
    Rigidbody2D myBody;
    private GameObject[] gravitySources;
    private float fixedDT;
    private GameObject crashLandedOn;
    private Vector2 lastVelocity = Vector2.zero;
    private bool crashed = false;
    private float closestPlanetDistance = float.PositiveInfinity;
    private Coroutine dampRoutine;

    void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
        gravitySources = GameObject.FindGameObjectsWithTag("Gravity Source");
        fixedDT = Time.fixedDeltaTime;
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Reset();
        }
    }

    void FixedUpdate()
    {
        if (!sailing) return;

        if (!crashed)
        {
            closestPlanetDistance = float.PositiveInfinity;
            foreach (GameObject obj in gravitySources)
            {
                Rigidbody2D otherBody = obj.GetComponent<Rigidbody2D>();

                Vector2 closestPoint = otherBody.ClosestPoint(transform.position);
                float distance = Vector2.Distance(closestPoint, transform.position);

                if (distance < closestPlanetDistance)
                    closestPlanetDistance = distance;

                myBody.AddForce(GetForce(obj, otherBody));
            }

            ScaleTime(closestPlanetDistance);

            Vector2 v = myBody.velocity;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

            myBody.MoveRotation(angle - 90f);

            Vector2 a = v - lastVelocity;
            angle = Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg;
            sail.transform.eulerAngles = new Vector3(sail.transform.eulerAngles.x, sail.transform.eulerAngles.y, angle - 90f);

            lastVelocity = myBody.velocity;
        }
        else
        {
            Rigidbody2D otherBody = crashLandedOn.GetComponent<Rigidbody2D>();
            myBody.AddForce(GetForce(crashLandedOn, otherBody));
            ScaleTime(float.PositiveInfinity);
        }
    }



    private void ScaleTime(float distance)
    {
        float t = Mathf.InverseLerp(minTimeScaleDistance, maxTimeScaleDistance, distance);
        t = Mathf.SmoothStep(minTimeScale, maxTimeScale, t);
        Time.timeScale = t;
        Time.fixedDeltaTime = fixedDT * t;
    }

    Vector2 GetForce(GameObject obj, Rigidbody2D otherBody)
    {
        Vector2 gravity = obj.transform.position - transform.position;
        float r = gravity.magnitude;
        gravity.Normalize();
        gravity *= (gravitationalConstant * otherBody.mass * myBody.mass) / (r * r);
        return gravity;
    }

    void Reset()
    {
        myBody.constraints = RigidbodyConstraints2D.FreezeAll;
        myBody.position = Vector2.zero;
        myBody.rotation = 0f;
        sailing = false;
        crashed = false;
        crashLandedOn = null;
        lastVelocity = Vector2.zero;
        myBody.drag = 0f;
        myBody.angularDrag = 0f;
    }

    void OnMouseDown()
    {
        if (sailing) return;

        aiming = true;
        aimPos = transform.position;
    }

    void OnMouseDrag()
    {
        if (sailing) return;

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void OnMouseUp()
    {
        if (sailing) return;

        aiming = false;
        sailing = true;
        myBody.constraints = RigidbodyConstraints2D.None;
        myBody.AddForce((aimPos - mousePos) * launchForceMultiplier, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        crashed = true;
        crashLandedOn = col.gameObject;
        dampRoutine = StartCoroutine(DampRoutine());
    }
    void OnCollisionExit2D(Collision2D collision)
    {
        StopCoroutine(dampRoutine);
    }

    IEnumerator DampRoutine()
    {
        Debug.Log("Starting");
        yield return new WaitForSeconds(dampWaitTime);
        Debug.Log("Finishing");
        myBody.drag = 0.1f;
        myBody.angularDrag = 5f;
    }
}
