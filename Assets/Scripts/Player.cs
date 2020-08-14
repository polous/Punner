using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public enum playerType
{
    Range,
    Melee,
    Heal
}

public class Player : MonoBehaviour
{
    public playerType type; // тип игрока    
    public float shootRange; // дистанция стрельбы
    public float shootSpreadCoeff;
    public float rocketDamage; // текущий урон от оружия
    public float rocketSpeed; // скорость полета пули
    public float reloadingTime; // время перезарядки оружия (задержка между соседними атаками)
    bool reloading;
    public float rotateSpeed; // скорость поворота

    public float maxHealthPoint; // максимальный запас здоровья
    public float curHealthPoint; // текущий запас здоровья
    public Transform healthPanel;
    public Image healthPanelFill;

    public float healPointsRecoveryCount; // количество хп, восстанавливаемое хиллером
    public float healReloadingTime; // время перезарядки лечилки
    [SerializeField] bool healReloading;
    public Transform myHealingEffect;

    public bool inJail;
    public bool inParty;

    Vector3 rndV3;
    RaycastHit hit;

    public Main main;
    public Collider coll;

    public Color bodyColor;
    [HideInInspector] public MaterialPropertyBlock MPB;
    [HideInInspector] public MeshRenderer mr;

    Vector3 newOffsetPos = Vector3.zero;


    void Start()
    {
        MPB = new MaterialPropertyBlock();
        mr = GetComponentInChildren<MeshRenderer>();
        mr.GetPropertyBlock(MPB);
        MPB.SetColor("_Color", bodyColor);
        mr.SetPropertyBlock(MPB);

        main = FindObjectOfType<Main>();
        if (inJail) coll.enabled = false;

        curHealthPoint = maxHealthPoint;

        newOffsetPos = transform.localPosition;
    }


    void Update()
    {
        if (main == null) return;
        if (inParty && healthPanel != null)
        {
            healthPanel.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * coll.bounds.size.y);
            healthPanelFill.fillAmount = curHealthPoint / maxHealthPoint;
        }

