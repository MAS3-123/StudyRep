using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Fab_Ranking : MonoBehaviour
{
    private bool initData = false;

    private TMP_Text textRank;
    private TMP_Text textScore;
    private TMP_Text textName;

    public void SetRanking(string _rank, string _score, string _name)
    {
        if(initData == false) 
        {
            textRank = transform.Find("TextRank").GetComponent<TMP_Text>();
            textScore = transform.GetChild(1).GetComponent<TMP_Text>();
            textName = GetComponentsInChildren<TMP_Text>()[2];
            initData = true;
        }

        textRank.text = _rank;
        textScore.text = _score;
        textName.text = _name;
    }
}
