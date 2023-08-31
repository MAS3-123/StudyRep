using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RankData
{
    public int Score;
    public string Name;
}

public enum SceneIndex 
{
    mainScene,
    playScene,
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;//싱글톤

    [SerializeField] private List<GameObject> listEnemy;
    [SerializeField] private GameObject enemyBoss;

    private float spawnTimer = 0.0f;//타이머
    [SerializeField] private float spawnTime;//몬스터가 생산되는 시간

    private Transform spawnPoint;//적기 생성위치
    [SerializeField] private Transform trsDynamicObject;//동적으로 움직이는 오브젝트가 생성되는 공간

    [Header("아이템")]
    [SerializeField, Range(0.0f, 100.0f)] private float itemDropRate = 30.0f;
    [SerializeField] private List<GameObject> listItem;

    [Header("보스 출동 게이지")]
    [SerializeField] private Slider sliderTimer;
    [SerializeField] private TMP_Text textTimer;
    private float timer = 0.0f;//타이머
    [SerializeField] private float appearBossTime = 30.0f;//보스 출동 시간

    private bool appearBoss = false; // 보스가 출동 했는지

    [SerializeField] private Slider sliderDestroy;
    [SerializeField] private TMP_Text textDestroy;
    private int DestCount = 0;
    private int appearBossDestroy = 10;

    [Header("점수")]
    [SerializeField] private TMP_Text textScore;
    private int score = 0;

    private List<GameObject> listAppearEnemy = new List<GameObject>();

    private Transform trsPlayer;

    [Header("게임오버 인풋필드")]
    [SerializeField] private TMP_Text textGameoverRank;
    [SerializeField] private TMP_InputField ipfGameoverName;
    [SerializeField] private Button btnGameover;

