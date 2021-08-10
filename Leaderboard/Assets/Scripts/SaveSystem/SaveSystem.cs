using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveSystem : MonoBehaviour
{
    private SaveData saveData;// = new SaveData();
    // Start is called before the first frame update
    void Start()
    {
        saveData = new SaveData();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SaveData();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadData();
        }
    }

    private void LoadData()
    {

        if (PlayerPrefs.HasKey("SaveData"))
        {
            string json = PlayerPrefs.GetString("SaveData");
            saveData = JsonUtility.FromJson<SaveData>(json);
            print("loaded datab");
            Leaderboard.Instance.localLeaderboards = saveData.localLeaderBoardData;
        }
    }

    private void SaveData()
    {
        saveData.localLeaderBoardData.Clear();
        saveData.localLeaderBoardData = Leaderboard.Instance.localLeaderboards;


        var json = JsonUtility.ToJson(saveData);


        Debug.Log(json);
        PlayerPrefs.SetString("SaveData", json);
        PlayerPrefs.Save();

        //something here to store data in registry

    }
}

[System.Serializable]
class SaveData
{
    public Dictionary<string, int> localLeaderBoardData = new Dictionary<string, int>();
}

public interface ISaveSystem
{
    void LoadGame();
    void SaveGame();

}
