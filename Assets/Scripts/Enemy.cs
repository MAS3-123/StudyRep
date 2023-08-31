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

    [Header("적 스텟")]
    [SerializeField] private float Hp = 5f;
    private float maxHp = 0;
    [SerializeField] private float Speed = 1f;

    [SerializeField] Sprite[] arrSprite;//0 기본상태 1 공격받았을때
    private SpriteRenderer Sr;

    [SerializeField] private GameObject objExplosion;
    private bool haveItem = false;
    private bool isAlive = true;

    [Header("보스")]
    private bool isStartingMove = false;//보스가 출동했을때 기본 이동을 했는지 파악
    private float startPointY;//보스 생성 최초 위치
    private float startPointX;//보스 생성 최초 위치
    private float ratioY = 0.0f;//어디까지 이동 했는지 비율 데이터
    private bool isSwayRight = false;//좌우로 이동 시 어디로 이동해야 하는지

    [Header("보스 패턴")]
    //앞으로 발사 1
    [SerializeField] private int pattern1Count = 10;
    [SerializeField] private float pattern1Reload = 1f;
    [SerializeField] private GameObject pattern1Bullet;
    [SerializeField] private float pattern1Speed = 8f;
    [Space]
    //샷건 2
    [SerializeField] private int pattern2Count = 5;
    [SerializeField] private float pattern2Reload = 1f;
    [SerializeField] private GameObject pattern2Bullet;
    [SerializeField] private float pattern2Speed = 8f;
    [Space]
    //조준 3
    [SerializeField] private int pattern3Count = 30;
    [SerializeField] private float pattern3Reload = 0.2f;
    [SerializeField] private GameObject pattern3Bullet;
    [SerializeField] private float pattern3Speed = 8f;

    private int curPattern = 1;//현재 패턴
    private int curPatternShootCount = 0;// 현재 패턴 카운트
    private float curPatternTimer = 0.0f;//현재 패턴 타이머
    private float patternChangeTime = 0.5f;
    private bool patternChange = false;//패턴을 바꿔야 하는지

    private Transform trsPlayer;

    private void OnDestroy()//진짜 삭제될때 한번만 사용되는 함수
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
        if(enemyType != EnemyType.EnemyBoss)//보스가 아닐경우
        {
            transform.position += Vector3.down * Speed * Time.deltaTime;
        }
        else//보스일때
        {
            if(isStartingMove == false)
            {
                //스타팅 무브
                bossStartMove();
            }
            else
            {
                //좌우로 무빙
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

        //패턴이 변경되며 잠시동안 플레이어가 공격할 시간을 만들어줌
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

        switch(curPattern)//현재 패턴 값
        {
            case 1://전방 3발 발사
                curPattern = bulletPattern(1, pattern1Reload, pattern1Count);
                //if (curPatternTimer >= pattern1Reload)
                //{
                //    curPatternTimer = 0.0f;
                //    shootStraight();
                //    if (pattern1Count <= curPatternShootCount)//패턴 교체
                //    {
                //        curPattern++;//case1 > case2 / 랜덤으로 해보는거 숙제?
                //        patternChange = true;
                //        curPatternShootCount = 0;
                //    }
                //}
                break;

            case 2://샷건 패턴
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

            case 3://조준 패턴
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

        if(_maxRangeVal > 0) //최대 각도 확인
        {
            for (int iNum = 0; iNum < shotBulletEA; iNum++)
            {
                if (iNum == 0)
                {
                    shots.Add(bulletRange); // 정 중앙 총알
                }
                else
                {
                    bulletRange = _maxRangeVal / shotBulletEA; // 최대 각도에서 중앙 까지 나뉘는 총알개수
                    bulletRange = bulletRange * iNum;  // 균등한 간격으로 발사
                    shots.Add(bulletRange);            // 양옆으로 발사하기 위해 +-로 저장
                    shots.Add(-bulletRange);
                }
            }
        }
        else //따로 각도가 없을경우
        {
            shots.Add(bulletRange);
        }

        int count = shots.Count;

        if(_xx > 0) // 중앙에서 발사 할 경우
        {   
            creatBullet(pattern1Bullet, transform.position, new Vector3(0, _yy, 180.0f), _bSpeed);
            creatBullet(pattern1Bullet, transform.position + new Vector3(_xx, _yy, 0), new Vector3(0, 0, 180.0f), _bSpeed);
            creatBullet(pattern1Bullet, transform.position + new Vector3(-_xx, _yy, 0), new Vector3(0, 0, 180.0f), _bSpeed);
        }
        else //증잉에서 발사가 아닐경우
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
