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
    Down,
    Up
}

public class Main : MonoBehaviour
{
    public Transform Level;
    int worldSize = 80;
    [HideInInspector] public float xPos;
    public float xLimit;
    public float xOffSet;

    public Transform Party;
    //public Transform Party_M, Party_R, Party_H;
    public List<Transform> PartyPositionList;
    public List<Transform> RowPositionList;
    public GameObject PlayerPrefab_Range;
    public GameObject PlayerPrefab_Melee;
    public GameObject PlayerPrefab_Heal;
    public List<Player> playersInParty = new List<Player>();
    public int partyPlayerLimit;

    public float worldMoveSpeed; // вертикальная скорость смещения мира
    public float partyMoveSpeed; // горизонатльная скорость перемещения пати
    public float playerMoveSpeedToParty; // базовая скорость перемещения игроков в пати при освобождении
    public float playerMoveSpeedInParty; // базовая скорость перемещения игроков в пати для случайного блуждания

    Vector3 newPartyPoint;
    public float maxOffsetInPaty_X, maxOffsetInPaty_Z; // случайное отклонение относительно классового центра пати для случайных блужданий (по Х и по Z)

    public GameObject rocketPrefab; // префаб ракеты
    public Transform rocketsPool; // пул прожектайлов
    public GameObject healthPanelPrefab; // префаб UI панели здоровья (игроки)
    public GameObject healthPanelPrefabE; // префаб UI панели здоровья (враги)
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
    public GameObject resetButton;
    public GameObject NextButton;
    public List<GameObject> dontDestroyOnLoadGameObjects;
    public List<PlayerData> storedPlayersListForReset = new List<PlayerData>();
    public List<PlayerData> storedPlayersListForNext = new List<PlayerData>();

    [HideInInspector] public float globalTimer;

    public bool shootingOnlyInMove;
    public bool inMove;

    public List<Transform> pattern_1_Positions;
    public List<Transform> pattern_2_Positions;
    public List<Transform> pattern_3_Positions;
    public Transform centerPosition;

    public Enemy Boss;

    public bool readyToGo;
    public Image HandImage;
    public Text LevelName;

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