    private List<RankData> listRank = new List<RankData>();
    private string keyRank = "keyRank";
    private int rankCount = 10;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        trsPlayer = FindObjectOfType<Player>().transform;
        ipfGameoverName.gameObject.SetActive(false);
        getRankDatas();

    }

    private void getRankDatas()//유니티에 저장된 랭크 데이터를 불러옵니다. >> listRank에
    {
        string value = PlayerPrefs.GetString(keyRank, string.Empty);//""
        if(value == string.Empty)//""
        {
            for(int iNum = 0; iNum < rankCount; iNum++)
            {
                RankData data = new RankData();
                data.Name = string.Empty;
                data.Score = 0;
                listRank.Add(data);
            }
            value = JsonConvert.SerializeObject(listRank);
            PlayerPrefs.SetString(keyRank, value);

        }
        else
        {
            listRank =  JsonConvert.DeserializeObject<List<RankData>>(value);
            if(listRank.Count != rankCount)
            {
                for (int iNum = 0; iNum < rankCount; iNum++)
                {
                    RankData data = new RankData();
                    data.Name = string.Empty;
                    data.Score = 0;
                    listRank.Add(data);
                }
                value = JsonConvert.SerializeObject(listRank);
                PlayerPrefs.SetString(keyRank, value);
                listRank = JsonConvert.DeserializeObject<List<RankData>>(value);
            }
        }
    }

    void Start()
    {
        //spawnPoint = transform.Find("SpawnPoint");
        spawnPoint = transform.GetChild(0);
    }

    void Update()
    {
        checkSpawn();
        checkTimer();
        //checkDestroyCount();
    }

    private void checkSpawn()
    {
        if(appearBoss == true)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnTime)
        {
            spawnTimer = 0.0f;
            spawnEnemy();
        }
    }

    private void checkTimer()
    {
        if(appearBoss == false)
        {
            if (timer < appearBossTime)
            {
                timer += Time.deltaTime;

                if (timer > appearBossTime)
                {
                    timer = appearBossTime;
                }

                sliderTimer.value = timer / appearBossTime;
                textTimer.text = $"{(int)timer} / {(int)appearBossTime}";
            }

            if (appearBoss == false && timer >= appearBossTime)
            {
                appearBoss = true;
                destroyAllEnemy();
                Instantiate(enemyBoss, spawnPoint.position, Quaternion.identity, trsDynamicObject);
            }
        }
        
    }

    public void CheckBossHP(float _currHp, float _maxHp)
    {
        if(appearBoss == false)
        {
            return;
        }

        sliderTimer.value = _currHp / _maxHp;
        textTimer.text = $"{(int)_currHp} / {(int)_maxHp}";
    }

    private void destroyAllEnemy()
    {
        int count = listAppearEnemy.Count;
        for(int iNum = count - 1; iNum > -1; iNum--)
        {
            Destroy(listAppearEnemy[iNum]);
        }
    }

    private void checkDestroyCount()
    {
        sliderDestroy.value = (float)DestCount / appearBossDestroy;
        textDestroy.text = $"{DestCount} / {appearBossDestroy}";
    }

    public void AddDestroyCount(Enemy.EnemyType _Type)
    {
        //if (DestCount < appearBossDestroy)
        //{ 
        //    DestCount++;
        //}
        switch (_Type)
        {
            case Enemy.EnemyType.EnemyA:
                score += 10;
                break;
            case Enemy.EnemyType.EnemyB:
                score += 30;
                break;
            case Enemy.EnemyType.EnemyC:
                score += 100;
                break;
            case Enemy.EnemyType.EnemyBoss:
                score += 200;
                break;
        }

        textScore.text = score.ToString("D8");
    }

    private void spawnEnemy()//적기 생성
    {
        int iRand = Random.Range(0, listEnemy.Count);//0 ~ 2;랜덤 넘버링
        GameObject objEnemy = listEnemy[iRand];//생성할 랜덤 에너미
        float fRand = Random.Range(-2.5f, 2.5f);//-2.0 ~ 2.0;랜덤 생성 좌우 포인트
        Vector3 spawnPostion = spawnPoint.position;
        spawnPostion.x += fRand;

        GameObject obj = Instantiate(objEnemy, spawnPostion, Quaternion.identity, trsDynamicObject);
        Enemy enemy = obj.GetComponent<Enemy>();

        float rate = Random.Range(0.0f, 100.0f);
        if (rate <= itemDropRate)
        {
            enemy.SetHaveItem();
        }

        listAppearEnemy.Add(obj);
    }

    public void RemoveEnemy(GameObject _obj)
    {
        listAppearEnemy.Remove(_obj);
    }

    public void CreatItem(Vector3 _pos)
    {
        int rand = Random.Range(0, listItem.Count);//0, 1;
        GameObject instObj = listItem[rand];
        Instantiate(instObj, _pos, Quaternion.Euler(Vector3.zero), trsDynamicObject);
    }

    public Transform GetPlayerTransform()
    {
        return trsPlayer;
    }

    public void GameOver()
    {
        //PlayerPrefs.SetInt
        int rank = getMyRank(score);// 몇등인지 체크하는 시스템
        if (rank == -1)
        {
            textGameoverRank.text = "당신은 등수에 들지 못했습니다.";
        }
        else
        {
            textGameoverRank.text = $"당신의 랭크는 {rank} 등 이었습니다.";
        }

        ipfGameoverName.gameObject.SetActive(true);

        //버튼을 코드에 등록하는 방법
        btnGameover.onClick.RemoveAllListeners();
        btnGameover.onClick.AddListener(() => { onClickGameoverFuntion(); });
        //btnGameover.onClick.AddListener(() => onClickGameoverFuntion());
        //btnGameover.onClick.AddListener(onClickGameoverFuntion);
    }

    private void onClickGameoverFuntion()
    {
        string ipfText = ipfGameoverName.text;
        if(ipfText == string.Empty || ipfText.Length < 3)
        {
            textGameoverRank.text = "이름에는 공백이나 , \n3글자 미만의 이름은 넣을 수 없습니다.";
            return;
        }
        int rank = getMyRank(score);

        if(rank != -1) //등수에 들지 못했다면
        {
            setMyRank(score, ipfText, rank);
            string value = JsonConvert.SerializeObject(listRank);
            PlayerPrefs.SetString(keyRank, value);
        }

        //다른 씬으로 이동
        SceneManager.LoadScene((int)SceneIndex.mainScene);
        //SceneManager.LoadScene("MainScene"); 다른 방법
    }

    private int getMyRank(int _score)
    {
        for(int iNum = 0; iNum < rankCount; ++iNum)
        {
            if (listRank[iNum].Score < _score)
            {
                return iNum + 1;
            }
            // 등수에 들 수 있었다면
        }
        return -1;// 등수에 들 수 없었다면
    }

    private void setMyRank(int _score, string _name, int _rank)
    {
        _rank = _rank - 1;

        listRank.Insert(_rank, new RankData() { Name = _name, Score = _score });
        listRank.RemoveAt(rankCount);
    }

    public void DestroyBoss()
    {
        //다시 적기가 출동하고 시간이 흐름
        timer = 0.0f;
        appearBoss = false;
    }
}
