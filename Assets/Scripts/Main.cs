using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public enum movingDirection
{
    Left,
    Right,
    Down
}

public class Main : MonoBehaviour
{
    [HideInInspector] public Transform Level;
    int worldSize = 80;
    [HideInInspector] public float xPos;
    public float xLimit;
    public float xOffSet;

    public Transform Party;
    public Transform Party_M, Party_R, Party_H;
    public List<Transform> PartyPositionList;
    public GameObject PlayerPrefab;
    public List<Player> playersInParty = new List<Player>();

    public float worldMoveSpeed; // вертикальная скорость смещения мира
    public float partyMoveSpeed; // горизонатльная скорость перемещения пати
    public float playerMoveSpeedToParty; // базовая скорость перемещения игроков в пати при освобождении
    public float playerMoveSpeedInParty; // базовая скорость перемещения игроков в пати для случайного блуждания

    Vector3 newPartyPoint;
    public float maxOffsetInPaty_X, maxOffsetInPaty_Z; // случайное отклонение относительно классового центра пати для случайных блужданий (по Х и по Z)

    public GameObject rocketPrefab; // префаб ракеты
    public Transform rocketsPool; // пул прожектайлов
    public GameObject healthPanelPrefab; // префаб UI панели здоровья
    public Transform healthPanelsPool; // пул UI панелей здоровья

    public GameObject healingEffectPrefab; // префаб эффектов лечения
    public Transform healingEffectsPool; // пул эффектов лечения
    public GameObject deathEffectPrefab; // префаб эффектов смерти
    public Transform deathEffectsPool; // пул эффектов смерти

    public GameObject voidZonePrefab; // префаб войд зон
    public Transform voidZonesPool; // пул войд зон
    public GameObject voidZoneCastEffectPrefab; // префаб эффектов кастования войд зоны
    public Transform voidZoneCastEffectsPool; // пул эффектов кастования войд зоны

    public Text messagePanel;
    public GameObject repeatButton;

    [HideInInspector] public float globalTimer;

    public bool shootingOnlyInMove;
    public bool inMove;

    void Awake()
    {
        // заполняем пул прожектайлов
        for (int i = 0; i < 300; i++)
        {
            GameObject rocket = Instantiate(rocketPrefab) as GameObject;
            rocket.transform.SetParent(rocketsPool);
            rocket.GetComponent<Rocket>().main = this;
        }

        // заполняем пул эффектов смерти
        for (int i = 0; i < 30; i++)
        {
            GameObject DE = Instantiate(deathEffectPrefab) as GameObject;
            DE.transform.SetParent(deathEffectsPool);
        }

        // заполняем пул эффектов лечения
        for (int i = 0; i < 30; i++)
        {
            GameObject HE = Instantiate(healingEffectPrefab) as GameObject;
            HE.transform.SetParent(healingEffectsPool);
        }

        // заполняем пул войд зон
        for (int i = 0; i < 30; i++)
        {
            GameObject voidzone = Instantiate(voidZonePrefab) as GameObject;
            voidzone.transform.SetParent(voidZonesPool);
            voidzone.GetComponent<VoidZone>().main = this;
        }

        // заполняем пул эффектов кастования войд зон
        for (int i = 0; i < 30; i++)
        {
            GameObject voidzonecasteffect = Instantiate(voidZoneCastEffectPrefab) as GameObject;
            voidzonecasteffect.transform.SetParent(voidZoneCastEffectsPool);
        }
    }

    void Start()
    {
        inMove = true;

        messagePanel.text = "";
        globalTimer = 0;
        repeatButton.SetActive(false);

        Player plr = Instantiate(PlayerPrefab).GetComponent<Player>();
        plr.transform.SetParent(PartyPositionList[0]);
        plr.transform.localPosition = Vector3.zero;
        plr.inJail = false;
        plr.inParty = true;
        plr.coll.enabled = true;

        foreach (Player p in Party.GetComponentsInChildren<Player>())
        {
            playersInParty.Add(p);
            Transform hPanelp = Instantiate(healthPanelPrefab).transform;
            hPanelp.SetParent(healthPanelsPool);
            hPanelp.localScale = new Vector3(1, 1, 1);
            p.healthPanel = hPanelp;
            p.healthPanelFill = hPanelp.GetChild(0).GetComponent<Image>();
        }
    }

