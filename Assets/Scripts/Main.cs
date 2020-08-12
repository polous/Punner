using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Transform Level;
    int worldSize = 80;
    public float xPos;
    public float xLimit;
    public Player player;

    public Transform Party;
    public Transform Party_M, Party_R, Party_H;
    public List<Player> playersInParty = new List<Player>();

    public float worldMoveSpeed;

    Vector3 newPoint;

    public GameObject rocketPrefab; // префаб ракеты
    public Transform rocketsPool; // пул прожектайлов
    public GameObject healthPanelPrefab; // префаб UI панели здоровья
    public Transform healthPanelsPool; // пул UI панелей здоровья

    void Awake()
    {
        // заполняем пул прожектайлов
        for (int i = 0; i < 300; i++)
        {
            GameObject rocket = Instantiate(rocketPrefab) as GameObject;
            rocket.transform.SetParent(rocketsPool);
            rocket.GetComponent<Rocket>().main = this;
        }
    }

    void Start()
    {
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

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1 << 8))
            {
                xPos = hit.point.x;
                if (Mathf.Abs(xPos) > xLimit)
                {
                    if (xPos > 0) xPos = xLimit;
                    if (xPos < 0) xPos = -xLimit;
                }
                newPoint = new Vector3(xPos, Party.position.y, Party.position.z);


                Party.position = Vector3.Lerp(Party.position, newPoint, Time.deltaTime * player.moveSpeed);
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
    }

}
