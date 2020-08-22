using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public Main main;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "WorldTrigger")
        {
            main.Level = null;
            Destroy(gameObject);

            main.storedPlayersListForNext.Clear();
            foreach(Player p in main.playersInParty)
            {
                main.storedPlayersListForNext.Add(new PlayerData(p));
            }
  
            foreach (Player p in main.playersInParty)
            {
                Destroy(p.healthPanel.gameObject);
            }

            main.messagePanel.gameObject.SetActive(true);
            main.messagePanel.text = "ТЫ ПОБЕДИЛ!\n за " + main.globalTimer.ToString("F0") + " сек";
            main.repeatButton.SetActive(true);
            main.NextButton.SetActive(true);
        }
    }
}
