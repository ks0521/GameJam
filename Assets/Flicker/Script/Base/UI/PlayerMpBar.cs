using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMpBar : MonoBehaviour
{
    [SerializeField]
    public Image images;
    [SerializeField]
    PlayerFlickerMove player;
    private void Awake()
    {
        //if (player == null)
            //player = FindObjectOfType<PlayerFlickerMove>();
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

    public void SetMp(int mp, int maxMp)
    {
        if (images == null) return;
        Debug.Log($"{mp}, {maxMp}");
        images.fillAmount = (float)mp / (float)maxMp;
    }
}
