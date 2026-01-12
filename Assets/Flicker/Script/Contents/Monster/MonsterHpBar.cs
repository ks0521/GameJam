using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHpBar : MonoBehaviour
{
    [SerializeField]
    public Image images;
    [SerializeField]
    Monster monster;
    private void Awake()
    {
        if (monster == null)
            monster = FindObjectOfType<Monster>();
    }
    private void OnEnable()
    {
        if (monster != null)
            monster.onMonstarHpChange += SetHp;
    }
    private void OnDisable()
    {
        if (monster != null)
            monster.onMonstarHpChange -= SetHp;
    }

    public void SetHp(float hp, float maxHp)
    {
        if (images == null) return;
        images.fillAmount = (float)hp / (float)maxHp;
    }
}
