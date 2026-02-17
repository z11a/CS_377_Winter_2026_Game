using UnityEngine;

public class StatTracker
{
    public int roundsWon;
    public float totalDamageDealt;
    public float totalDamageTaken;
    public float roundDamageDealt;
    public float roundDamageTaken;
    public int timesAttack;
    public int timesHit;
    public int kills;
    public int deaths;
    public int cheeseCollected;
    public int currentCheese;
    // public int stolenCheese;

    public float PlayerAccuracy
    {
        get
        {
            if (timesAttack == 0) return 0;
            return (float)timesHit / timesAttack;
        }
    }

    public void ResetRoundStats()
    {
        roundDamageDealt = 0;
        roundDamageTaken = 0;
        timesAttack = 0;
        timesHit = 0;
    }
    

}
