using UnityEngine;

public class GameManager : MonoBehaviour, ILeaderboard
{
    public static GameManager instance; 
    public int score = 100; 
    
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
}
