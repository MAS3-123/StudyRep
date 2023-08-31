using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    private Image HpBack;//연출용
    private Image HpFront;//실제HP
    private Transform trsPlayer;

    void Awake()
    {
        HpBack = transform.GetChild(1).GetComponent<Image>();
        HpFront = transform.GetChild(2).GetComponent<Image>();

        trsPlayer = FindObjectOfType<Player>().transform;
    }

    void Update()
    {
        checkPosition();
        checkPlayerHP();
        isDestorying();
    }

    private void checkPosition()
    {
        if (trsPlayer != null)
        { 
            transform.position = trsPlayer.position + new Vector3(0, -0.7f, 0);
        }
    }

    private void checkPlayerHP()
    {
        if(HpFront.fillAmount < HpBack.fillAmount)//이펙트가 작아져야함
        {
            HpBack.fillAmount -= Time.deltaTime * 0.5f;
            if(HpBack.fillAmount <= HpFront.fillAmount)
            {
                HpBack.fillAmount = HpFront.fillAmount;
            }
        }
        else if(HpBack.fillAmount < HpFront.fillAmount)//이펙트가 HP와 같아져야함
        {
            HpBack.fillAmount = HpFront.fillAmount;
        }
    }

    private void isDestorying()
    {
        if (HpBack.fillAmount <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerHp(int _curHp, int _maxHp)
    {
        HpFront.fillAmount = (float)_curHp / _maxHp;
    }
}
