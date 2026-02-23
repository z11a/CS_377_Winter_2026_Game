using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text cheesesText;

    private void Awake()
    {
        // Update text references
        healthText = transform.Find("Health")?.GetComponent<TMP_Text>();
        scoreText = transform.Find("Score")?.GetComponent<TMP_Text>();
        cheesesText = transform.Find("Cheese")?.GetComponent<TMP_Text>();

        if (healthText == null)
            Debug.LogError("Health text not found under " + gameObject.name);

        if (scoreText == null)
            Debug.LogError("Score text not found under " + gameObject.name);

        if (cheesesText == null)
            Debug.LogError("Cheese text not found under " + gameObject.name);
    }

    public void UpdateHealth(int health)
    {
        healthText.text = "Health: " + health;
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    public void UpdateCheeses(int amount)
    {
        cheesesText.text = "Cheese: " + amount;
    }
}