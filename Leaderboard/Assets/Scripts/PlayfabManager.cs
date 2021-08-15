using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
public class PlayfabManager : MonoBehaviour
{ 
    // Start is called before the first frame update
    [HideInInspector]
    public static PlayfabManager instance;

    void Start()
    {
        instance = this;
        Login();
    }
    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("successful login/account create");
        string name = null;
        if(result.InfoResultPayload.PlayerProfile != null)
            name = result.InfoResultPayload.PlayerProfile.DisplayName;
    }

    public void SubmitNameButton(string name) {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
    }

    private void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("updated display name!");
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("error whole logging in/creating account!");
        Debug.Log(error.GenerateErrorReport());
    }
   
    public void SendLeaderboard(string _name, int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "score",
                    Value = score
                }
            }
        };
        SubmitNameButton(_name);
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }
    public void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("successful leaderboard sent");
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "score",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        List<string> tmpKeys = new List<string>();
        List<int> tmpValues = new List<int>();

        foreach (var item in result.Leaderboard)
        {
            tmpKeys.Add(item.DisplayName);
            tmpValues.Add(item.StatValue);

            Debug.Log(string.Format ("RANK: {0} | NAME: {1} | SCORE: {2}", item.Position, item.DisplayName, item.StatValue));
        }

        Leaderboard.Instance.SetLeaderboardData(tmpKeys, tmpValues);
    }
}
