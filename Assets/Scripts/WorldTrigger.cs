using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTrigger : MonoBehaviour
{
    Main main;

    private void Start()
    {
        main = FindObjectOfType<Main>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "WorldTrigger")
        {
            main.WorldUpdate();
        }
    }
}
