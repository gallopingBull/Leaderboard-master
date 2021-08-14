using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;	
using UnityEngine;
[Serializable]
public class Leaderboard : MonoBehaviour
{
	[SerializeField]
	private static Leaderboard instance;

	private enum LeaderboardStates
    {
		init, local, online
    }

	[SerializeField]
	private Dictionary<string, int> leaderboard = new Dictionary<string, int>();
	[SerializeField]
	public Dictionary<string, int> localLeaderboard = new Dictionary<string, int>();
	[SerializeField]
	private Dictionary<string, int> onlineLeaderboard = new Dictionary<string, int>();

	private bool init = false; 

	public TextMeshProUGUI playerNameField;
	public GameObject playerScoreField;


	[SerializeField]
	private GameObject player_Score_UITemplate_Prefab; //ref to instantiate
	private List<GameObject> playerScore_entries_UI;

	[SerializeField]
	private int LeaderboardListSizeMAX = 10;

    public static Leaderboard Instance { get => instance;}

    private void Awake()
    {
		//if (instance == null)
			instance = this;

		print(instance.gameObject.name);
    }

    // Start is called before the first frame update
    void Start()
	{
		//player_Score_UITemplate_Prefab = GameObject.Find("Panel_Player_Score_Row_Template");
		playerScore_entries_UI = new List<GameObject>();
		playerNameField = GameObject.Find("Text_PlayerName").GetComponent<TextMeshProUGUI>();
		playerScoreField = GameObject.Find("InputField_PlayerScore");

		// if no leaderboard has been created
		// creeate new leaderboard with blank values
		if (!init)
			CreateLeaderboard();

		//set data in leaderboard fields
		InitLocalLeadboard();
	}


	private void InitLocalLeadboard()
	{
		// copy player score data into list
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
				SetRankDataToTextField(pair.Key, pair.Value, i, playerScore_entries_UI[i]); // do not like the way im sending
																							// the object reference here
				i++;
			}
		}
		
		//init = true;
	}
	
	// creeate new blank leaderboard with default rank data
	private void CreateLeaderboard()
    {
		GameObject tmpEntry;
		for (int i = 0; i < LeaderboardListSizeMAX; i++)
		{
			tmpEntry = Instantiate(player_Score_UITemplate_Prefab, transform.position, transform.rotation,
			transform);
			playerScore_entries_UI.Add(tmpEntry); // this list will be read in by UI and displayed on screenS
		}
	}

	public void SetLeaderboardData(List<string> keys, List<int> values)
	{

		GameObject tmpEntry;
		for (int i = 0; i < keys.Count; i++)
		{
			if (localLeaderboard.ContainsKey(keys[i])
				&& localLeaderboard.ContainsValue(values[i]))
            {
				continue; 
            }
			localLeaderboard.Add(keys[i], values[i]);
			tmpEntry = playerScore_entries_UI[i];
			SetRankDataToTextField(keys[i], values[i], i, tmpEntry);
			playerScore_entries_UI.Add(tmpEntry); 
		}
	}
	public List<KeyValuePair<string, int>> GetLocalLeaderboard()
	{
		return localLeaderboard.ToList();
	}

	private void SetRankDataToTextField(string key, int value, int index, GameObject rank_UI_Panel)
	{
		rank_UI_Panel.GetComponent<PlayerRankData>().SetTextFields(key, value, index + 1);
	}

	// create leaderboard rank UI panels and assign correct
	// data to TMP text fields
	private void InitOnlineLeadboard()
	{
	}

	public void Add_RankData_To_Leaderboard()
	{
		//destroy previous yu player rank panels if new score is added to leaderboard
        if (playerScore_entries_UI.Count > 0)
        {
            foreach (GameObject item in playerScore_entries_UI)
				Destroy(item.gameObject);
		}

		CreateLeaderboard();
        #region input field stuff
        string _name = "";
		string _score = "";
		
		int score = 0;


		_name = playerNameField.text;
		_score = playerScoreField.GetComponent<TMP_InputField>().text;
		score = int.Parse(_score);
		PlayfabManager.instance.SubmitNameButton(_name);


		if (_name == "" ||
			score == 0)
			return;


        #endregion

        localLeaderboard.Add(_name, score);

		// tmp list to sort elements in dictionary by value (aka: scpre)
		List<KeyValuePair<string, int>> reorderedList = 
			ReorderPlayerRank_HigestToLowest(localLeaderboard.ToList());
	
		//rearrange entires
		int i = 0;
		GameObject tmpEntry;

		foreach (KeyValuePair<string, int> pair in reorderedList)
        {
			tmpEntry = Instantiate(player_Score_UITemplate_Prefab, transform.position, transform.rotation,
			transform);

			// set text fields 
			SetRankDataToTextField(pair.Key, pair.Value, i, tmpEntry);
			playerScore_entries_UI.Add(tmpEntry); //is this necessary?
			i++;
		}
	}

	public void SendLeaderboardToPlayfab()
	{
		for (int i = 0; i < GetLocalLeaderboard().Count; i++)
		{
			PlayfabManager.instance.SendLeaderboard(GetLocalLeaderboard()[i].Value);
		}
	}

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
}

