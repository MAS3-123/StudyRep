using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType
    {
        PowerUp,
        HpRecovery,
    }

    [SerializeField] private ItemType itemType;
    private Vector3 movePose;//움직이는 방향
    private float speed;//속도

    private bool checkViewPoint = false;

    private void Awake()
    {
        movePose = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        speed = Random.Range(2.0f,5.0f);
        checkStartPos();
    }

    void Update()
    {
        transform.position += movePose * speed * Time.deltaTime;
        checkPos();
    }

    private void checkStartPos()
    {
        Vector3 currentPos = Camera.main.WorldToViewportPoint(transform.position);

        if (currentPos.x < 0)
        {
            movePose = new Vector3(Random.Range(-1.0f, -0.1f), Random.Range(-1.0f, 1.0f));
        }
        if (currentPos.x > 1)
        {
            movePose = new Vector3(Random.Range(0.1f, 1.0f), Random.Range(-1.0f, 1.0f));
        }
        if (currentPos.y < 0)
        {
            movePose = new Vector3(Random.Range(-1.0f, -0.1f), Random.Range(-1.0f, -0.1f));
        }
        if (currentPos.y > 1)
        {
            movePose = new Vector3(Random.Range(-1.0f, -0.1f), Random.Range(0.1f, 1.0f));
        }
    }

    private void checkPos()
    {
        Vector3 currentPos = Camera.main.WorldToViewportPoint(transform.position);

        //if (currentPos.x < 0f)
        //{
        //    movePose = Vector3.Reflect(movePose, Vector3.left);
        //}   // 기존 코드

        if (currentPos.x < 0f && checkViewPoint == false)
        {
            movePose = Vector3.Reflect(movePose, Vector3.left);
            checkViewPoint = true;
        }
        else if(currentPos.x > 0f && currentPos.x < 1)
        {
            checkViewPoint = false;
        }  // 변경 코드

        //else if (currentPos.x > 1f)
        //{
        //    movePose = Vector3.Reflect(movePose, Vector3.right);
        //}

        if (currentPos.x > 1f && checkViewPoint == false)
        {
            movePose = Vector3.Reflect(movePose, Vector3.right);
            checkViewPoint = true;
        }
        else if (currentPos.x > 0f && currentPos.x < 1)
        {
            checkViewPoint = false;
        }  // 변경 코드

        if (currentPos.y < 0f)
        {
            movePose = Vector3.Reflect(movePose, Vector3.down);
        }
        else if (currentPos.y > 1f)
        {
            movePose = Vector3.Reflect(movePose, Vector3.up);
        }
    }

    public ItemType GetItemType()
    {
        return itemType;
    }
}
