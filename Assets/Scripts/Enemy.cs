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
    [HideInInspector] public Transform healthPanel;
    [HideInInspector] public HealthPanel healthPanelScript;

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
    [HideInInspector] public Vector3 curDir;
    [HideInInspector] public Vector3 startPos;

    public float voidZoneDamage; // урон от войд зоны
    public float voidZoneRadius; // радиус войд зоны
    public float voidZoneDuration; // продолжительность от начала каста до непосредственно взрыва (в секундах)
    public float voidZoneReloadingTime; // время перезарядки войд зоны (задержка между соседними кастами в секундах)
    bool voidZoneReloading;

    public float collDamage;

    public float actionRange;

    [Space]
    [Space]
    [Header("BOSS SECTION")]
    public bool isBoss;
    int bossPattern;
    [HideInInspector] public bool bossIsWaiting;
    public int pattern_1_Cycles;
    public int pattern_2_Cycles;
    public int pattern_3_Cycles;
    [HideInInspector] public List<Vector3> pattern_1_Positions;
    [HideInInspector] public List<Vector3> pattern_2_Positions;
    [HideInInspector] public List<Vector3> pattern_3_Positions;
    public float patternReloadingTime;
    bool patternReloading;
    int moveIndex = 0;
    public GameObject jailPrefab;
    public GameObject wallPrefab;
    public float jailHpBot, jailHpTop;
    public float wallInstanceReloadingTime;
    bool wallInstanceReloading;
    Transform Throwpoint;


    float ThrowVelocityCalc(float g, float ang, float x, float y)
    {
        float angRad = ang * Mathf.PI / 180f;
        float v2 = (g * x * x) / (2 * (y - Mathf.Tan(angRad) * x) * Mathf.Pow(Mathf.Cos(angRad), 2));
        float v = Mathf.Sqrt(Mathf.Abs(v2));
        return v;
    }

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

        if (isBoss)
        {
            main.Boss = this;
            bossIsWaiting = true;

            bossPattern = Random.Range(1, 4);
            pattern_1_Positions = new List<Vector3>();
            pattern_2_Positions = new List<Vector3>();
            pattern_3_Positions = new List<Vector3>();

            pattern_1_Positions.Add(main.centerPosition.position);
            pattern_2_Positions.Add(main.centerPosition.position);
            pattern_3_Positions.Add(main.centerPosition.position);

            for (int i = 0; i < pattern_1_Cycles; i++)
            {
                foreach (Transform tr in main.pattern_1_Positions)
                {
                    pattern_1_Positions.Add(tr.position);
                }
            }
            for (int i = 0; i < pattern_2_Cycles; i++)
            {
                foreach (Transform tr in main.pattern_2_Positions)
                {
                    pattern_2_Positions.Add(tr.position);
                }
            }
            for (int i = 0; i < pattern_3_Cycles; i++)
            {
                foreach (Transform tr in main.pattern_3_Positions)
                {
                    pattern_3_Positions.Add(tr.position);
                }
            }

            pattern_1_Positions.Add(main.centerPosition.position);
            pattern_2_Positions.Add(main.centerPosition.position);
            pattern_3_Positions.Add(main.centerPosition.position);
        }
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
        if (main.Level == null) return;
        if (!main.readyToGo) return;

        if (healthPanel != null) healthPanel.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * coll.bounds.size.y * 1.2f);

        if (isBoss)
        {
            if (bossIsWaiting == false)
            {
                //transform.localPosition += new Vector3(0, 0, Time.deltaTime * main.worldMoveSpeed);
                //transform.Translate(Vector3.forward * Time.deltaTime * main.worldMoveSpeed, Space.World);

                if (!patternReloading)
                {
                    if (bossPattern == 1)
                    {
                        // движение босса
                        if (speed > 0)
                        {
                            if ((transform.position - pattern_1_Positions[moveIndex]).magnitude <= 0.1f)
                            {
                                moveIndex++;
                                if (moveIndex == pattern_1_Positions.Count)
                                {
                                    transform.position = pattern_1_Positions.Last();
                                    moveIndex = 0;
                                    for (int i = 0; i < 20; i++)
                                    {
                                        bossPattern = Random.Range(1, 4);
                                        if (bossPattern != 1) break;
                                    }

                                    // "пережаряжаемся" (задержка между паттернами)
                                    StartCoroutine(PatternReloading(patternReloadingTime));
                                }
                            }
                            else
                            {
                                transform.position = Vector3.MoveTowards(transform.position, pattern_1_Positions[moveIndex], speed * Time.deltaTime);
                            }
                        }

                        // стрельба босса
                        if (shootRange > 0)
                        {
                            if (!reloading)
                            {
                                // вытаскиваем из пула и настраиваем прожектайл 
                                Rocket rocket = main.rocketsPool.GetChild(0).GetComponent<Rocket>();
                                rocket.transform.parent = main.Level;
                                rocket.transform.position = new Vector3(transform.position.x, 0.8f, transform.position.z);
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

                    else if (bossPattern == 2)
                    {
                        // движение босса
                        if (speed > 0)
                        {
                            if ((transform.position - pattern_2_Positions[moveIndex]).magnitude <= 0.1f)
                            {
                                moveIndex++;
                                if (moveIndex == pattern_2_Positions.Count)
                                {
                                    transform.position = pattern_2_Positions.Last();
                                    moveIndex = 0;
                                    for (int i = 0; i < 20; i++)
                                    {
                                        bossPattern = Random.Range(1, 4);
                                        if (bossPattern != 2) break;
                                    }

                                    // "пережаряжаемся" (задержка между паттернами)
                                    StartCoroutine(PatternReloading(patternReloadingTime));
                                }
                            }
                            else
                            {
                                transform.position = Vector3.MoveTowards(transform.position, pattern_2_Positions[moveIndex], speed * Time.deltaTime);
                            }
                        }

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

                                //ParticleSystem ps = voidZone.GetComponentInChildren<ParticleSystem>(true);
                                //var psMain = ps.main;
                                //psMain.startSizeMultiplier = voidZoneRadius * 2f;

                                voidZone.VZShowRadius();

                                Transform vzce = main.voidZoneCastEffectsPool.GetChild(0);
                                vzce.transform.parent = transform;
                                vzce.transform.position = transform.position;
                                voidZone.castEffect = vzce;

                                // "пережаряжаемся" (задержка между войд зонами)
                                StartCoroutine(VoidZoneReloading(voidZoneReloadingTime));
                            }
                        }
                    }

                    else if (bossPattern == 3)
                    {
                        // движение босса
                        if (speed > 0)
                        {
                            if ((transform.position - pattern_3_Positions[moveIndex]).magnitude <= 0.1f)
                            {
                                moveIndex++;
                                if (moveIndex == pattern_3_Positions.Count)
                                {
                                    transform.position = pattern_3_Positions.Last();
                                    moveIndex = 0;
                                    for (int i = 0; i < 20; i++)
                                    {
                                        bossPattern = Random.Range(1, 4);
                                        if (bossPattern != 3) break;
                                    }

                                    // "пережаряжаемся" (задержка между паттернами)
                                    StartCoroutine(PatternReloading(patternReloadingTime));
                                }
                            }
                            else
                            {
                                transform.position = Vector3.MoveTowards(transform.position, pattern_3_Positions[moveIndex], speed * Time.deltaTime);
                            }
                        }

                        if (!wallInstanceReloading)
                        {
                            //int type = Random.Range(0, 2);
                            int type = 1;
                            GameObject wall;
                            if (type == 1) wall = Instantiate(jailPrefab, main.Level);
                            else wall = Instantiate(wallPrefab, main.Level);

                            Obstacles wallObstacle = wall.GetComponent<Obstacles>();
                            Collider wallCollider = wall.GetComponent<Collider>();

                            //wall.transform.position = new Vector3(Random.Range(-4.5f, 4.5f), wall.GetComponent<Collider>().bounds.center.y, transform.position.z - 5f);
                            //wall.transform.rotation = Quaternion.Euler(0, Random.Range(-30f, 30f), 0);

                            if (type == 1)
                            {
                                Jail jail = wall.GetComponent<Jail>();
                                jail.Bot = jailHpBot;
                                jail.Top = jailHpTop;
                                jail.StartScene();
                            }

                            float velocity;
                            float ThrowDistX, ThrowDistY;
                            float Ang = 50;

                            Vector3 bornPos = new Vector3(Random.Range(-4.0f, 4.0f), 0.75f, transform.position.z - 6f);

                            Throwpoint = transform.Find("Throwpoint");
                            wall.transform.position = Throwpoint.position;

                            Vector3 FromTo = bornPos - Throwpoint.position;
                            Vector3 FromToXZ = new Vector3(FromTo.x, 0f, FromTo.z);

                            ThrowDistX = FromToXZ.magnitude;
                            ThrowDistY = FromTo.y;

                            Throwpoint.rotation = Quaternion.LookRotation(FromToXZ);

                            Throwpoint.localEulerAngles = new Vector3(-Ang, Throwpoint.localEulerAngles.y, Throwpoint.localEulerAngles.z);
                            velocity = ThrowVelocityCalc(Physics.gravity.y, Ang, ThrowDistX, ThrowDistY);

                            //ShowTrajectory(Throwpoint.position, velocity * Throwpoint.forward);

                            wallObstacle.StartScene();
                            wallObstacle.isBorning = true;
                            wallObstacle.velocity = velocity * Throwpoint.forward;
                            wallObstacle.origin = Throwpoint.position;
                            wallCollider.enabled = false;

                            // "перезарядка" инстанса препятствий (задержка между инстансом препятствий)
                            StartCoroutine(WallInstanceReloading(wallInstanceReloadingTime));
                        }
                    }
                }
            }
        }

        // ЕСЛИ НЕ БОСС:
        else
        {
            if (transform.position.z > -50f && (main.Party.position - transform.position).magnitude <= actionRange)
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

                        //ParticleSystem ps = voidZone.GetComponentInChildren<ParticleSystem>(true);
                        //var psMain = ps.main;
                        //psMain.startSizeMultiplier = voidZoneRadius * 2f;

                        voidZone.VZShowRadius();

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

    // "перезарядка" паттернов (задержка между паттернами)
    IEnumerator PatternReloading(float patternReloadingTime)
    {
        patternReloading = true;
        yield return new WaitForSeconds(patternReloadingTime);
        patternReloading = false;
    }

    // "перезарядка" инстанса препятствий (задержка между инстансом препятствий)
    IEnumerator WallInstanceReloading(float wallInstanceReloadingTime)
    {
        wallInstanceReloading = true;
        yield return new WaitForSeconds(wallInstanceReloadingTime);
        wallInstanceReloading = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Player plr = other.GetComponent<Player>();
            if (plr.inParty)
            {
                curHealthPoint -= plr.collDamage;
                main.BodyHitReaction(mr, MPB, bodyColor);

                if (curHealthPoint <= 0)
                {
                    main.EnemyDie(this);
                }
                else
                {
                    plr.curHealthPoint -= collDamage;
                    plr.main.BodyHitReaction(plr.mr, plr.MPB, plr.bodyColor);

                    plr.main.PlayerDie(plr);
                }
            }
        }
    }
}
