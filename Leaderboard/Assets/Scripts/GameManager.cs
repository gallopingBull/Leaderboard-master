using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, ILeaderboard
{

    public int score = 10; 
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
