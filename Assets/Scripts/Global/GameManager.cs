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

    private SoundController soundController;
    private Player player;
    private Camera mainCam;
    private SkinController skinController;

    private bool ready;


    void Start()
    {
        Init();
    }

    public void Init()
    {
        soundController = FindObjectOfType<SoundController>();
        player = FindObjectOfType<Player>();
        mainCam = FindObjectOfType<Camera>();
        skinController = FindObjectOfType<SkinController>();

        soundController.Init();
        skinController.Init(Screen.currentResolution);
        player.Init(skinController.GetDermisLayers());

        mainCam.transform.position += skinController.GetSize() * 0.5f;

        ready = true;
    }

    void Update()
    {
        if (ready)
        {

        }
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
