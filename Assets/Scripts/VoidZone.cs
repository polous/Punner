using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class VoidZone : MonoBehaviour
{
    public Transform fillPanel;
    public float damage;
    public float radius;
    public float duration; // продолжительность от начала каста до непосредственно взрыва (в секундах)
    public GameObject explosion;
    public bool isCasting;
    public Enemy Custer; // враг, который кастует данную войд зону
    float timer = 0;
    public Transform castEffect;

    public Main main;

    public LineRenderer lr;


    public void VZShowRadius()
    {
        float ThetaScale = 0.02f;
        int Size = (int)((1f / ThetaScale) + 1f);
        float theta = 0;

        lr.positionCount = Size;
        for (int i = 0; i < Size; i++)
        {
            theta += (2.0f * Mathf.PI * ThetaScale);
            float x = Mathf.Cos(theta);
            float y = Mathf.Sin(theta);
            lr.SetPosition(i, new Vector3(x, y, 0.1f));
        }
    }


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
                //Custer.lr.enabled = false;
                timer = 0;
                transform.SetParent(main.voidZonesPool);
                castEffect.SetParent(main.voidZoneCastEffectsPool);
                return;
            }

            timer += Time.deltaTime;
            if (timer >= duration)
            {
                explosion.SetActive(true);
                lr.enabled = false;
                //Custer.lr.enabled = false;
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
                            p.healthPanel.gameObject.SetActive(true);
                            p.healthPanelScript.HitFunction(p.curHealthPoint / p.maxHealthPoint, damage);
                            main.BodyHitReaction(p.mr, p.MPB, p.bodyColor);

                            main.PlayerDie(p);
                        }
                    }
                }

                Invoke("GoToPool", 1.5f);
                return;
            }
            fillPanel.localScale += Vector3.one * Time.deltaTime / duration;
        }
    }

    void GoToPool()
    {
        lr.enabled = true;
        explosion.SetActive(false);
        transform.SetParent(main.voidZonesPool);
    }
}
