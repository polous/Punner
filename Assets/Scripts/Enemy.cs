using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public float shootRange; // дистанция стрельбы
    public float shootSpreadCoeff;
    public float rocketDamage; // текущий урон от оружия
    public float rocketSpeed; // скорость полета пули
    public float reloadingTime; // время перезарядки оружия (задержка между соседними атаками)
    bool reloading;

    public float maxHealthPoint; // максимальный запас здоровья
    public float curHealthPoint; // текущий запас здоровья
    //public Transform healthPanel;
    //public Image healthPanelFill;

    public Main main;
    public Collider coll;

    public Color bodyColor;
    [HideInInspector] public MaterialPropertyBlock MPB;
    [HideInInspector] public MeshRenderer mr;
    public Color rocketColor;
    public float rocketSize;
    public bool rocketFlyingThrough;

    public float speed;
    public float distance;
    public movingDirection direction;
    Vector3 curDir;
    Vector3 startPos;

    public float voidZoneDamage; // урон от войд зоны
    public float voidZoneRadius; // радиус войд зоны
    public float voidZoneDuration; // продолжительность от начала каста до непосредственно взрыва (в секундах)
    public float voidZoneReloadingTime; // время перезарядки войд зоны (задержка между соседними кастами в секундах)
    bool voidZoneReloading;

    public float collDamage;

    public void StartScene()
    {
        MPB = new MaterialPropertyBlock();
        mr = GetComponentInChildren<MeshRenderer>();
        mr.GetPropertyBlock(MPB);
        MPB.SetColor("_Color", bodyColor);
        mr.SetPropertyBlock(MPB);

        main = FindObjectOfType<Main>();

        curHealthPoint = maxHealthPoint;

        startPos = transform.localPosition;

        if (direction == movingDirection.Left) curDir = Vector3.left;
        else if (direction == movingDirection.Right) curDir = Vector3.right;
        else if (direction == movingDirection.Down) curDir = Vector3.back;
        else if (direction == movingDirection.Up) curDir = Vector3.forward;
    }


    // Получение точки выставления войд зоны
    Vector3 GetVoidZoneLocalPosition()
    {
        Vector3 partyLocPos = main.Level.InverseTransformPoint(main.Party.position);
        return new Vector3(partyLocPos.x, partyLocPos.y, partyLocPos.z + voidZoneDuration * main.worldMoveSpeed + 2f);
    }


    void Update()
    {
        if (main == null) return;

        if (transform.position.z > -50f && (main.Party.position - transform.position).magnitude <= 40f)
        {
            // кастуем войд зоны
            if (voidZoneDamage > 0)
            {
                if (!voidZoneReloading)
                {
                    VoidZone voidZone = main.voidZonesPool.GetChild(0).GetComponent<VoidZone>();
                    voidZone.transform.parent = main.Level;
                    voidZone.transform.localPosition = GetVoidZoneLocalPosition();
                    voidZone.damage = voidZoneDamage;
                    voidZone.radius = voidZoneRadius;
                    voidZone.transform.localScale = Vector3.one * voidZoneRadius;
                    voidZone.duration = voidZoneDuration;
                    voidZone.isCasting = true;
                    voidZone.Custer = this;

                    ParticleSystem ps = voidZone.GetComponentInChildren<ParticleSystem>(true);
                    var psMain = ps.main;
                    psMain.startSizeMultiplier = voidZoneRadius * 2f;

                    Transform vzce = main.voidZoneCastEffectsPool.GetChild(0);
                    vzce.transform.parent = transform;
                    vzce.transform.position = transform.position;
                    voidZone.castEffect = vzce;

                    // "пережаряжаемся" (задержка между войд зонами)
                    StartCoroutine(VoidZoneReloading(voidZoneReloadingTime));
                }
            }


            // движение врага
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

            // стрельба врага
            if (shootRange > 0)
            {
                if (!reloading)
                {
                    // вытаскиваем из пула и настраиваем прожектайл 
                    Rocket rocket = main.rocketsPool.GetChild(0).GetComponent<Rocket>();
                    rocket.transform.parent = main.Level;
                    rocket.transform.position = coll.bounds.center;
                    rocket.startPoint = rocket.transform.position;
                    rocket.maxRange = shootRange;
                    rocket.MyShooterTag = tag;
                    rocket.flying = true;
                    rocket.speed = rocketSpeed;
                    rocket.damage = rocketDamage;
                    //rocket.RocketTypeChanger(rocketType.Bomb);
                    rocket.RocketParamsChanger(MPB, rocketColor, rocketSize);
                    rocket.flyThrough = rocketFlyingThrough;

                    Vector3 randomVector = new Vector3(Random.Range(-shootSpreadCoeff, +shootSpreadCoeff), 0, Random.Range(-shootSpreadCoeff, +shootSpreadCoeff));
                    Vector3 lastPoint = transform.localPosition + transform.forward * shootRange + randomVector;
                    Vector3 direction = lastPoint - transform.localPosition;

                    rocket.direction = direction;

                    // "пережаряжаемся" (задержка между выстрелами)
                    StartCoroutine(Reloading(reloadingTime));
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

    // "перезарядка" войд зоны (задержка между кастами)
    IEnumerator VoidZoneReloading(float voidZoneReloadingTime)
    {
        voidZoneReloading = true;
        yield return new WaitForSeconds(voidZoneReloadingTime);
        voidZoneReloading = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Player plr = other.GetComponent<Player>();
            if (plr.inParty)
            {
                plr.curHealthPoint -= collDamage;
                plr.main.BodyHitReaction(plr.mr, plr.MPB, plr.bodyColor);

                plr.main.PlayerDie(plr);
            }
        }
    }
}
