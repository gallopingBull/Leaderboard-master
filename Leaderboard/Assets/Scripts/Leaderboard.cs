using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;	
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
	
	private static Leaderboard instance;
	public Dictionary<string, int> localLeaderboards = new Dictionary<string, int>();
	private Dictionary<string, int> onlineLeaderboard = new Dictionary<string, int>();

	private string name = "";


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

		//localLeaderboards.Add("fucj", 211);
		//localLeaderboards.Add("ass", 18);
		//localLeaderboards.Add("bdsfds", 50);
		//localLeaderboards.Add("cdcda", 78);
		//localLeaderboards.Add("zsdfs", 45);
		InitLocalLeadboard();
	}


	private void InitLocalLeadboard()
	{
		//read in player score database
		//(sort if you have too)
		//declare and initialize new list of player sc ore

		//copy player score data into list


		GameObject tmpEntry;
		for (int i = 0; i < LeaderboardListSizeMAX; i++)
        {
			tmpEntry = Instantiate(player_Score_UITemplate_Prefab, transform.position, transform.rotation,
			transform);
			playerScore_entries_UI.Add(tmpEntry); //is this necessary?
		}


		if (localLeaderboards.Count > 0)
		{
			// tmp list to sort elements in dictionary by value (aka: scpre)
			List<KeyValuePair<string, int>> tmplist = localLeaderboards.ToList();

			//sort by highest to lowest
			var val = from element in localLeaderboards
					  orderby element.Value descending
					  select element;

			int i = 0;
			foreach (KeyValuePair<string, int> pair in val)
            {
				if (i == localLeaderboards.Count)
					return;
				playerScore_entries_UI[i].GetComponent<PlayerRankData>().SetTextFields(pair.Key, pair.Value, i + 1);
				i++;
			}
		}
		print("init over");
	}


	private void SetText()
	{

	}

	// create leaderboard rank UI panels and assign correct
	// data to TMP text fields
	private void InitOnlineLeadboard()
	{


	}


	public void AddToLeaderboard()
	{
		//destroy panels if new score is added to leaderboard
        if (playerScore_entries_UI.Count > 0)
        {
            foreach (GameObject item in playerScore_entries_UI)
				Destroy(item.gameObject);
		}

		string _name = "";
		string _score = "";
		
		int score = 0;


		_name = playerNameField.text;
		_score = playerScoreField.GetComponent<TMP_InputField>().text;
		score = int.Parse(_score);


		if (_name == "" ||
			score == 0)
			return;

		
		localLeaderboards.Add(_name, score);

		// tmp list to sort elements in dictionary by value (aka: scpre)
		List<KeyValuePair<string, int>> tmplist = localLeaderboards.ToList();
	
		//sort by highest to lowest
		var val = from element in localLeaderboards
                  orderby element.Value descending
                  select element;

		tmplist = val.ToList();

		//rearrange entires
		int i = 0;
		GameObject tmpEntry;

		foreach (KeyValuePair<string, int> pair in val)
        {
			tmpEntry = Instantiate(player_Score_UITemplate_Prefab, transform.position, transform.rotation,
			transform);

			// set text fields
			tmpEntry.GetComponent<PlayerRankData>().SetTextFields(pair.Key, pair.Value, i + 1);
			playerScore_entries_UI.Add(tmpEntry); //is this necessary?
			i++;
		}
	}
}

