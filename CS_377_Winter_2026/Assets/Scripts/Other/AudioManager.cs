using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [HideInInspector] public AudioSource audioSource;
    public AudioClip dmgSFX;
    public AudioClip nomSFX;
    public AudioClip depositSFX;
    public AudioClip winRoundSFX;

    public enum SFXType
    {
        Damage,
        Nom,
        Deposit,
        WinRound
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed extra AudioManager");
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySFX(SFXType sfx)
    {
        if (audioSource != null) {
            switch (sfx)
            {
                case SFXType.Damage:
                    audioSource.PlayOneShot(dmgSFX);
                    break;
                case SFXType.Nom:
                    audioSource.PlayOneShot(nomSFX);
                    break;
                case SFXType.Deposit:
                    audioSource.PlayOneShot(depositSFX);
                    break;
                case SFXType.WinRound:
                    audioSource.PlayOneShot(winRoundSFX);
                    break;
            }
        }
    }

    public void PlayRandomSFX(AudioClip[] clips)
    {
        int random = Random.Range(0, clips.Length);

        audioSource.PlayOneShot(clips[random]);
    }
}
