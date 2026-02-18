using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameUIManager : MonoBehaviour
{

    public PlayerHandler player1;
    public PlayerHandler player2;
    
    public Slider player1HealthBar;
    public Slider player2HealthBar;

    public TextMeshProUGUI player1Points;
    public TextMeshProUGUI player2Points;

    void Update()
    {
        if (player1 != null)
        {
            player1HealthBar.maxValue = 50f;
            player1HealthBar.value = player1.playerHealth;
            player1Points.text = "Points: " + player1.playerCurrentRoundScore.ToString();
        }

        if (player2 != null)
        {
            player2HealthBar.maxValue = 50f;
            player2HealthBar.value = player2.playerHealth;
            player2Points.text = "Points: " + player2.playerCurrentRoundScore.ToString();
        }
    }
}
