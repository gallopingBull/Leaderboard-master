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

	[SerializeField]
	private Dictionary<string, int> leaderboard = new Dictionary<string, int>();
	[HideInInspector]
	public Dictionary<string, int> localLeaderboard = new Dictionary<string, int>();
	[SerializeField]
	private Dictionary<string, int> onlineLeaderboard = new Dictionary<string, int>();

	private bool init = false;

	public TextMeshProUGUI playerNameField;
	public GameObject playerScoreField;
	private Button submitEntry_Button;
	
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
		scoreEntryParent = GameObject.Find("Panel_List").transform;
		playerScore_entries_UI = new List<GameObject>();
		playerNameField = GameObject.Find("Text_PlayerName").GetComponent<TextMeshProUGUI>();
		playerScoreField = GameObject.Find("InputField_PlayerScore");
		//submitEntry_Button = GameObject.Find("").GetComponent<Button>();

		submitEntryPrompt = GameObject.Find("Button_SubmitEntry");
		
		dreamLoManager = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
        // if no leaderboard has been created
        // creeate new leaderboard with blank values
        if (!init)
			CreateNewLeaderboard();

		// set data in leaderboard fields
		InitLeadboards();
	}

	private void InitLeadboards()
	{
		// ** **\\
		// localLeaderboard =  SaveSystem.instance.LoadData();
		// onlineLeaderBoard = Get Leaderboard from DreamLo
		// copy player score data into list
		// ** **\\

		//every thing here seems redundant and i dont think it activates
		if (localLeaderboard.Count > 0)
		{
			// tmp list to sort elements in dictionary by value (aka: scpre)
			List<KeyValuePair<string, int>> reorderedList =
				ReorderPlayerRank_HigestToLowest(localLeaderboard.ToList());

			int i = 0;
			foreach (KeyValuePair<string, int> pair in reorderedList)
			{
				if (i == reorderedList.Count)
					return;
				SetEntryDataToTextField(pair.Key, pair.Value, i, playerScore_entries_UI[i]); // do not like the way im sending
																							// the object reference here
				i++;
			}
		}
		//init = true;
	}

	//this is an outside caller
	public void SetLeaderboardData(List<string> keys, List<int> values)
	{
		leaderboard.Clear();
		ResetLeaderboard();	

		GameObject tmpEntry;

		for (int i = 0; i < keys.Count; i++)
		{
			if (leaderboard.ContainsKey(keys[i]))
				continue;

			leaderboard.Add(keys[i], values[i]);
			tmpEntry = playerScore_entries_UI[i];
			SetEntryDataToTextField(keys[i], values[i], i, tmpEntry);
			playerScore_entries_UI.Add(tmpEntry);
		}


		List<KeyValuePair<string, int>> reorderedList =
			ReorderPlayerRank_HigestToLowest(leaderboard.ToList());
		int f = 0;
		foreach (KeyValuePair<string, int> pair in reorderedList)
		{
			tmpEntry = playerScore_entries_UI[f];
			// set text fields 
			SetEntryDataToTextField(pair.Key, pair.Value, f, tmpEntry);
			f++;
		}
	}


	public List<KeyValuePair<string, int>> GetLeaderboardList()
	{
		return leaderboard.ToList();
	}
	public void SetLocalLeaderboard(List<KeyValuePair<string, int>> list)
	{
		localLeaderboard.Clear();

		foreach (var item in list)
			localLeaderboard.Add(item.Key, item.Value);
	}

	public void AddNewEntryToLeaderboard()
	{
		// destroy previous rank entries lif new score is added to leaderboard
		if (playerScore_entries_UI.Count > 0)
			DestroyLeaderboard();

		CreateNewLeaderboard();
				
		KeyValuePair<string, int> tmp = GetTextField();
	
		if (!IsEntryValid(tmp.Key, tmp.Value))
			return;

		leaderboard.Add(tmp.Key, tmp.Value);

		// tmp list to sort elements in dictionary by value (aka: score)
		List<KeyValuePair<string, int>> reorderedList =
			ReorderPlayerRank_HigestToLowest(leaderboard.ToList());

		// reorder entires
		int i = 0;
		GameObject tmpEntry;

		foreach (KeyValuePair<string, int> pair in reorderedList)
		{
			if (i >= LeaderboardListSizeMAX) {
				for (int k = reorderedList.Count - 1; k >= LeaderboardListSizeMAX; k--)
					reorderedList.Remove(reorderedList[k]);

				SetLocalLeaderboard(reorderedList);
				return;
			}

			tmpEntry = playerScore_entries_UI[i];
			SetEntryDataToTextField(pair.Key, pair.Value, i, tmpEntry);
			i++;
		}

        if (currentState == LeaderboardStates.local)
			SaveSystem.instance.SaveData();

		// ** **\\
		// check online leaderboard and if new score entry is higher 
		// than any of the top ten, add new entry in there as well
		//SendLeaderboardToDreamLo();
		// ** **\\
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
		if (leaderboard.ContainsKey(_name) &&
            leaderboard.ContainsValue(_score))
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
	
	// creeate new blank leaderboard with default rank data
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
				currentState = LeaderboardStates.entry;
                break;
            case LeaderboardStates.local:
				currentState = LeaderboardStates.local;
				SaveSystem.instance.LoadData();
				//leaderboard = localLeaderboard;
                break;

            case LeaderboardStates.online:
				currentState = LeaderboardStates.online;
				GetLeaderboardDataFromDreamLo();
				leaderboard = onlineLeaderboard;
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


	// triggered by UI button
	public void SendLeaderboardToDreamLo()
	{
		for (int i = 0; i < GetLeaderboardList().Count; i++)
			dreamLoManager.AddScore(GetLeaderboardList()[i].Key, GetLeaderboardList()[i].Value);
	}

	public void GetLeaderboardDataFromDreamLo()
	{
		List<dreamloLeaderBoard.Score> scoreList = dreamLoManager.ToListHighToLow();
		if (scoreList == null)
			print("(loading...)");
		else
		{
			List<string> tmpKeys = new List<string>();
			List<int> tmpValues = new List<int>();
			foreach (var item in scoreList)
			{
				tmpKeys.Add(item.playerName);
				tmpValues.Add(item.score);
			}
			SetLeaderboardData(tmpKeys, tmpValues);
		}
	}


	#region sorting functions
	// sort leaderboard by highest to lowest
	private List<KeyValuePair<string, int>> ReorderPlayerRank_HigestToLowest(List<KeyValuePair<string, int>> data)
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
