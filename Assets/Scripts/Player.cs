using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Main main;
    public float moveSpeed; // базовая скорость перемещения игрока
    public float shootRange; // дистанция стрельбы
    public float shootSpreadCoeff;
    public float rocketDamage; // текущий урон от оружия
    public float rocketSpeed; // скорость полета пули
    public float reloadingTime; // время перезарядки оружия (задержка между соседними атаками)
    bool reloading;
    public float rotateSpeed; // скорость поворота

    public float maxHealthPoint; // максимальный запас здоровья
    public float curHealthPoint; // текущий запас здоровья

    public bool inJail;
    public bool inParty;

    public Collider coll;

    Vector3 rndV3;


    void Start()
    {
        main = FindObjectOfType<Main>();
        if (inJail) coll.enabled = false;
    }

    void Update()
    {
        if (main == null) return;

        if (!inJail)
        {
            if (inParty)
            {
                Vector3 fwd = transform.forward; fwd.y = 0;
                if (!reloading)
                {
                    // вытаскиваем из пула и настраиваем прожектайл 
                    Rocket rocket = main.rocketsPool.GetChild(0).GetComponent<Rocket>();
                    rocket.transform.parent = null;
                    rocket.transform.position = transform.position + 1.4f * Vector3.up;
                    rocket.startPoint = rocket.transform.position;
                    rocket.maxRange = shootRange;
                    rocket.MyShooterTag = tag;
                    rocket.flying = true;
                    rocket.speed = rocketSpeed;
                    rocket.damage = rocketDamage;

                    Vector3 randomVector = new Vector3(Random.Range(-shootSpreadCoeff, +shootSpreadCoeff), 0, Random.Range(-shootSpreadCoeff, +shootSpreadCoeff));
                    Vector3 lastPoint = transform.position + transform.forward * shootRange + randomVector;
                    Vector3 direction = lastPoint - transform.position;

                    rocket.direction = direction;

                    // "пережаряжаемся" (задержка между выстрелами)
                    StartCoroutine(Reloading(reloadingTime));
                }
            }
            else
            {
                Vector3 fwd = transform.forward; fwd.y = 0;
                Vector3 dir = Vector3.forward; dir.y = 0;
                Vector3 newOffsetPos = main.player.transform.position + rndV3;
                if ((transform.position - newOffsetPos).magnitude <= 0.2f)
                {
                    inParty = true;
                    transform.rotation = Quaternion.identity;
                    transform.SetParent(main.Party);
                    coll.enabled = true;
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, newOffsetPos, Time.deltaTime * moveSpeed);
                    transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(fwd, dir, rotateSpeed * Time.deltaTime, 0));
                }
            }
        }
        else
        {
            if (transform.position.z > -35f && (main.player.transform.position - transform.position).magnitude <= shootRange)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, 2f, 1 << 9))
                {
                    inJail = false;
                    transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                    transform.SetParent(null);

                    //rndV3 = new Vector3(Random.Range(-1f, +1f), 0, Random.Range(-1f, +1f));

                    for (int i = 0; i < 100; i++)
                    {
                        rndV3 = new Vector3(Random.Range(-1f, +1f), 0, Random.Range(-1f, +1f));
                        List<bool> array = new List<bool>();
                        foreach (Player p in main.GetComponentsInChildren<Player>())
                        {
                            if ((p.transform.position - rndV3).magnitude < 0.6f) array.Add(false);
                            else array.Add(true);
                        }
                        if (array.All(x => x)) break;
                    }
                }
            }
        }
    }

    // "перезарядка" оружия (задержка между выстрелами)
    IEnumerator Reloading(float reloadingTime)
    {
        reloading = true;
        yield return new WaitForSeconds(reloadingTime);
        reloading = false;
    }
}
