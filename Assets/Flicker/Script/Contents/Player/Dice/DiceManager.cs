using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    [SerializeField] GameObject dice;
    [SerializeField]List<GameObject> diceList; 

    public int ValueSum { get; private set; }
    int maxCompleteCount;
    int completeCount;
    private void Awake()
    {
        diceList = new List<GameObject>();
    }
    void Sum(int value)
    {
        ValueSum += value;
        completeCount++;
        if(completeCount == maxCompleteCount)
        {
            CalcEnd();
        }
    }
    public void RollDice(int count)
    {
        completeCount = 0;
        maxCompleteCount = count;
        ValueSum = 0;
        for(int i = 0; i < count; i++)
        {
            diceList.Add(Instantiate(dice, transform.position, Quaternion.identity));
            diceList[i].GetComponent<DiceRoll>().OnResult += Sum;
            diceList[i].GetComponent<DiceRoll>().Rolling();
        }
    }
    public void CalcEnd()
    {
        Debug.Log($"계산 결과 : {ValueSum}");
        for(int i = 0; i < maxCompleteCount; i++)
        {
            diceList[i].GetComponent<DiceRoll>().OnResult -= Sum;
            Destroy(diceList[i]);
        }
        diceList.Clear();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RollDice(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RollDice(2);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            diceList.Clear();
        }
    }
}
