using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMpText : MonoBehaviour
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
            player.onPlayerMpChange += SetMp;
    }
    private void OnDisable()
    {
        if (player != null)
            player.onPlayerMpChange -= SetMp;
    }

    void SetMp(int mp, int maxMp)
    {
        Debug.Log("mp바뀜");
        text.text = $"{mp} / {maxMp}";
    }

}
