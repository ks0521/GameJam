using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillManager : MonoBehaviour
{
    [SerializeField] List<SkillSO> skills;
    [SerializeField] SkillSO seledtedSkill;
    [SerializeField] PlayerFlickerMove player;
    [SerializeField] DiceManager diceManager;
    [SerializeField] Image image;
    // Start is called before the first frame update
    void Start()
    {
        ChangeSkill(skills[0]);
        if(player == null)
        {
            player = FindObjectOfType<PlayerFlickerMove>();
        }
    }
    public void UseSkill()
    {
        diceManager.RollDice(seledtedSkill.diceCount,seledtedSkill.diceType);
        player.Mp -= seledtedSkill.Cost;
        ChangeSkill(skills[0]);
    }
    public void ChangeSkill(SkillSO skill)
    {
        seledtedSkill = skill;
        image.sprite = seledtedSkill.images;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (skills[0].Cost <= player.Mp)
            {
                ChangeSkill(skills[0]);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (skills[1].Cost <= player.Mp)
            {
                ChangeSkill(skills[1]);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (skills[2].Cost <= player.Mp)
            {
                ChangeSkill(skills[2]);
            }
        }
    }

}
