using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Checker 
{
    public static bool isLoadedToStartScene = false;
}


public class SceneChecker : MonoBehaviour
{
    public bool startScene = false;

    private void Awake()
    {
        if(startScene == true)
        {
            Checker.isLoadedToStartScene = true;
        }
        
        if(Checker.isLoadedToStartScene == false)
        {
            SceneManager.LoadScene((int)SceneIndex.mainScene);
        }
    }
}
