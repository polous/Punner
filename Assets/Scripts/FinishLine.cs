using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
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
            main.worldMoveSpeed = 0;
            Destroy(gameObject);
            main.messagePanel.gameObject.SetActive(true);
            main.messagePanel.text = "ТЫ ПОБЕДИЛ!\n за " + main.globalTimer.ToString("F0") + " сек";
            main.repeatButton.SetActive(true);
        }
    }
}
