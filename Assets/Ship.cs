using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField]
    private float launchForceMultiplier = 1f;
    [SerializeField]
    private float gravitationalConstant = 0.6f;
    private bool sailing = false;
    private bool aiming = false;
    private Vector2 aimPos;
    private Vector2 mousePos;
    Rigidbody2D myBody;
    private GameObject[] gravitySources;
    void Start()
    {
        myBody = GetComponent<Rigidbody2D>();
        gravitySources = GameObject.FindGameObjectsWithTag("Gravity Source");
    }
    void Update()
    {
        if (!sailing) return;

        Vector2 v = myBody.velocity;
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        myBody.SetRotation(angle - 90);
    }
    void FixedUpdate()
    {
        if (!sailing) return;

        foreach (GameObject obj in gravitySources)
        {
            Rigidbody2D otherBody = obj.GetComponent<Rigidbody2D>();
            Vector2 gravity = obj.transform.position - transform.position;
            float r = gravity.magnitude;
            gravity.Normalize();
            gravity *= (gravitationalConstant * otherBody.mass * myBody.mass) / (r * r);
            myBody.AddForce(gravity);
        }
    }
    void OnMouseDown()
    {
        aiming = true;
        aimPos = transform.position;
    }
    void OnMouseDrag()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    void OnMouseUp()
    {
        aiming = false;
        sailing = true;
        myBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        myBody.AddForce((aimPos - mousePos) * launchForceMultiplier, ForceMode2D.Impulse);
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        myBody.constraints = RigidbodyConstraints2D.FreezeAll;
        myBody.position = Vector2.zero;
        myBody.rotation = 0;
        sailing = false;
    }
}
