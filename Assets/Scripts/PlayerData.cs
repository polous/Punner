using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData
{
    public playerType type;
    public float shootRange;
    public float shootSpreadCoeff;
    public float rocketDamage;
    public float rocketSpeed;
    public float reloadingTime;
    public float rotateSpeed;
    public float maxHealthPoint;
    public float healPointsRecoveryCount;
    public float healReloadingTime;
    public Color bodyColor;
    public Color rocketColor;
    public float rocketSize;

    public PlayerData(Player p)
    {
        type = p.type;
        shootRange = p.shootRange;
        shootSpreadCoeff = p.shootSpreadCoeff;
        rocketDamage = p.rocketDamage;
        rocketSpeed = p.rocketSpeed;
        reloadingTime = p.reloadingTime;
        rotateSpeed = p.rotateSpeed;
        maxHealthPoint = p.maxHealthPoint;
        healPointsRecoveryCount = p.healPointsRecoveryCount;
        healReloadingTime = p.healReloadingTime;
        bodyColor = p.bodyColor;
        rocketColor = p.rocketColor;
        rocketSize = p.rocketSize;
    }
}
