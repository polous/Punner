using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public float damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Player plr = other.GetComponent<Player>();
            if (plr.inParty)
            {
                plr.curHealthPoint -= damage;

                if (plr.curHealthPoint <= 0)
                {
                    plr.main.playersInParty.Remove(plr);
                    Destroy(plr.healthPanel.gameObject);
                    Destroy(plr.gameObject);
                }
            }
        }
    }
}
