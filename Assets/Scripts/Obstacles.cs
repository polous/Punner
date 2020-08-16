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

    void Start()
    {
        startPos = transform.localPosition;

        if (direction == movingDirection.Left) curDir = Vector3.left;
        else if (direction == movingDirection.Right) curDir = Vector3.right;
        else if (direction == movingDirection.Down) curDir = Vector3.back;
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
                plr.curHealthPoint -= damage;
                plr.main.BodyHitReaction(plr.mr, plr.MPB, plr.bodyColor);

                plr.main.PlayerDie(plr);
            }
        }
    }
}
