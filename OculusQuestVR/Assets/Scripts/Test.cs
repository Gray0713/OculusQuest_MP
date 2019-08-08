using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject lh;

    // Start is called before the first frame update
    void Start()
    {
        transform.SetParent(lh.transform);
    }
}
