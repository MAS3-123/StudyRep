using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;
    private float damege = 1.0f;
    private bool enemyBullet;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyBullet == true && collision.gameObject.tag == "Player")
        {
            Destroy(gameObject);
            Player sc = collision.GetComponent<Player>();
            sc.Hit();
        }
        else if (enemyBullet == false && collision.gameObject.tag == "Enemy")//플레이어가 쏜총알, 콜루전이 적기일때
        {
            Destroy(gameObject);
            Enemy sc = collision.gameObject.GetComponent<Enemy>();
            sc.Hit(damege);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    void Update()
    {
        transform.position += transform.up * Time.deltaTime * speed;
    }

    public void SetBullet(float _speed, float _damege, bool _isPlayer = false)
    {
        if(_isPlayer == true)
        {
            enemyBullet = false;
        }
        else
        {
            enemyBullet = true;
        }

        speed = _speed;
        damege = _damege;
    }
}