        StartCoroutine(resetToFirstLevel());
    }

    void Start()
    {
        StartScene();
    }

    void StartScene()
    {
        inMove = true;
        Boss = null;

        readyToGo = false;

        LevelName.text = "LEVEL 1";

        messagePanel.text = "";
        globalTimer = 0;
        repeatButton.SetActive(false);
        resetButton.SetActive(false);
        NextButton.SetActive(false);

        Party.position = new Vector3(0, 0, -35);

        //int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
        //if (curSceneIndex == 1)
        //{
        //    Player plr = Instantiate(PlayerPrefab_Range).GetComponent<Player>();
        //    plr.transform.SetParent(PartyPositionList[0]);
        //    plr.transform.localPosition = Vector3.zero;
        //    plr.inJail = false;
        //    plr.inParty = true;
        //    plr.coll.enabled = true;
        //    playersInParty.Add(plr);
        //}

        foreach (Player p in playersInParty)
        {
            Transform hPanelp = Instantiate(healthPanelPrefabE).transform;
            hPanelp.SetParent(healthPanelsPool);
            hPanelp.localScale = new Vector3(1, 1, 1);
            p.healthPanel = hPanelp;
            //p.healthPanelFill = hPanelp.GetChild(0).GetComponent<Image>();
            p.healthPanelScript = hPanelp.GetComponent<HealthPanel>();
            p.healthPanel.gameObject.SetActive(false);
        }

        foreach (Player p in FindObjectsOfType<Player>())
        {
            p.StartScene();
        }

        foreach (Enemy e in FindObjectsOfType<Enemy>())
        {
            // инстанциируем для врагов хэлс бары
            Transform hPanele = Instantiate(healthPanelPrefabE).transform;
            hPanele.SetParent(healthPanelsPool);
            hPanele.localScale = new Vector3(1, 1, 1);
            e.healthPanel = hPanele;
            e.healthPanelScript = hPanele.GetComponent<HealthPanel>();
            e.healthPanel.gameObject.SetActive(false);

            e.StartScene();
        }

        foreach (Jail j in FindObjectsOfType<Jail>())
        {
            j.StartScene();
        }

        foreach (Obstacles o in FindObjectsOfType<Obstacles>())
        {
            o.StartScene();
        }

        FinishLine Finish = FindObjectOfType<FinishLine>();
        if (Finish != null) Finish.main = this;

        RefreshPartyPositions(null);

        GameObject level = GameObject.Find("Level");
        if (level != null) Level = level.transform;
    }


    void Update()
    {
        //foreach (Touch touch in Input.touches)
        //{
        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        //Ray ray = Camera.main.ScreenPointToRay(touch.position);
        //        //if (Physics.Raycast(ray))
        //        //{
        //        //
        //        //}
        //    }
        //}
        if (shootingOnlyInMove) inMove = false;
        if (Input.GetMouseButton(0))
        {
            if (!readyToGo)
            {
                readyToGo = true;
                HandImage.gameObject.SetActive(false);
                return;
            }

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

                    newPartyPoint = new Vector3(xPos, 0, -35);


                    Party.position = Vector3.Slerp(Party.position, newPartyPoint, Time.deltaTime * partyMoveSpeed);
                }
            }
        }
    }

    public void WorldUpdate()
    {
        Level.GetChild(0).position += new Vector3(0, 0, worldSize * 3);
        Level.GetChild(0).SetSiblingIndex(2);
    }

    private void LateUpdate()
    {
        if (!readyToGo) return;

        if (Level != null) Level.Translate(Vector3.back * Time.deltaTime * worldMoveSpeed, Space.World);

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

            RefreshPartyPositions(p);

            Destroy(p.healthPanel.gameObject);
            Destroy(p.gameObject);

            Transform deathEffect = deathEffectsPool.GetChild(0);
            deathEffect.SetParent(Level);
            deathEffect.position = p.transform.position;

            yield return new WaitForSeconds(1);

            playersInParty.Remove(p);

            deathEffect.SetParent(deathEffectsPool);

            if (playersInParty.Count == 0)
            {
                // находим всех игроков вне пати, но уже и не в тюрьме
                List<Player> freePlrs = Party.GetComponentsInChildren<Player>().Select(x => x).Where(x => !x.inParty).ToList();
                // если освобожденных игроков нет, то конец игры
                if (freePlrs.Count == 0)
                {
                    Level = null;
                    messagePanel.gameObject.SetActive(true);
                    messagePanel.text = "Sorry, you lose.\nTry again!";
                    repeatButton.SetActive(true);
                    resetButton.SetActive(true);
                    NextButton.SetActive(false);
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
            //e.healthPanel.GetComponent<Image>().enabled = false;
            //e.healthPanel.GetComponentInChildren<Image>().enabled = false;
            Destroy(e.healthPanel.gameObject);

            if (e.isBoss)
            {
                storedPlayersListForNext.Clear();
                foreach (Player p in playersInParty)
                {
                    storedPlayersListForNext.Add(new PlayerData(p));
                }

                foreach (Player p in playersInParty)
                {
                    Destroy(p.healthPanel.gameObject);
                }

                messagePanel.gameObject.SetActive(true);
                messagePanel.text = "Good job!\nIt took you " + globalTimer.ToString("F0") + " seconds";
                repeatButton.SetActive(true);
                resetButton.SetActive(true);
                NextButton.SetActive(true);

                Level = null;
            }

            e.enabled = false;
            foreach (MeshRenderer mr in e.GetComponentsInChildren<MeshRenderer>()) mr.enabled = false;
            e.GetComponent<Collider>().enabled = false;

            Transform deathEffect = deathEffectsPool.GetChild(0);
            deathEffect.SetParent(Level);
            deathEffect.position = e.transform.position;

            yield return new WaitForSeconds(1);

            deathEffect.SetParent(deathEffectsPool);
            Destroy(e.gameObject);
            //Destroy(e.healthPanel.gameObject);
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

    public void RefreshPartyPositions(Player plr)
    {
        Transform LastPlayerParent = null;
        Transform LastMeleeParent = null;
        List<Player> melees;
        List<Player> supports;

        int indexOfLastPlayerParent = -1;
        int indexOfLastMeleeParent = -1;

        List<Player> fullPlayerStack = Party.GetComponentsInChildren<Player>().ToList();
        if (plr != null)
        {
            if (fullPlayerStack.Count == partyPlayerLimit)
            {
                plr.inJail = true;
                plr.inParty = false;
                playersInParty.Remove(plr);
                Destroy(plr.gameObject);

                //лечим всех
                foreach (Player p in playersInParty)
                {
                    p.curHealthPoint += 2;
                    p.healthPanelScript.HealFunction(p.curHealthPoint / p.maxHealthPoint, 2);
                    if (p.curHealthPoint >= p.maxHealthPoint)
                    {
                        p.curHealthPoint = p.maxHealthPoint;
                        if (p.healthPanel != null) p.healthPanel.gameObject.SetActive(false);
                    }
                }

                return;
            }
            else
            {
                fullPlayerStack.Add(plr);
            }
        }

        melees = fullPlayerStack.Select(x => x).Where(x => x.type == playerType.Melee && x.curHealthPoint > 0).ToList();
        supports = fullPlayerStack.Select(x => x).Where(x => x.type != playerType.Melee && x.curHealthPoint > 0).ToList();

        if (melees.Count != 0)
        {
            LastMeleeParent = melees.Last().transform.parent;
            indexOfLastMeleeParent = PartyPositionList.IndexOf(LastMeleeParent);
        }
        else LastMeleeParent = null;
        if (supports.Count != 0)
        {
            LastPlayerParent = supports.Last().transform.parent;
            indexOfLastPlayerParent = PartyPositionList.IndexOf(LastPlayerParent);
        }
        else LastPlayerParent = null;

        for (int i = 0; i < melees.Count; i++)
        {
            melees[i].newOffsetPos = Vector3.zero;
            melees[i].transform.SetParent(PartyPositionList[i]);
            melees[i].changeParent = true;
        }
        for (int i = 0; i < supports.Count; i++)
        {
            supports[i].newOffsetPos = Vector3.zero;
            if (melees.Count > 0 && melees.Count <= 3) supports[i].transform.SetParent(PartyPositionList[i + 3]);
            else supports[i].transform.SetParent(PartyPositionList[i + melees.Count]);
            supports[i].changeParent = true;
        }

        for (int i = 0; i < RowPositionList.Count; i++)
        {
            if (RowPositionList[i].GetComponentsInChildren<Player>().Select(x => x).Where(x => x.curHealthPoint > 0).Count() == 2)
            {
                RowPositionList[i].localPosition = new Vector3(0.3f, RowPositionList[i].localPosition.y, RowPositionList[i].localPosition.z);
            }
            else
            {
                RowPositionList[i].localPosition = new Vector3(0, RowPositionList[i].localPosition.y, RowPositionList[i].localPosition.z);
            }
        }
    }


    public void ResetCurrentLevel()
    {
        StartCoroutine(resetCurrentLevel());
    }

    public void ResetGame()
    {
        StartCoroutine(resetToFirstLevel());
    }

    IEnumerator resetCurrentLevel()
    {
        int curSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (curSceneIndex > 1)
        {
            foreach (GameObject go in dontDestroyOnLoadGameObjects)
            {
                DontDestroyOnLoad(go);
            }

            foreach (Player p in playersInParty)
            {
                Destroy(p.gameObject);
            }
            playersInParty.Clear();

            foreach (Transform tr in healthPanelsPool)
            {
                Destroy(tr.gameObject);
            }

            yield return null;

            AsyncOperation operation = SceneManager.LoadSceneAsync(curSceneIndex);
            while (!operation.isDone)
            {
                yield return null;
            }

            for (int i = 0; i < storedPlayersListForReset.Count; i++)
            {
                Player plr = null;
                if (storedPlayersListForNext[i].type == playerType.Range)
                {
                    plr = Instantiate(PlayerPrefab_Range).GetComponent<Player>();
                }
                else if (storedPlayersListForNext[i].type == playerType.Melee)
                {
                    plr = Instantiate(PlayerPrefab_Melee).GetComponent<Player>();
                }
                else if (storedPlayersListForNext[i].type == playerType.Heal)
                {
                    plr = Instantiate(PlayerPrefab_Heal).GetComponent<Player>();
                }
                plr.transform.SetParent(PartyPositionList[i]);
                plr.transform.localPosition = Vector3.zero;
                plr.inJail = false;
                plr.inParty = true;
                plr.coll.enabled = true;
                playersInParty.Add(plr);

                PlayerDataToPlayer(storedPlayersListForNext[i], plr);
            }

            //прогрузилась сцена
            yield return null;
            StartScene();

            LevelName.text = "LEVEL " + (curSceneIndex).ToString();
        }
        else
        {
            StartCoroutine(resetToFirstLevel());
        }
    }

    IEnumerator resetToFirstLevel()
    {
        int curSceneIndex = 1;
        foreach (GameObject go in dontDestroyOnLoadGameObjects)
        {
            DontDestroyOnLoad(go);
        }

        foreach (Player p in playersInParty)
        {
            Destroy(p.gameObject);
        }
        playersInParty.Clear();

        foreach (Transform tr in healthPanelsPool)
        {
            Destroy(tr.gameObject);
        }

        storedPlayersListForReset.Clear();
        storedPlayersListForNext.Clear();

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(curSceneIndex);
        while (!operation.isDone)
        {
            yield return null;
        }

        Player plr = Instantiate(PlayerPrefab_Range).GetComponent<Player>();
        plr.transform.SetParent(PartyPositionList[0]);
        plr.transform.localPosition = Vector3.zero;
        plr.inJail = false;
        plr.inParty = true;
        plr.coll.enabled = true;
        playersInParty.Add(plr);

        //прогрузилась сцена
        yield return null;
        StartScene();

        HandImage.gameObject.SetActive(true);
    }

    public void LoadNextLevel()
    {
        StartCoroutine(loadNextLevel());
    }

    IEnumerator loadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (SceneManager.sceneCountInBuildSettings == nextSceneIndex)
        {
            messagePanel.text = "Ты прошел все уровни!\nХочешь играть дальше?\nСоздай новые!";
            yield break;
        }

        yield return null;

        foreach (GameObject go in dontDestroyOnLoadGameObjects)
        {
            DontDestroyOnLoad(go);
        }

        storedPlayersListForReset = storedPlayersListForNext;

        foreach (Player p in playersInParty)
        {
            Destroy(p.gameObject);
        }
        playersInParty.Clear();

        foreach (Transform tr in healthPanelsPool)
        {
            Destroy(tr.gameObject);
        }

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneIndex);
        while (!operation.isDone)
        {
            yield return null;
        }

        for (int i = 0; i < storedPlayersListForNext.Count; i++)
        {
            Player plr = null;
            if (storedPlayersListForNext[i].type == playerType.Range)
            {
                plr = Instantiate(PlayerPrefab_Range).GetComponent<Player>();
            }
            else if (storedPlayersListForNext[i].type == playerType.Melee)
            {
                plr = Instantiate(PlayerPrefab_Melee).GetComponent<Player>();
            }
            else if (storedPlayersListForNext[i].type == playerType.Heal)
            {
                plr = Instantiate(PlayerPrefab_Heal).GetComponent<Player>();
            }
            plr.transform.SetParent(PartyPositionList[i]);
            plr.transform.localPosition = Vector3.zero;
            plr.inJail = false;
            plr.inParty = true;
            plr.coll.enabled = true;
            playersInParty.Add(plr);

            PlayerDataToPlayer(storedPlayersListForNext[i], plr);
        }

        // прогрузилась сцена
        yield return null;
        StartScene();

        LevelName.text = "LEVEL " + (nextSceneIndex).ToString();
    }



    public void PlayerDataToPlayer(PlayerData pd, Player p)
    {
        p.type = pd.type;
        p.shootRange = pd.shootRange;
        p.shootSpreadCoeff = pd.shootSpreadCoeff;
        p.rocketDamage = pd.rocketDamage;
        p.rocketSpeed = pd.rocketSpeed;
        p.reloadingTime = pd.reloadingTime;
        p.rotateSpeed = pd.rotateSpeed;
        p.maxHealthPoint = pd.maxHealthPoint;
        p.healPointsRecoveryCount = pd.healPointsRecoveryCount;
        p.healReloadingTime = pd.healReloadingTime;
        p.bodyColor = pd.bodyColor;
        p.rocketColor = pd.rocketColor;
        p.rocketSize = pd.rocketSize;
    }

}
