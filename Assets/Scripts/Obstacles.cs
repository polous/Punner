using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public float damage;
    public float speed;
    public float distance;
    public movingDirection direction;
    Vector3 curDir;
    Vector3 startPos;

    float t = 0;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public Vector3 origin;
    [HideInInspector] public bool isBorning;
    Collider coll;

    public void StartScene()
    {
        startPos = transform.localPosition;

        if (direction == movingDirection.Left) curDir = Vector3.left;
        else if (direction == movingDirection.Right) curDir = Vector3.right;
        else if (direction == movingDirection.Down) curDir = Vector3.back;
        else if (direction == movingDirection.Up) curDir = Vector3.forward;

        coll = GetComponent<Collider>();
    }


    Vector3 Ballistic(float time, Vector3 origin, Vector3 speed)
    {
        return origin + speed * time + Physics.gravity * time * time / 2f;
    }

    void Update()
    {
        if (isBorning)
        {
            t += Time.deltaTime;
            transform.position = Ballistic(t, origin, velocity);

            if (transform.position.y <= 0.75f)
            {
                coll.enabled = true;
                isBorning = false;
                transform.position = new Vector3(transform.position.x, 0.75f, transform.position.z);
                t = 0;                
            }
        }
        else
        {
            if (speed > 0)
            {
                if ((transform.localPosition - startPos).magnitude >= distance)
                {
                    startPos = transform.localPosition;
                    curDir = -curDir;
                }
                else
                {
                    transform.Translate(curDir * Time.deltaTime * speed, Space.World);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Player plr = other.GetComponent<Player>();
            if (plr.inParty)
            {
                Jail jail = GetComponent<Jail>();
                if (jail != null)
                {
                    jail.curHP -= plr.collDamage;
                    jail.textMesh.text = jail.curHP.ToString();
                    plr.main.BodyHitReaction(jail.mr, jail.MPB, jail.bodyColor);
                    
                    if (jail.curHP <= 0)
                    {
                        Destroy(jail.gameObject);
                    }
                    else
                    {
                        plr.curHealthPoint -= damage;
                        plr.healthPanel.gameObject.SetActive(true);
                        plr.healthPanelScript.HitFunction(plr.curHealthPoint / plr.maxHealthPoint, damage);
                        plr.main.BodyHitReaction(plr.mr, plr.MPB, plr.bodyColor);

                        plr.main.PlayerDie(plr);
                    }
                }
                else
                {
                    plr.curHealthPoint -= damage;
                    plr.main.BodyHitReaction(plr.mr, plr.MPB, plr.bodyColor);

                    plr.main.PlayerDie(plr);
                }
            }
        }
    }
}