        if (!inJail)
        {
            if (inParty)
            {
                // случайные блуждания в пати относительно классовoго центра
                if ((transform.localPosition - newOffsetPos).magnitude <= 0.1f)
                {
                    rndV3 = new Vector3(Random.Range(-main.maxOffsetInPaty_X, +main.maxOffsetInPaty_X), 0, Random.Range(-main.maxOffsetInPaty_Z, +main.maxOffsetInPaty_Z));
                    if (type == playerType.Melee) newOffsetPos = main.Party_M.localPosition + rndV3;
                    else if (type == playerType.Range) newOffsetPos = main.Party_R.localPosition + rndV3;
                    else if (type == playerType.Heal) newOffsetPos = main.Party_H.localPosition + rndV3;
                }
                else
                {
                    transform.localPosition = Vector3.MoveTowards(transform.localPosition, newOffsetPos, Time.deltaTime * main.playerMoveSpeedInParty);
                }

                // поведение в зависимости от класса игрока
                if (type == playerType.Range)
                {
                    // стрелок стреляет
                    Vector3 fwd = transform.forward; fwd.y = 0;
                    if (!reloading)
                    {
                        // вытаскиваем из пула и настраиваем прожектайл 
                        Rocket rocket = main.rocketsPool.GetChild(0).GetComponent<Rocket>();
                        rocket.transform.parent = null;
                        rocket.transform.position = coll.bounds.center;
                        rocket.startPoint = rocket.transform.position;
                        rocket.maxRange = shootRange;
                        rocket.MyShooterTag = tag;
                        rocket.flying = true;
                        rocket.speed = rocketSpeed;
                        rocket.damage = rocketDamage;
                        rocket.RocketTypeChanger(rocketType.Bullet);

                        Vector3 randomVector = new Vector3(Random.Range(-shootSpreadCoeff, +shootSpreadCoeff), 0, Random.Range(-shootSpreadCoeff, +shootSpreadCoeff));
                        Vector3 lastPoint = transform.position + transform.forward * shootRange + randomVector;
                        Vector3 direction = lastPoint - transform.position;

                        rocket.direction = direction;

                        // "пережаряжаемся" (задержка между выстрелами)
                        StartCoroutine(Reloading(reloadingTime));
                    }
                }
                else if (type == playerType.Melee)
                {
                    // милишник тоже стреляет, но на короткой дистанции
                    Vector3 fwd = transform.forward; fwd.y = 0;
                    if (!reloading)
                    {
                        // вытаскиваем из пула и настраиваем прожектайл 
                        Rocket rocket = main.rocketsPool.GetChild(0).GetComponent<Rocket>();
                        rocket.transform.parent = null;
                        rocket.transform.position = coll.bounds.center;
                        rocket.startPoint = rocket.transform.position;
                        rocket.maxRange = shootRange;
                        rocket.MyShooterTag = tag;
                        rocket.flying = true;
                        rocket.speed = rocketSpeed;
                        rocket.damage = rocketDamage;
                        rocket.RocketTypeChanger(rocketType.Melee);

                        rocket.direction = fwd;

                        // "пережаряжаемся" (задержка между выстрелами)
                        StartCoroutine(Reloading(reloadingTime));

                        #region
                        //if (Physics.Raycast(transform.position + Vector3.up * 1f, fwd, out hit, shootRange))
                        //{
                        //    if (hit.transform.tag == "Jail")
                        //    {
                        //        // вытаскиваем из пула и настраиваем прожектайл 
                        //        Rocket rocket = main.rocketsPool.GetChild(0).GetComponent<Rocket>();
                        //        rocket.transform.parent = null;
                        //        rocket.transform.position = transform.position + 1.4f * Vector3.up;
                        //        rocket.startPoint = rocket.transform.position;
                        //        rocket.maxRange = shootRange;
                        //        rocket.MyShooterTag = tag;
                        //        rocket.flying = true;
                        //        rocket.speed = rocketSpeed;
                        //        rocket.damage = rocketDamage;
                        //        rocket.RocketTypeChanger(rocketType.Melee);

                        //        rocket.direction = fwd;

                        //        // "пережаряжаемся" (задержка между выстрелами)
                        //        StartCoroutine(Reloading(reloadingTime));
                        //    }
                        //}
                        #endregion
                    }
                }
                else if (type == playerType.Heal)
                {
                    // хиллер лечит всех в пати
                    if (!healReloading)
                    {
                        foreach (Player p in main.playersInParty)
                        {
                            p.curHealthPoint += healPointsRecoveryCount;
                            if (p.curHealthPoint > p.maxHealthPoint)
                            {
                                p.curHealthPoint = p.maxHealthPoint;
                            }
                        }

                        // "пережаряжаемся" (задержка между лечениями)
                        StartCoroutine(HealReloading(healReloadingTime));
                    }
                }
            }
            // если не в тюрьме, но еще и не в пати, то бежим в пати
            else
            {
                Vector3 fwd = transform.forward; fwd.y = 0;
                Vector3 dir = Vector3.forward; dir.y = 0;

                if (type == playerType.Melee) newOffsetPos = main.Party_M.position + rndV3;
                else if (type == playerType.Range) newOffsetPos = main.Party_R.position + rndV3;
                else if (type == playerType.Heal) newOffsetPos = main.Party_H.position + rndV3;

                if ((transform.position - newOffsetPos).magnitude <= 0.2f)
                {
                    inParty = true;
                    transform.rotation = Quaternion.identity;

                    if (type == playerType.Melee) transform.SetParent(main.Party_M);
                    else if (type == playerType.Range) transform.SetParent(main.Party_R);
                    else if (type == playerType.Heal) transform.SetParent(main.Party_H);

                    coll.enabled = true;
                    main.playersInParty.Add(this);

                    Transform hPanelp = Instantiate(main.healthPanelPrefab).transform;
                    hPanelp.SetParent(main.healthPanelsPool);
                    hPanelp.localScale = new Vector3(1, 1, 1);
                    healthPanel = hPanelp;
                    healthPanelFill = hPanelp.GetChild(0).GetComponent<Image>();

                    newOffsetPos = transform.localPosition;

                    if (type == playerType.Heal)
                    {
                        Transform healingEffect = main.healingEffectsPool.GetChild(0);
                        healingEffect.SetParent(transform);
                        healingEffect.position = transform.position;
                        myHealingEffect = healingEffect;
                    }
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, newOffsetPos, Time.deltaTime * main.playerMoveSpeedToParty);
                    transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(fwd, dir, rotateSpeed * Time.deltaTime, 0));
                }
            }
        }
        // если в тюрьме, то ждем освобождения
        else
        {
            if (transform.position.z > -35f && (main.Party.position - transform.position).magnitude <= 30f)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, 2f, 1 << 9))
                {
                    inJail = false;
                    transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                    transform.SetParent(null);

                    rndV3 = new Vector3(Random.Range(-main.maxOffsetInPaty_X, +main.maxOffsetInPaty_X), 0, Random.Range(-main.maxOffsetInPaty_Z, +main.maxOffsetInPaty_Z));
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

    // "перезарядка" лечения (задержка между лечениями)
    IEnumerator HealReloading(float healReloadingTime)
    {
        healReloading = true;
        yield return new WaitForSeconds(healReloadingTime);
        healReloading = false;
    }
}
