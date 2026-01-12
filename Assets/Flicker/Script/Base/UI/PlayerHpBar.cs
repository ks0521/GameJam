using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpBar : MonoBehaviour
{
    [SerializeField]
    public Image images;
    [SerializeField]
    PlayerFlickerMove player;
    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<PlayerFlickerMove>();
    }
    private void OnEnable()
    {
        if (player != null)
            player.onPlayerHpChange += SetHp;
    }
    private void OnDisable()
    {
        if (player != null)
            player.onPlayerHpChange -= SetHp;
    }

    public void SetHp(float hp, float maxHp)
    {
        if (images == null) return;
        //Debug.Log($"{hp}, {maxHp}");
        images.fillAmount = (float)hp / (float)maxHp;
    }
}
