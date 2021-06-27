using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGameObject : MonoBehaviour
{
    [SerializeField]
    private GameObject obj;
    void Update()
    {
        Vector3 pos = new Vector3(obj.transform.position.x, obj.transform.position.y, transform.position.z);
        transform.position = pos;
    }
}
