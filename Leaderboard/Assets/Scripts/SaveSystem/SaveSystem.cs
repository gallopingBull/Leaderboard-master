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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SaveData();

        if (Input.GetKeyDown(KeyCode.L))
            LoadData();
    }
    private void LoadData()
    {
        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            saveData = JsonUtility.FromJson<SavaData>(json);
            print("loaded datab");
            Leaderboard.Instance.SetLocalLeaderboard(saveData.keys, saveData.values);
        }
    }

    private void SaveData()
    {
        //saveData.localLeaderBoardData.Clear();
        saveData.keys.Clear();
        saveData.values.Clear();


        for (int i = 0; i< Leaderboard.Instance.GetLocalLeaderboard().Count; i++)
        {
            saveData.keys.Add(Leaderboard.Instance.GetLocalLeaderboard()[i].Key);
            saveData.values.Add (Leaderboard.Instance.GetLocalLeaderboard()[i].Value);
        }
        //saveData.localLeaderBoardData = Leaderboard.Instance.GetLocalLeaderboard();

        var json =  JsonUtility.ToJson(saveData);


        Debug.Log(json);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();

        //something here to store data in registry

    }
}



/*
public interface ISaveSystem
{
    void LoadGame();
    void SaveGame();

}

*/