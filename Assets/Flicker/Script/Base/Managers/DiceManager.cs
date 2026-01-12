using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DiceManager : MonoBehaviour
{
    public static DiceManager instance;
    [SerializeField] GameObject dice;
    [SerializeField]List<GameObject> diceList;
    [SerializeField] GameObject HUD;
    [SerializeField] PlayerFlickerMove player;
    public event Action<int> OnDiceResult;
    public int ValueSum { get; private set; }
    DiceType nowType; //현재 주사위를 굴리고 있는 타입
    int maxCompleteCount;
    int completeCount;
    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        instance = this;
        diceList = new List<GameObject>();
    }
    /// <summary>
    /// count개의 주사위 굴림
    /// </summary>
    /// <param name="count">주사위 갯수</param>
    public void RollDice(int count, DiceType type)
    {
        nowType = type;
        HUD.SetActive(false);
        completeCount = 0;
        maxCompleteCount = count;
        ValueSum = 0;
        for(int i = 0; i < count; i++)
        {
            diceList.Add(PoolManager.poolDic[nowType].UsePool(transform.position,Quaternion.identity));
            //diceList.Add(Instantiate(dice, transform.position, Quaternion.identity));
            diceList[i].GetComponent<DiceRoll>().OnResult += AddResult;
            diceList[i].GetComponent<DiceRoll>().Rolling();
        }
    }
    //주사위 결과가 나오면 반영
    void AddResult(int value)
    {
        ValueSum += value;
        completeCount++;
        if(completeCount == maxCompleteCount)
        {
            CalcEnd();
        }
    }
    public void CalcEnd()
    {
        Debug.Log($"계산 결과 : {ValueSum}");
        //모든 주사위의 결과가 나오면 기존 주사위 다 반환하고 리스트 정리
        for(int i = 0; i < maxCompleteCount; i++)
        {
            diceList[i].GetComponent<DiceRoll>().OnResult -= AddResult;
            PoolManager.poolDic[nowType].ReturnPool(diceList[i]);
        }
        if(ValueSum<=2)
        {
            Debug.Log("참가상! 마나+2");
            
        }
        diceList.Clear();
        OnDiceResult?.Invoke(ValueSum);
        HUD.SetActive(true);
        ValueSum = 0;
    }

}
