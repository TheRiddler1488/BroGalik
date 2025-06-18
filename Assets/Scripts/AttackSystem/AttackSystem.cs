using UnityEngine;
using System.Collections.Generic;

public class AttackSystem : MonoBehaviour
{
    private readonly List<AttackBehaviour> activeAttacks = new();

    public void Register(AttackBehaviour attack)
    {
        if (!activeAttacks.Contains(attack))
            activeAttacks.Add(attack);
    }

    public void Unregister(AttackBehaviour attack)
    {
        activeAttacks.Remove(attack);
    }

    void Update()
    {
        foreach (var attack in activeAttacks)
        {
            attack.Tick(Time.deltaTime);
        }
    }
}