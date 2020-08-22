using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class VoidZone : MonoBehaviour
{
    public Transform fillPanel;
    public SpriteRenderer Border;
    public float damage;
    public float radius;
    public float duration; // продолжительность от начала каста до непосредственно взрыва (в секундах)
    public GameObject explosion;
    public bool isCasting;
    public Enemy Custer; // враг, который кастует данную войд зону
    float timer = 0;
    public Transform castEffect;

    public Main main;

    void Update()
    {
        if (isCasting)
        {
            // прервем каст, если кастующего врага убили
            if (Custer.curHealthPoint <= 0)
            {
                explosion.SetActive(false);
                fillPanel.localScale = Vector3.zero;
                isCasting = false;
                timer = 0;
                transform.SetParent(main.voidZonesPool);
                castEffect.SetParent(main.voidZoneCastEffectsPool);
                return;
            }

            timer += Time.deltaTime;
            if (timer >= duration)
            {
                explosion.SetActive(true);
                Border.enabled = false;
                isCasting = false;
                castEffect.SetParent(main.voidZoneCastEffectsPool);
                fillPanel.localScale = Vector3.zero;
                timer = 0;

                foreach (Player p in main.playersInParty)
                {
                    if (p != null)
                    {
                        if ((p.transform.position - transform.position).magnitude <= radius)
                        {
                            p.curHealthPoint -= damage;
                            main.BodyHitReaction(p.mr, p.MPB, p.bodyColor);

                            main.PlayerDie(p);
                        }
                    }
                }

                Invoke("GoToPool", 1.5f);
                return;
            }
            fillPanel.localScale += Vector3.one * 0.77f * Time.deltaTime / duration;
        }
    }

    void GoToPool()
    {
        Border.enabled = true;
        explosion.SetActive(false);
        transform.SetParent(main.voidZonesPool);
    }
}
