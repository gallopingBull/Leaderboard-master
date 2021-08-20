using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Leaderboard : MonoBehaviour
{
	[SerializeField]
	private static Leaderboard instance;

	[SerializeField]	
	private bool EnableTesting = false; 
	public enum LeaderboardStates
	{
		entry, local, online
	}

	private LeaderboardStates currentState;

	private bool isOnline = true;

	[SerializeField]
	private Dictionary<string, int> _leaderboard = new Dictionary<string, int>();
	[HideInInspector]
	public Dictionary<string, int> _localLeaderboard = new Dictionary<string, int>();
	[SerializeField]
	private Dictionary<string, int> _onlineLeaderboard = new Dictionary<string, int>();

	private bool init = false;

	private TextMeshProUGUI headerTitle; 
	public TextMeshProUGUI playerNameField;
	public GameObject playerScoreField;
	private Button submitEntry_Button;


	private TextMeshProUGUI PlayerScore_Text;
	private GameObject submitEntryPrompt;
	private Transform scoreEntryParent;
	
	[SerializeField]
	private GameObject player_Score_UITemplate_Prefab; //ref to instantiate
	private List<GameObject> playerScore_entries_UI;

	[SerializeField]
	private int LeaderboardListSizeMAX = 10;

	private dreamloLeaderBoard dreamLoManager;

	public static Leaderboard Instance { get => instance; }

	private void Awake()
	{
		if (instance == null)
			instance = this;
	}

	private void Start()
	{
		// set data in leaderboard fields
		Invoke("InitLeaderboard", 1f);
	}

	private void InitLeaderboard()
	{
        if (!init)
        {
			scoreEntryParent = GameObject.Find("Panel_List").transform;
			playerScore_entries_UI = new List<GameObject>();
			playerNameField = GameObject.Find("Text_PlayerName").GetComponent<TextMeshProUGUI>();
			
			
			if (EnableTesting)
				playerScoreField = GameObject.Find("InputField_PlayerScore");


			headerTitle = GameObject.Find("Text_HeaderTitle").GetComponent<TextMeshProUGUI>();
			PlayerScore_Text = GameObject.Find("Text_NewScore").GetComponent<TextMeshProUGUI>();
			
			submitEntryPrompt = GameObject.Find("Panel_InputPlayerName");
			submitEntryPrompt.SetActive(false);


			dreamLoManager = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
			init = true;
		}

		CreateNewLeaderboard();

		// start with local leaderboard by default
		EnterState(LeaderboardStates.local);
	}

	//this is an outside caller
	public void SetLeaderboardData(List<string> keys, List<int> values)
	{
		_leaderboard.Clear();
		ResetLeaderboard();	

		GameObject tmpEntry;

		for (int i = 0; i < keys.Count; i++)
		{
			if (_leaderboard.ContainsKey(keys[i]))
				continue;

			_leaderboard.Add(keys[i], values[i]);
			tmpEntry = playerScore_entries_UI[i];
			SetEntryDataToTextField(keys[i], values[i], i, tmpEntry);
			playerScore_entries_UI.Add(tmpEntry);
		}
		SortLeaderboard(_leaderboard);
	}
	public void SetLocalLeaderboard(List<KeyValuePair<string, int>> list)
	{
		_leaderboard.Clear();

		foreach (var item in list)
			_leaderboard.Add(item.Key, item.Value);
	}

	public List<KeyValuePair<string, int>> GetLeaderboardList()
	{
		return _leaderboard.ToList();
	}
	
	public void AddNewEntryToLeaderboard()
	{
		KeyValuePair<string, int> tmp = GetTextField();

		if (!IsEntryValid(tmp.Key, tmp.Value))
			return;

		ResetLeaderboard();
				
		_leaderboard.Add(tmp.Key, tmp.Value);

		SortLeaderboard(_leaderboard);


        if (currentState == LeaderboardStates.local ||
			currentState == LeaderboardStates.entry)
			SaveSystem.instance.SaveData();


        // ** **\\
        // check online leaderboard and if new score entry is higher 
        // than any of the top ten, add new entry in there as well
        //SendLeaderboardToDreamLo();
        // ** **\\

        if (CheckOnlineConnection() && (currentState == LeaderboardStates.local || currentState == LeaderboardStates.entry)) {
			EnterState(LeaderboardStates.online);
			AddNewEntryToLeaderboard();
			return;
		}

		if (currentState == LeaderboardStates.online)
			SendLeaderboardDataToDreamLo();

		DisablePlayerNameEntryPrompt();

		
	}

	private void SetEntryDataToTextField(string key, int value, int index, GameObject rank_UI_Panel)
	{
		rank_UI_Panel.GetComponent<PlayerRankData>().SetTextFields(key, value, index + 1);
	}

	private KeyValuePair<string, int> GetTextField()
	{
		string _name = "";
		string _score = "";

		int score = 0;


		_name = playerNameField.text;

        if (EnableTesting)
        {
			_score = playerScoreField.GetComponent<TMP_InputField>().text;
			score = int.Parse(_score);
        }
        else
			score = GameManager.instance.SendScore();



		KeyValuePair<string, int> tmp = new KeyValuePair<string, int>(_name, score);
		return tmp;
	}

	// check if there's duplicate scores/names 
	private bool IsEntryValid(string _name, int _score)
    {
		if (_leaderboard.ContainsKey(_name) &&
            _leaderboard.ContainsValue(_score))
        {
			Debug.Log("Entry with similar name and score already exists");
			Debug.Log("Please change your name");
			return false;
		}
		if (_name == "" || _score == 0)
        {
			Debug.Log("nothing assigned in text field");
			return false;
		}
		return true; 
    }

	private void SortLeaderboard(Dictionary<string, int> leaderboard)
    {
		// tmp list to sort elements in dictionary by value (aka: score)
		List<KeyValuePair<string, int>> reorderedList =
			SortList_HigestToLowest(leaderboard.ToList());

		// reorder entires
		int i = 0;
		GameObject tmpEntry;

		foreach (KeyValuePair<string, int> pair in reorderedList)
		{
			// remove any score entries pass the leaderboard size limit
			if (i >= LeaderboardListSizeMAX)
			{
				for (int k = reorderedList.Count - 1; k >= LeaderboardListSizeMAX; k--)
					reorderedList.Remove(reorderedList[k]);

				SetLocalLeaderboard(reorderedList);
				break;
			}

			tmpEntry = playerScore_entries_UI[i];
			SetEntryDataToTextField(pair.Key, pair.Value, i, tmpEntry);
			i++;
		}
	}

	// create new blank leaderboard with default entry data
	private void CreateNewLeaderboard()
	{
		GameObject tmpEntry;
		for (int i = 0; i < LeaderboardListSizeMAX; i++)
		{
			tmpEntry = Instantiate(player_Score_UITemplate_Prefab, scoreEntryParent.position, scoreEntryParent.rotation,
			scoreEntryParent);
			playerScore_entries_UI.Add(tmpEntry); // this list will be read in by UI and displayed on screenS
		}
	}
	private void DestroyLeaderboard()
	{
		foreach (GameObject item in playerScore_entries_UI)
			Destroy(item.gameObject);
		playerScore_entries_UI.Clear();
	}
	public void ResetLeaderboard()
    {
		DestroyLeaderboard();
		CreateNewLeaderboard();
    }
	
	public void EnterState(LeaderboardStates _state)
    {
		ExitState(currentState);
        switch (_state)	
        {
            case LeaderboardStates.entry:
				submitEntryPrompt.SetActive(true);
				currentState = LeaderboardStates.entry;
				PlayerScore_Text.text = GameManager.instance.score.ToString(); 
                break;

            case LeaderboardStates.local:
				currentState = LeaderboardStates.local;
				headerTitle.text = "Local Leaderboard";
				_localLeaderboard = GetLocalLeaderboardData();
				_leaderboard = _localLeaderboard;
				SetLeaderboardData(_leaderboard.Keys.ToList(), _leaderboard.Values.ToList());


				break;

            case LeaderboardStates.online:
				currentState = LeaderboardStates.online;
				headerTitle.text = "Online Leaderboard";
				_onlineLeaderboard = GetLeaderboardDataFromDreamLo();
				_leaderboard = _onlineLeaderboard;
				SetLeaderboardData(_leaderboard.Keys.ToList(), _leaderboard.Values.ToList());
				break;

            default:
                break;
        }
    }

	private void ExitState(LeaderboardStates _state)
	{
		switch (_state)
		{
			case LeaderboardStates.entry:
				submitEntryPrompt.SetActive(false);
				break;
			case LeaderboardStates.local:
				break;
			case LeaderboardStates.online:
				break;
			default:
				break;
		}
	}

	[ContextMenu("Open Local Leaderboard")]
	private void OpenLocalLeaderboard()
	{
		EnterState(LeaderboardStates.local);
	}
	[ContextMenu("Open Online Leaderboard")]
	private void OpenOnlineLeaderboard()
	{
		EnterState(LeaderboardStates.online);
	}
	[ContextMenu("Open PlayerName Prompt")]
	private void EnablePlayerNameEntryPrompt()
	{
		EnterState(LeaderboardStates.entry);
	}
	private void DisablePlayerNameEntryPrompt()
	{
		ExitState(LeaderboardStates.entry);
	}

	private bool CheckOnlineConnection()
    {
		if (Application.internetReachability != NetworkReachability.NotReachable)
		{
			//Connection OK
			print("connection Ok");
			return true;
		}
		else
		{
			print("connection false");
			//NoInternetConnection
			return false;
		}

    }

	private Dictionary<string, int> GetLocalLeaderboardData()
    {
		return SaveSystem.instance.GetLocalLeaderboard();
	}


	// triggered by UI button
	public void SendLeaderboardDataToDreamLo()
	{
		for (int i = 0; i < GetLeaderboardList().Count; i++)
			dreamLoManager.AddScore(GetLeaderboardList()[i].Key, GetLeaderboardList()[i].Value);
	}

	public Dictionary<string, int> GetLeaderboardDataFromDreamLo()
	{
		List<dreamloLeaderBoard.Score> scoreList = dreamLoManager.ToListHighToLow();
		// might delete this condition
		// i'll have a pre condition to check if player is online and
		// whether they can access the online leaderboard
		if (scoreList == null)
			print("(loading...)");
		
		Dictionary<string, int> tmpDic = new Dictionary<string, int>();

		for (int i = 0; i < scoreList.Count; i++)
			tmpDic.Add(scoreList[i].playerName, scoreList[i].score);

		return tmpDic;
	}


	#region sorting functions
	// sort leaderboard by highest to lowest
	private List<KeyValuePair<string, int>> SortList_HigestToLowest(List<KeyValuePair<string, int>> data)
    {
		// tmp list to sort elements in dictionary by value (aka: scpre)
		List<KeyValuePair<string, int>> tmplist = data.ToList();
	
		var val = from element in data.ToList()
				  orderby element.Value descending
				  select element;
		
		return tmplist = val.ToList();
	}
    #endregion

}

//implemented in GameManager to send player score to leadeboard
public interface ILeaderboard{
	int SendScore();
}
