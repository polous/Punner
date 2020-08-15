using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class Main : MonoBehaviour
{
    [HideInInspector] public Transform Level;
    int worldSize = 80;
    [HideInInspector] public float xPos;
    public float xLimit;
    public float xOffSet;

    public Transform Party;
    public Transform Party_M, Party_R, Party_H;
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
    }

    void Start()
    {
        messagePanel.text = "";
        globalTimer = 0;
        repeatButton.SetActive(false);

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
        if(shootingOnlyInMove) inMove = false;
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

}
