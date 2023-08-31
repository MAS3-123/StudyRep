using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public enum EnemyType
    {
        EnemyA,
        EnemyB,
        EnemyC,
        EnemyBoss
    }
    [SerializeField] private EnemyType enemyType;

    [Header("�� ����")]
    [SerializeField] private float Hp = 5f;
    private float maxHp = 0;
    [SerializeField] private float Speed = 1f;

    [SerializeField] Sprite[] arrSprite;//0 �⺻���� 1 ���ݹ޾�����
    private SpriteRenderer Sr;

    [SerializeField] private GameObject objExplosion;
    private bool haveItem = false;
    private bool isAlive = true;

    [Header("����")]
    private bool isStartingMove = false;//������ �⵿������ �⺻ �̵��� �ߴ��� �ľ�
    private float startPointY;//���� ���� ���� ��ġ
    private float startPointX;//���� ���� ���� ��ġ
    private float ratioY = 0.0f;//������ �̵� �ߴ��� ���� ������
    private bool isSwayRight = false;//�¿�� �̵� �� ���� �̵��ؾ� �ϴ���

    [Header("���� ����")]
    //������ �߻� 1
    [SerializeField] private int pattern1Count = 10;
    [SerializeField] private float pattern1Reload = 1f;
    [SerializeField] private GameObject pattern1Bullet;
    [SerializeField] private float pattern1Speed = 8f;
    [Space]
    //���� 2
    [SerializeField] private int pattern2Count = 5;
    [SerializeField] private float pattern2Reload = 1f;
    [SerializeField] private GameObject pattern2Bullet;
    [SerializeField] private float pattern2Speed = 8f;
    [Space]
    //���� 3
    [SerializeField] private int pattern3Count = 30;
    [SerializeField] private float pattern3Reload = 0.2f;
    [SerializeField] private GameObject pattern3Bullet;
    [SerializeField] private float pattern3Speed = 8f;

    private int curPattern = 1;//���� ����
    private int curPatternShootCount = 0;// ���� ���� ī��Ʈ
    private float curPatternTimer = 0.0f;//���� ���� Ÿ�̸�
    private float patternChangeTime = 0.5f;
    private bool patternChange = false;//������ �ٲ�� �ϴ���

    private Transform trsPlayer;

    private void OnDestroy()//��¥ �����ɶ� �ѹ��� ���Ǵ� �Լ�
    {
        GameManager.Instance.RemoveEnemy(gameObject);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void Awake()
    {
        Sr = GetComponent<SpriteRenderer>();
        startPointY = transform.position.y;
        startPointX = transform.position.x;
        maxHp = Hp;
    }

    private void Start()
    {
        trsPlayer = GameManager.Instance.GetPlayerTransform();
        GameManager.Instance.CheckBossHP(Hp, maxHp);
    }

    void Update()
    {
        moving();
        doPattern();
    }

    private void moving()
    {
        if(enemyType != EnemyType.EnemyBoss)//������ �ƴҰ��
        {
            transform.position += Vector3.down * Speed * Time.deltaTime;
        }
        else//�����϶�
        {
            if(isStartingMove == false)
            {
                //��Ÿ�� ����
                bossStartMove();
            }
            else
            {
                //�¿�� ����
                bossSwayMove();
            }
        }
    }

    private void doPattern()
    {
        if(enemyType != EnemyType.EnemyBoss || isStartingMove == false)
        {
            return;
        }

        //������ ����Ǹ� ��õ��� �÷��̾ ������ �ð��� �������
        curPatternTimer += Time.deltaTime;
        if(patternChange == true)
        {
            if(curPatternTimer >= patternChangeTime)
            {
                curPatternTimer = 0.0f;
                patternChange = false;
            }
            return;
        }

        switch(curPattern)//���� ���� ��
        {
            case 1://���� 3�� �߻�
                curPattern = bulletPattern(1, pattern1Reload, pattern1Count);
                //if (curPatternTimer >= pattern1Reload)
                //{
                //    curPatternTimer = 0.0f;
                //    shootStraight();
                //    if (pattern1Count <= curPatternShootCount)//���� ��ü
                //    {
                //        curPattern++;//case1 > case2 / �������� �غ��°� ����?
                //        patternChange = true;
                //        curPatternShootCount = 0;
                //    }
                //}
                break;

            case 2://���� ����
                curPattern = bulletPattern(2, pattern2Reload, pattern2Count);
                //if (curPatternTimer >= pattern2Reload)
                //{
                //    curPatternTimer = 0.0f;
                //    shootShotgun();
                //    if (pattern2Count <= curPatternShootCount)
                //    {
                //        curPattern++;
                //        patternChange = true;
                //        curPatternShootCount = 0;
                //    }
                //}
                break;

            case 3://���� ����
                curPattern = bulletPattern(3, pattern3Reload, pattern3Count);
                //if (curPatternTimer >= pattern3Reload)
                //{
                //    curPatternTimer = 0.0f;
                //    shootGatling();
                //    if (pattern3Count <= curPatternShootCount)
                //    {
                //        curPattern = 1;
                //    }
                //}
                break;
        }
    }

    private void bulletVector(float _xx, float _yy,float _maxRangeVal, int shotBulletEA, float _bSpeed)
    {
        List<float> shots = new List<float>();

        float bulletRange = 0.0f;

        if(_maxRangeVal > 0) //�ִ� ���� Ȯ��
        {
            for (int iNum = 0; iNum < shotBulletEA; iNum++)
            {
                if (iNum == 0)
                {
                    shots.Add(bulletRange); // �� �߾� �Ѿ�
                }
                else
                {
                    bulletRange = _maxRangeVal / shotBulletEA; // �ִ� �������� �߾� ���� ������ �Ѿ˰���
                    bulletRange = bulletRange * iNum;  // �յ��� �������� �߻�
                    shots.Add(bulletRange);            // �翷���� �߻��ϱ� ���� +-�� ����
                    shots.Add(-bulletRange);
                }
            }
        }
        else //���� ������ �������
        {
            shots.Add(bulletRange);
        }

        int count = shots.Count;

        if(_xx > 0) // �߾ӿ��� �߻� �� ���
        {   
            creatBullet(pattern1Bullet, transform.position, new Vector3(0, _yy, 180.0f), _bSpeed);
            creatBullet(pattern1Bullet, transform.position + new Vector3(_xx, _yy, 0), new Vector3(0, 0, 180.0f), _bSpeed);
            creatBullet(pattern1Bullet, transform.position + new Vector3(-_xx, _yy, 0), new Vector3(0, 0, 180.0f), _bSpeed);
        }
        else //���׿��� �߻簡 �ƴҰ��
        {
            for (int iNum = 0; iNum < count; iNum++)
            {
                creatBullet(pattern1Bullet, transform.position, new Vector3(_xx, _yy, 180.0f + shots[iNum]), _bSpeed);
            }
        }
        
    }
    private void shootStraight(float _bSpeed)
    {
        bulletVector(1.0f, 0.0f, 0.0f, 0, _bSpeed);
        //creatBullet(pattern1Bullet, transform.position, new Vector3(0, 0, 180.0f));
        //creatBullet(pattern1Bullet, transform.position + new Vector3(-1.0f,0,0), new Vector3(0, 0, 180.0f));
        //creatBullet(pattern1Bullet, transform.position + new Vector3(1.0f, 0, 0), new Vector3(0, 0, 180.0f));
        curPatternShootCount++;
    }

    private void shootShotgun(float _bSpeed)
    {
        bulletVector(0.0f, 0.0f, 60.0f, 4, _bSpeed);
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f - 60.0f));
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f - 45.0f));
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f - 30.0f));
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f - 15.0f));
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f));
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f + 15.0f));
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f + 30.0f));
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f + 45.0f));
        //creatBullet(pattern2Bullet, transform.position, new Vector3(0, 0, 180.0f + 60.0f));
        curPatternShootCount++;
    }

    private void shootGatling(float _bSpeed)
    {
        if(trsPlayer == null)
        {
            return;
        }

        Vector3 playerPos = trsPlayer.position;
        float zAngle = Quaternion.FromToRotation(Vector3.up, playerPos - transform.position).eulerAngles.z;
        creatBullet(pattern3Bullet, transform.position, new Vector3(0, 0, zAngle), _bSpeed);
        curPatternShootCount++;
    }

    private void creatBullet(GameObject _obj, Vector3 _pos, Vector3 _rot, float _bSpeed)
    {
        GameObject obj = Instantiate(_obj, _pos, Quaternion.Euler(_rot), transform.root);
        Bullet sc = obj.GetComponent<Bullet>();
        sc.SetBullet(_bSpeed, 1);
    }

    private int bulletPattern(int _Pattern, float _Reload, int _Count)
    {
        if (curPatternTimer >= _Reload)
        {
            curPatternTimer = 0.0f;

            switch (_Pattern)
            {
                case 1:
                    shootStraight(pattern1Speed);
                    break;
                case 2: 
                    shootShotgun(pattern2Speed); 
                    break;
                case 3:
                    shootGatling(pattern3Speed); 
                    break;
            }

            if (_Count <= curPatternShootCount)
            {
                if(_Pattern < 3)
                {
                    _Pattern++;
                }
                else
                {
                    _Pattern = 1;
                }
                patternChange = true;
                curPatternShootCount = 0;
            }
        }
        //curPattern = _Pattern;
        return _Pattern;
    }


    private void bossStartMove()
    {
        ratioY += Time.deltaTime * 0.5f;
        if (ratioY >= 1.0f)
        {
            isStartingMove = true;
        }

        Vector3 vecPos = transform.position;
        vecPos.y = Mathf.SmoothStep(startPointY, 3.0f, ratioY);
        vecPos.x = Mathf.SmoothStep(startPointX, 0.0f, ratioY);
        transform.position = vecPos;

    }

    private void bossSwayMove()
    {
        if (isSwayRight == true)// right
        {
            transform.position += Vector3.right * Time.deltaTime * Speed;
            checkBossMoveLimit();
        }
        else//left
        {
            transform.position += Vector3.left * Time.deltaTime * Speed;
            checkBossMoveLimit();
        }
    }

    private void checkBossMoveLimit()
    {
        Vector3 currPos = Camera.main.WorldToViewportPoint(transform.position);

        if(isSwayRight == true && currPos.x > 0.95f)
        {
            isSwayRight = false;
        }
        else if (isSwayRight == false && currPos.x < 0.05f)
        {
            isSwayRight= true;
        }
    }


    public void Hit(float _damege)
    {
        if (isAlive == false)
        {
            return;
        }

        Hp -= _damege;
        GameManager.Instance.CheckBossHP(Hp, maxHp);

        if (Hp <= 0)
        {
            if(enemyType == EnemyType.EnemyBoss)
            {
                GameManager.Instance.DestroyBoss();
            }

            isAlive = false;
            ExplosionEnemy();

            GameManager.Instance.AddDestroyCount(enemyType);

            if (haveItem == true)
            {
                GameManager.Instance.CreatItem(transform.position);
            }
        }
        else
        {
            Sr.sprite = arrSprite[1];
            Invoke("defaultSprite", 0.1f);
        }
    }

    public void ExplosionEnemy()
    {
        Destroy(gameObject);

        GameObject obj = Instantiate(objExplosion, transform.position,
            Quaternion.identity, transform.parent);

        Explosion sc = obj.GetComponent<Explosion>();
        sc.SetAnimationSize(Sr.sprite.rect.width);
    }

    private void defaultSprite()
    {
        Sr.sprite = arrSprite[0];
    }

    public void SetHaveItem()
    {
        haveItem = true;
        Sr.color = new Color(0.5f, 0.5f, 1);
    }

    public EnemyType GetEnemyType()
    {
        return enemyType;
    }
}
