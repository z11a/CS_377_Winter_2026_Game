using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FPSManager : MonoBehaviour
{
    public static FPSManager instance;
    public int targetFPS = 60;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed extra FPSManager");
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.targetFrameRate != targetFPS)
            Application.targetFrameRate = targetFPS;
    }
}
