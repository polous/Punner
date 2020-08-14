using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Jail : MonoBehaviour
{
    [HideInInspector] public float curHP;
    public float Bot, Top;
    public TextMeshPro textMesh;

    public Color bodyColor;
    [HideInInspector] public MaterialPropertyBlock MPB;
    [HideInInspector] public MeshRenderer mr;


    void Start()
    {
        MPB = new MaterialPropertyBlock();
        mr = GetComponent<MeshRenderer>();
        mr.GetPropertyBlock(MPB);
        MPB.SetColor("_Color", bodyColor);
        mr.SetPropertyBlock(MPB);

        curHP = Mathf.RoundToInt(Random.Range(Bot, Top));
        textMesh.text = curHP.ToString();
    }
}
