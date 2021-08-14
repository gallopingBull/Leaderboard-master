using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveSystem : MonoBehaviour
{
    [SerializeField]
    private SavaData saveData;
    
    // Start is called before the first frame update
    void Start()
    {
        saveData = new SavaData();
    }
    
    public void LoadData()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            saveData = JsonUtility.FromJson<SavaData>(json);
            //print("loaded data");
            Leaderboard.Instance.SetLeaderboardData(saveData.keys, saveData.values);
        }
    }

    public void SaveData()
    {
        saveData.keys.Clear();
        saveData.values.Clear();

        for (int i = 0; i< Leaderboard.Instance.GetLocalLeaderboard().Count; i++)
        {
            saveData.keys.Add(Leaderboard.Instance.GetLocalLeaderboard()[i].Key);
            saveData.values.Add (Leaderboard.Instance.GetLocalLeaderboard()[i].Value);
        }

        var json =  JsonUtility.ToJson(saveData);

        Debug.Log(json);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();        
    }
}



/*
public interface ISaveSystem
{
    void LoadGame();
    void SaveGame();

}

*/