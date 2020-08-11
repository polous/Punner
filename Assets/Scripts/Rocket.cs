using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rocket : MonoBehaviour
{
    public float damage; // урон
    public float maxRange; // предельная дистанция полета
    public float speed; // скорость полета 
    public Vector3 startPoint; // начальная точка полета    
    public string MyShooterTag; // тэг стреляющего НПС
    public bool flying; // летит ли?
    public Vector3 direction; // направление полета

    public float spreadCoeff; // разброс относительно точного направления на цель

    public Rigidbody rb;
    public Main main;

    void Update()
    {
        if (flying)
        {
            transform.position += direction.normalized * speed * Time.deltaTime;

            // если прожектайл летит и достиг максимальной длины полета - возвращаем в пул
            if ((startPoint - transform.position).magnitude >= maxRange)
            {
                transform.SetParent(main.rocketsPool);
                flying = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // если прожектайл столкнулся с препятствием - возвращаем в пул
        if (other.tag == "Obstacles")
        {
            transform.SetParent(main.rocketsPool);
            flying = false;
        }
        else if (other.tag == "Jail")
        {
            transform.SetParent(main.rocketsPool);
            flying = false;

            Jail jail = other.GetComponent<Jail>();
            jail.curHP -= damage;
            jail.textMesh.text = jail.curHP.ToString();

            if (jail.curHP <= 0)
            {
                Destroy(jail.gameObject);
            }
        }
        else if (other.tag == "Player")
        {
            if (other.tag == MyShooterTag)
            {
                print("");
            }
        }


        //// если прожектайл столкнулся с препятствием - возвращаем в пул
        //if (other.gameObject.layer == 9)
        //{
        //    //transform.SetParent(main.rocketsPool);
        //    flying = false;
        //}
        //// если прожектайл столкнулся с НПС - наносим урон - возвращаем в пул
        //else if (other.gameObject.layer == 10)
        //{
        //    if (other.tag != MyShooterTag) // исключаем самопоражение и фрэндли файр
        //    {
        //        if (other.tag == "Enemy")
        //        {

        //        }

        //        else if (other.tag == "Player")
        //        {

        //        }

        //        flying = false;
        //        //transform.SetParent(main.rocketsPool);
        //    }
        //}
    }
}