    // Update is called once per frame
    void Update()
    {

        //foreach (Touch touch in Input.touches)
        //{
        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        //Ray ray = Camera.main.ScreenPointToRay(touch.position);
        //        //if (Physics.Raycast(ray))
        //        //{
        //        //    print("");
        //        //}
        //        print("");
        //    }
        //}
        if (shootingOnlyInMove) inMove = false;
        if (Input.GetMouseButton(0))
        {
            inMove = true;
            if (!repeatButton.activeSelf)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1 << 8))
                {
                    xPos = hit.point.x;
                    xPos += xPos * xOffSet; // ввожу оффсет для более раннего позиционирования на границе дороги

                    if (Mathf.Abs(xPos) > xLimit)
                    {
                        if (xPos > 0) xPos = xLimit;
                        if (xPos < 0) xPos = -xLimit;
                    }

                    newPartyPoint = new Vector3(xPos, Party.position.y, Party.position.z);


                    Party.position = Vector3.Slerp(Party.position, newPartyPoint, Time.deltaTime * partyMoveSpeed);
                }
            }
        }

        //RefreshPartyPositions(playersInParty[0], 0);
    }

    public void WorldUpdate()
    {
        Level.GetChild(0).position += new Vector3(0, 0, worldSize * 3);
        Level.GetChild(0).SetSiblingIndex(2);
    }

    private void LateUpdate()
    {
        Level.Translate(Vector3.back * Time.deltaTime * worldMoveSpeed, Space.World);

        if (!repeatButton.activeSelf)
        {
            globalTimer += Time.deltaTime;
        }
    }


    public void BodyHitReaction(MeshRenderer mr, MaterialPropertyBlock MPB, Color color)
    {
        StartCoroutine(ChangeBodyColor(mr, MPB, color));
    }

    // меняем цвет тушки
    IEnumerator ChangeBodyColor(MeshRenderer mr, MaterialPropertyBlock MPB, Color color)
    {
        mr.GetPropertyBlock(MPB);
        MPB.SetColor("_Color", Color.red);
        mr.SetPropertyBlock(MPB);

        yield return new WaitForSeconds(0.1f);

        if (mr != null)
        {
            mr.GetPropertyBlock(MPB);
            MPB.SetColor("_Color", color);
            mr.SetPropertyBlock(MPB);
        }
    }


    public void ResetCurrentLevel()
    {
        StartCoroutine(resetCurrentLevel());
    }

    IEnumerator resetCurrentLevel()
    {
        int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (curSceneIndex == 0)
        {
            yield return null;
            SceneManager.LoadScene(curSceneIndex);
        }
    }


    public void PlayerDie(Player p)
    {
        StartCoroutine(PlayerDeath(p));
    }

    // убиваем игрока
    IEnumerator PlayerDeath(Player p)
    {
        if (p.curHealthPoint <= 0)
        {
            //if (p.type == playerType.Heal)
            //{
            //    p.myHealingEffect.SetParent(healingEffectsPool);
            //}
            if (p.myHealingEffect != null)
            {
                p.myHealingEffect.SetParent(healingEffectsPool);
            }

            RefreshPartyPositions(p, 1);

            playersInParty.Remove(p);
            Destroy(p.healthPanel.gameObject);
            Destroy(p.gameObject);

            Transform deathEffect = deathEffectsPool.GetChild(0);
            deathEffect.SetParent(Level);
            deathEffect.position = p.transform.position;

            yield return new WaitForSeconds(1);

            deathEffect.SetParent(deathEffectsPool);

            if (playersInParty.Count == 0)
            {
                // находим всех игроков вне пати, но уже и не в тюрьме
                List<Player> freePlrs = Level.GetComponentsInChildren<Player>().Select(x => x).Where(x => !x.inJail).ToList();
                // если освобожденных игроков нет, то конец игры
                if (freePlrs.Count == 0)
                {
                    worldMoveSpeed = 0;
                    messagePanel.gameObject.SetActive(true);
                    messagePanel.text = "Ты проиграл!\nСоберись, тряпка!";
                    repeatButton.SetActive(true);
                }
            }
        }
    }

    public void EnemyDie(Enemy e)
    {
        StartCoroutine(EnemyDeath(e));
    }

    // убиваем врага
    IEnumerator EnemyDeath(Enemy e)
    {
        if (e.curHealthPoint <= 0)
        {
            Destroy(e.gameObject);

            Transform deathEffect = deathEffectsPool.GetChild(0);
            deathEffect.SetParent(Level);
            deathEffect.position = e.transform.position;

            yield return new WaitForSeconds(1);

            deathEffect.SetParent(deathEffectsPool);
        }
    }

    public void HealingEffectReturnToPool(Transform healingEffect)
    {
        StartCoroutine(healingEffectReturnToPool(healingEffect));
    }

    // убиваем врага
    IEnumerator healingEffectReturnToPool(Transform healingEffect)
    {
        yield return new WaitForSeconds(1);

        healingEffect.SetParent(healingEffectsPool);
    }

    public void RefreshPartyPositions(Player plr, int type) // type: 0 - добавление игрока в пати; 1 - убыль (смерть) игрока из пати
    {
        Transform LastPlayerParent = PartyPositionList.Select(x => x).Where(x => x.childCount != 0).Last();
        Transform LastMeleeParent = null;
        List<Player> melees = playersInParty.Select(x => x).Where(x => x.type == playerType.Melee).ToList();
        List<Player> supports = playersInParty.Select(x => x).Where(x => x.type != playerType.Melee).ToList();
        if (melees.Count != 0) LastMeleeParent = melees.Last().transform.parent;

        int indexOfLastPlayerParent = PartyPositionList.IndexOf(LastPlayerParent);
        int indexOfLastMeleeParent = -1;
        if (LastMeleeParent != null) indexOfLastMeleeParent = PartyPositionList.IndexOf(LastMeleeParent);

        if (type == 0)
        {
            if (plr.type != playerType.Melee)
            {
                if (PartyPositionList[0].childCount == 0)
                {
                    plr.transform.SetParent(PartyPositionList[0]);
                    plr.newOffsetPos = Vector3.zero;
                }
                else
                {
                    plr.transform.SetParent(PartyPositionList[indexOfLastPlayerParent + 1]);
                    plr.newOffsetPos = Vector3.zero;
                }
            }
            else
            {
                if (melees.Count == 0)
                {
                    if (PartyPositionList[0].childCount == 0)
                    {
                        plr.transform.SetParent(PartyPositionList[0]);
                        plr.newOffsetPos = Vector3.zero;
                    }
                    else
                    {
                        Player curPlayer = PartyPositionList[0].GetComponentInChildren<Player>();
                        curPlayer.newOffsetPos = Vector3.zero;
                        curPlayer.transform.SetParent(PartyPositionList[indexOfLastPlayerParent + 1]);
                        curPlayer.changeParent = true;

                        plr.transform.SetParent(PartyPositionList[0]);
                        plr.newOffsetPos = Vector3.zero;
                    }
                }
                else
                {
                    if (PartyPositionList[0].childCount == 0)
                    {
                        plr.transform.SetParent(PartyPositionList[0]);
                        plr.newOffsetPos = Vector3.zero;
                    }
                    else
                    {
                        if (supports.Count == 0)
                        {
                            plr.transform.SetParent(PartyPositionList[indexOfLastMeleeParent + 1]);
                            plr.newOffsetPos = Vector3.zero;
                        }
                        else
                        {
                            Player firstSupportPlayer = PartyPositionList[indexOfLastMeleeParent + 1].GetComponentInChildren<Player>();
                            firstSupportPlayer.newOffsetPos = Vector3.zero;
                            firstSupportPlayer.transform.SetParent(PartyPositionList[indexOfLastPlayerParent + 1]);
                            firstSupportPlayer.changeParent = true;

                            plr.transform.SetParent(PartyPositionList[indexOfLastMeleeParent + 1]);
                            plr.newOffsetPos = Vector3.zero;
                        }
                    }
                }
            }
        }

        if (type == 1)
        {
            Transform deadPlayerParent = plr.transform.parent;
            int indexOfDeadPlayerParent = PartyPositionList.IndexOf(deadPlayerParent);

            if (plr.type != playerType.Melee)
            {
                if (supports.Count > 1)
                {
                    if (indexOfDeadPlayerParent != indexOfLastPlayerParent)
                    {
                        Player lastPlayer = PartyPositionList[indexOfLastPlayerParent].GetComponentInChildren<Player>();
                        lastPlayer.newOffsetPos = Vector3.zero;
                        lastPlayer.transform.SetParent(PartyPositionList[indexOfDeadPlayerParent]);
                        lastPlayer.changeParent = true;
                    }
                }
            }
            else
            {
                if (melees.Count > 1)
                {
                    if (indexOfDeadPlayerParent != indexOfLastMeleeParent)
                    {
                        Player lastMeleePlayer = PartyPositionList[indexOfLastMeleeParent].GetComponentInChildren<Player>();
                        lastMeleePlayer.newOffsetPos = Vector3.zero;
                        lastMeleePlayer.transform.SetParent(PartyPositionList[indexOfDeadPlayerParent]);
                        lastMeleePlayer.changeParent = true;

                        if (supports.Count != 0)
                        {
                            Player lastPlayer = PartyPositionList[indexOfLastPlayerParent].GetComponentInChildren<Player>();
                            lastPlayer.newOffsetPos = Vector3.zero;
                            lastPlayer.transform.SetParent(PartyPositionList[indexOfLastMeleeParent]);
                            lastPlayer.changeParent = true;
                        }
                    }
                    else
                    {
                        if (supports.Count != 0)
                        {
                            Player lastPlayer = PartyPositionList[indexOfLastPlayerParent].GetComponentInChildren<Player>();
                            lastPlayer.newOffsetPos = Vector3.zero;
                            lastPlayer.transform.SetParent(PartyPositionList[indexOfDeadPlayerParent]);
                            lastPlayer.changeParent = true;
                        }
                    }
                }
                else
                {
                    if (supports.Count != 0)
                    {
                        Player lastPlayer = PartyPositionList[indexOfLastPlayerParent].GetComponentInChildren<Player>();
                        lastPlayer.newOffsetPos = Vector3.zero;
                        lastPlayer.transform.SetParent(PartyPositionList[indexOfDeadPlayerParent]);
                        lastPlayer.changeParent = true;
                    }
                }
            }
        }
    }

}
