using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("스텟")]
    [SerializeField] private float m_Speed;
    [SerializeField] private int m_maxHp = 5;
    private int m_curHp;

    private Camera mainCam;
    private Animator anim;

    [SerializeField] private bool IsUserShoot;//true가 되면 유저가 스스로 발사, false 자동으로 발사 되는 시스템

    [SerializeField] private GameObject objBullet;
    [SerializeField] private Transform trsShootPoint;
    [SerializeField] private Transform trsObjectDynamic;

    [Header("불렛")]
    [SerializeField][Tooltip("총알의 공격력입니다.")] private float bulletDamege;
    [SerializeField, Tooltip("총알의 이동속도입니다.")] private float bulletSpeed;
    private float timer = 0.0f;//시간
    [SerializeField] private float m_shootTimer = 0.5f;

    [Header("HP연출")]
    [SerializeField] private PlayerHP playerHP;
    [SerializeField] private GameObject objExplosion;
    private SpriteRenderer Sr;

    [Header("공격 레벨")]
    [SerializeField, Range(1, 5)] private int playerAttackLevel = 1;
    private int playerAttackMaxLevel = 5;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Enemy sc = collision.gameObject.GetComponent<Enemy>();
            if(sc.GetEnemyType() != Enemy.EnemyType.EnemyBoss)
            {
                sc.ExplosionEnemy();
            }

            Hit();
        }
        else if (collision.gameObject.tag == "Item")
        {
            Item sc = collision.gameObject.GetComponent<Item>();
            Item.ItemType type = sc.GetItemType();

            switch (type)
            {
                case Item.ItemType.PowerUp:
                    if(playerAttackLevel + 1 <= playerAttackMaxLevel)
                    {
                        playerAttackLevel++;
                    }
                    break;
                case Item.ItemType.HpRecovery:
                    if (m_curHp + 1 <= m_maxHp)
                    {
                        m_curHp++;
                    }
                    playerHP.SetPlayerHp(m_curHp, m_maxHp);
                    break;
            }

            Destroy(collision.gameObject);
        }
    }

    private void Start()
    {
        mainCam = Camera.main;//싱글톤, 메모리에 저장하고 사용
        anim = GetComponent<Animator>();

        m_curHp = m_maxHp;
        playerHP.SetPlayerHp(m_curHp, m_maxHp);

        Sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        moving();
        checkMovePosition();
        doAnimation();
        shoot();
    }

    private void moving()
    {
        float vertical = Input.GetAxisRaw("Vertical") * Time.deltaTime;
        float horizontal = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        transform.position += new Vector3(horizontal, vertical) * m_Speed;
    }

    private void checkMovePosition()
    {
        //월드포지션이 카메라의 뷰 포트 포인트로 변경되어 저장
        Vector3 currentPos = mainCam.WorldToViewportPoint(transform.position);

        if (currentPos.x < 0.05f)//왼쪽으로 너무 나감
        {
            currentPos.x = 0.05f;//0으로 보정
        }
        else if (currentPos.x > 0.95f)//오른쪽으로 너무 나감
        {
            currentPos.x = 0.95f;//1로 보정 
        }

        if (currentPos.y < 0.05f)
        {
            currentPos.y = 0.05f;
        }
        else if (currentPos.y > 0.95f)
        {
            currentPos.y = 0.95f;
        }

        //뷰포트 데이터를 월드 포지션으로 변경하여 저장
        transform.position = mainCam.ViewportToWorldPoint(currentPos);
    }

    private void doAnimation()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        anim.SetInteger("horizontal", (int)horizontal);
    }

    private void shoot()
    {
        if (IsUserShoot == true && Input.GetKeyDown(KeyCode.Space))
        {
            shootBullet();
        }
        else if (IsUserShoot == false)
        {
            timer += Time.deltaTime;
            if (timer >= m_shootTimer)
            {
                timer = 0.0f;
                shootBullet();
            }
        }
    }
    private void shootBullet()
    {
        switch (playerAttackLevel)
        {
            case 1:
                createBullet(trsShootPoint.position, Vector3.zero);//1자 총알
                break;
            case 2:
                createBullet(trsShootPoint.position + new Vector3(-0.2f, 0f, 0f)//1자 총알1
                    , Vector3.zero);
                createBullet(trsShootPoint.position + new Vector3(0.2f, 0f, 0f)//1자 총알2
                    , Vector3.zero);
                break;
            case 3:
                createBullet(trsShootPoint.position, Vector3.zero);
                createBullet(trsShootPoint.position + new Vector3(-0.2f, 0f, 0f)//1자 총알1
                    , new Vector3(0, 0, 15f));
                createBullet(trsShootPoint.position + new Vector3(0.2f, 0f, 0f)//1자 총알2
                    , new Vector3(0, 0, -15f));
                break;
            case 4:
                createBullet(trsShootPoint.position + new Vector3(-0.2f, 0f, 0f)//1자 총알1
                    , Vector3.zero);
                createBullet(trsShootPoint.position + new Vector3(0.2f, 0f, 0f)//1자 총알2
                    , Vector3.zero);
                createBullet(trsShootPoint.position + new Vector3(-0.2f, 0f, 0f)//1자 총알1
                    , new Vector3(0, 0, 15f));
                createBullet(trsShootPoint.position + new Vector3(0.2f, 0f, 0f)//1자 총알2
                    , new Vector3(0, 0, -15f));
                break;
            case 5:
                createBullet(trsShootPoint.position, Vector3.zero);
                createBullet(trsShootPoint.position + new Vector3(-0.2f, 0f, 0f)//1자 총알1
                    , new Vector3(0, 0, 15f));
                createBullet(trsShootPoint.position + new Vector3(0.2f, 0f, 0f)//1자 총알2
                    , new Vector3(0, 0, -15f));
                createBullet(trsShootPoint.position + new Vector3(-0.2f, 0f, 0f)//1자 총알1
                    , new Vector3(0, 0, 30f));
                createBullet(trsShootPoint.position + new Vector3(0.2f, 0f, 0f)//1자 총알2
                    , new Vector3(0, 0, -30f));
                break;
        }

    }

    private void createBullet(Vector3 _pos, Vector3 _rot)
    {
        GameObject obj = Instantiate(objBullet, _pos, Quaternion.Euler(_rot), trsObjectDynamic);
        Bullet bullet = obj.GetComponent<Bullet>();
        bullet.SetBullet(bulletSpeed, bulletDamege, true);
    }

    public void Hit()
    {
        m_curHp--;
        playerHP.SetPlayerHp(m_curHp, m_maxHp);

        if (playerAttackLevel - 1 > 0)
        {
            playerAttackLevel--;
        }

        if (m_curHp <= 0)
        {
            GameManager.Instance.GameOver();

            Destroy(gameObject);

            GameObject obj = Instantiate(objExplosion, transform.position,
                Quaternion.identity, transform.parent);

            Explosion Esc = obj.GetComponent<Explosion>();
            Esc.SetAnimationSize(Sr.sprite.rect.width);
        }
    }

}
