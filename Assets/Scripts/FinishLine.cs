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
            if (main.Boss == null)
            {
                main.Level = null;
                Destroy(gameObject);

                main.storedPlayersListForNext.Clear();
                foreach (Player p in main.playersInParty)
                {
                    main.storedPlayersListForNext.Add(new PlayerData(p));
                }

                foreach (Player p in main.playersInParty)
                {
                    Destroy(p.healthPanel.gameObject);
                }

                main.messagePanel.gameObject.SetActive(true);
                main.messagePanel.text = "Good job!\nIt took you " + main.globalTimer.ToString("F0") + " seconds";
                main.repeatButton.SetActive(true);
                main.resetButton.SetActive(false);
                main.NextButton.SetActive(true);
            }
            else
            {
                //main.messagePanel.gameObject.SetActive(true);
                //main.messagePanel.text = "Думаешь, что победил!?\nА как на счет\nглавного испытания!?";
                main.Boss.transform.parent = null;
                //main.Boss.bossIsWaiting = true;

                //StartCoroutine(MessageDeactivate());

                main.Boss.bossIsWaiting = false;
                Destroy(gameObject);
            }
        }
    }

    IEnumerator MessageDeactivate()
    {
        yield return new WaitForSeconds(3);

        main.messagePanel.gameObject.SetActive(false);
        main.Boss.bossIsWaiting = false;

        Destroy(gameObject);
    }
}
