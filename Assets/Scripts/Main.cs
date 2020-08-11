using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Main : MonoBehaviour
{
    public Transform Level;
    //public Transform LevelPart1;
    //public Transform LevelPart2;
    int worldSize = 80;
    public float xPos;
    public float xLimit;
    public Player player;

    public Transform Party;

    public float worldMoveSpeed;

    Vector3 newPoint;

    public GameObject rocketPrefab; // префаб ракеты
    public Transform rocketsPool; // пул прожектайлов


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
