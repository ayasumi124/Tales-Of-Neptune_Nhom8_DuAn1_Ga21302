using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    public GameObject playerPrefab;

    private GameObject player;

    public Transform Player => player != null ? player.transform : null;

    public Transform PickupPoint
    {
        get
        {
            if (player == null)
                return null;

            return player.transform.Find("PickupPoint");
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindOrSpawnPlayer();
    }

    void FindOrSpawnPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public Health PlayerHealth
    {
        get
        {
            if (Player == null)
                return null;

            return Player.GetComponent<Health>();
        }
    }

    public void SetPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }
}