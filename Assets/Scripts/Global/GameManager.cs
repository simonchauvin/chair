using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    private Player player;

    private bool ready;


    void Start()
    {
        Init();
    }

    public void Init()
    {
        player = FindObjectOfType<Player>();
        FindObjectOfType<Grid>().Init();

        player.Init();

        ready = true;
    }

    void Update()
    {
        
    }

    public void Restart()
    {
        Debug.Log("Restart");
        ready = false;

        Init();
    }

    public bool IsReady()
    {
        return ready;
    }
}
