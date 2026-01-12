using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpText : MonoBehaviour
{
    [SerializeField]
    Text text; 
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

    void SetHp(float hp, float maxhp)
    {
        text.text = $"{(int)hp} / {(int)maxhp}";
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
