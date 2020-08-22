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

    public void StartScene()
    {
        startPos = transform.localPosition;

        if (direction == movingDirection.Left) curDir = Vector3.left;
        else if (direction == movingDirection.Right) curDir = Vector3.right;
        else if (direction == movingDirection.Down) curDir = Vector3.back;
        else if (direction == movingDirection.Up) curDir = Vector3.forward;
    }

    void Update()
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
                    jail.curHP -= plr.rocketDamage + 10;
                    jail.textMesh.text = jail.curHP.ToString();
                    plr.main.BodyHitReaction(jail.mr, jail.MPB, jail.bodyColor);
                    
                    if (jail.curHP <= 0)
                    {
                        Destroy(jail.gameObject);
                    }
                    else
                    {
                        plr.curHealthPoint -= damage;
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
