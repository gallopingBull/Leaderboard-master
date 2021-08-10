using TMPro; 
using UnityEngine;

[System.Serializable] 
public class PlayerRankData : MonoBehaviour
{
    public TextMeshProUGUI playerName_Text;
    public TextMeshProUGUI playerScore_Text;
    public TextMeshProUGUI playerRank_Text;
            
    public TextMeshProUGUI PlayerNameField {set => playerName_Text = value; }

    // Start is called before the first frame update
    void Awake()
    {
        playerName_Text = transform.Find("Panel_Name").GetComponentInChildren<TextMeshProUGUI>(); 
        playerScore_Text = transform.Find("Panel_Score").GetComponentInChildren<TextMeshProUGUI>(); 
        playerRank_Text = transform.Find("Panel_Rank").GetComponentInChildren<TextMeshProUGUI>(); 
    }

    public void SetTextFields(string _playerName, int score, int rank)
    {
        playerName_Text.text = _playerName;
        playerScore_Text.text = score.ToString();
        playerRank_Text.text = rank.ToString();
    }

}
