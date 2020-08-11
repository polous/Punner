using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jail : MonoBehaviour
{
    public float curHP;
    public TextMesh textMesh;

    // Start is called before the first frame update
    void Start()
    {
        curHP = Mathf.RoundToInt(Random.Range(50, 150));
        textMesh.text = curHP.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
