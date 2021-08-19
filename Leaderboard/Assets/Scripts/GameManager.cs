using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, ILeaderboard
{
    public static GameManager instance; 
    public int score = 10; 
    
    
    void Start()
    {
        if (instance == null)
            instance = this;
    }
    public int SendScore()
    {
        Debug.Log("sending score to leaderboard");
        return score;
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            SendScore();
    }
}
