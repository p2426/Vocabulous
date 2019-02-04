﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GC : MonoBehaviour
{
    #region Variable Declaration
    [Header("HoverOver -1 = nowt, 9999 = edgelike, 0-xxx = grid location")]
    public bool HoverChange = false; // quick "has hover changed bool" for frame to frame checking
    // a lot of messing about ... but ... https://answers.unity.com/questions/915032/make-a-public-variable-with-a-private-setter-appea.html
    // changed in Update() to reflect code of what mouse is over
    public int NewHoverOver { get { return _NewHoverOver; } private set { _NewHoverOver = value; } }
    [SerializeField]
    private int _NewHoverOver = -1;
    // reflects the HoverOvervalue from the PREVIOUS frame (for frame by frame comparison)
    public int OldHoverOver { get { return _OldHoverOver; } private set { _OldHoverOver = value; } }
    [SerializeField]
    private int _OldHoverOver = -1;

    [Header("The Trie's")]
    public MaxTrie maxTrie;
    public TrieTest phoenixTrie;

    [Header("Player Manager")]

    [Header("Player Info")]
    [SerializeField]
    private string PlayerName;
    public PlayerStats player;
    public PlayerManager playerManager = new PlayerManager();

    [Header("WordDice Info")]
    [SerializeField]
    private int WorddiceLength;

    [Header("WordSearch Info")]
    [SerializeField]
    int wordSearchGridXLength;
    [SerializeField]
    int wordSearchGridYLength;
    [SerializeField]
    int wordSearchMinimumLengthWord;
    [SerializeField]
    int wordSearchMaximumLengthWord;
    [SerializeField]
    int wordSearchFourLetterWordsCount;
    [SerializeField]
    int wordSearchFiveLetterWordsCount;
    [SerializeField]
    int wordSearchSixLetterWordsCount;
    [SerializeField]
    int wordSearchSevenLetterWordsCount;
    [SerializeField]
    int wordSearchEightLetterWordsCount;

    [Header("Assets")]
    public OurAssets assets;

    [Header("SoundManager(sm)")]
    public SoundMan sm;

    [Header("Game Positions (for positional tweaking)")] // can't have a transform (more's the pity)
    public Vector3 PosWordSearch = new Vector3();
    public Vector3 PosTranWordDice = new Vector3();
    public Vector3 PosTranAnagram = new Vector3();
    public Vector3 PosTranWordrop = new Vector3();
    public Vector3 PosTranSolver = new Vector3();
    [Header("Game Rotations (for rotational tweaking)")]
    public Vector3 RotWordSearch = new Vector3();
    public Vector3 RotTranWordDice = new Vector3();
    public Vector3 RotTranAnagram = new Vector3();
    public Vector3 RotTranWordrop = new Vector3();
    public Vector3 RotTranSolver = new Vector3();
    [Header("Game Scales (for scale tweaking)")]
    public Vector3 ScaleWordSearch = new Vector3();
    public Vector3 ScaleTranWordDice = new Vector3();
    public Vector3 ScaleTranAnagram = new Vector3();
    public Vector3 ScaleTranWordDrop = new Vector3();
    public Vector3 ScaleTranSolver = new Vector3();
    [Header("Default Dice/face colours")]
    public Color ColorBase = new Color();
    public Color ColorSelected = new Color();
    public Color ColorHighlight = new Color();
    public Color ColorLegal = new Color();

    [Header("THE GAME STATE")]
    public int GameState = 0;
    //... needs to be agreed ... maybe 
    // 0 = initialising/loading
    // 1 = at table (choosing)
    // 2 = transition to game area
    // 30 = In Game
    // 31 = Playing WordDice
    // 32 = Playing Solver
    // 33 = Playing Anagram
    // 34 = Playing WordDrop
    // 35 = Playing WordSearch
    // 5 = transitioning from a game to 1 again
    // 9 = Quitting

    [Header("The GAME OBJECTS")]
    public UIC UIController;
    public CameraController cameraController;
    public ConWordDice WordDice;
    public WordSearchController wordSearchController;
    public ConTypeWriter solverController;

     #endregion


    #region Set Singelton
    // --------------------//
    // establish Singelton //
    // ------------------- //
    public static GC Instance
    {
        get
        {
            return instance;
        }
    }
    private static GC instance = null;
    void Awake()
    {
        if (instance)
        {
            Debug.Log("Already a GameController - going to die now .....");
            DestroyImmediate(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        // ESSENTIAL IF YOU ARE EDDITING PlayerStats stuff
        playerManager.ResetToDefault();
        player = playerManager.LoadPlayer();
        // Do stuff for a new player?  Say Hi ?
        PlayerName = player.Name;

        WorddiceLength = player.WordDiceGameLength;

        wordSearchGridXLength = player.WordSearchSize;
        wordSearchGridYLength = player.WordSearchSize;
        wordSearchMinimumLengthWord = player.WordSearchMinimumLengthWord;
        wordSearchMinimumLengthWord = player.WordSearchMaximumLengthWord;
        wordSearchFourLetterWordsCount = player.WordSearchFourLetterWordsCount;
        wordSearchFiveLetterWordsCount = player.WordSearchFiveLetterWordsCount;
        wordSearchSixLetterWordsCount = player.WordSearchSixLetterWordsCount;
        wordSearchSevenLetterWordsCount = player.WordSearchSevenLetterWordsCount;
        wordSearchEightLetterWordsCount = player.WordSearchEightLetterWordsCount;
}
    //---------------------------//
    // Finished Singelton set up //
    // --------------------------//
    #endregion


    #region Unity API
    // Start is called before the first frame update
    void Start()
    {
        NewHoverOver = -1;
        OldHoverOver = -1;
        // all loaded
        GameState = 1;
        // Set game locations/rotation
        if (WordDice != null)
        {
            WordDice.transform.position = PosTranWordDice;
            WordDice.transform.localRotation = Quaternion.Euler(RotTranWordDice);
            WordDice.transform.localScale = ScaleTranWordDice;
        }
        if (wordSearchController != null)
        {
            wordSearchController.transform.position = PosWordSearch;
            wordSearchController.transform.localRotation = Quaternion.Euler(RotWordSearch);
            wordSearchController.transform.localScale = ScaleWordSearch;
        }
        if (solverController != null)
        {
            solverController.transform.position = PosTranSolver;
            solverController.transform.rotation = Quaternion.Euler(RotTranSolver);
            solverController.transform.localScale = ScaleTranSolver;
        }
    }


    // Update is called once per frame
    void Update()
    {
        // testings
        if (Input.GetKeyDown(KeyCode.G)) wordSearchController.Initialise();
        if (Input.GetKeyDown(KeyCode.T))
        {
            GameObject thing;
            if (Random.value > 0.5) thing = assets.SpawnTile("questquest", cameraController.transform.position, Random.value > 0.5, Random.value > 0.5);
            else thing = assets.SpawnDice("?", cameraController.transform.position);
            Rigidbody rb = thing.AddComponent<Rigidbody>();
            thing.transform.localRotation = Random.rotation;
            rb.AddForce((cameraController.transform.forward + new Vector3(0, 0.35f, 0)) * 1000);
        }

        // sets HoverOver values to the returned value from any IisOverlayTile class (if none, then -1)
        CheckHoverOver();
        CheckClicks();
    }
    #endregion


    #region HoverOver and Clicks
    void CheckHoverOver()
    {
        OldHoverOver = NewHoverOver;
        NewHoverOver = -1;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if (hit.collider != null)
            {
                IisOverlayTile tile = hit.collider.GetComponent<IisOverlayTile>();
                if (tile != null)
                {
                    NewHoverOver = tile.getID();
                }
            }
        }
        if (OldHoverOver == NewHoverOver) HoverChange = false;
        else { HoverChange = true; }
    }


    private void CheckClicks()
    {
        // when clicking the intro object for a game at the table, rotate to it
        if (GameState == 1)
        {
            // WordDice
            if (Input.GetMouseButtonDown(0) && NewHoverOver == 8881) { cameraController.RotateToGameWordDice(); }
            // Solver
            if (Input.GetMouseButtonDown(0) && NewHoverOver == 7771) { cameraController.RotateToSolver(); }
            // Anagram
            //if (Input.GetMouseButtonDown(0) && NewHoverOver == 6661) { cameraController.RotateToGameAnagram(); }
            // WordDrop
            //if (Input.GetMouseButtonDown(0) && NewHoverOver == 5551) { cameraController.RotateToGameWordDrop(); }
            // WordSearch
            if (Input.GetMouseButtonDown(0) && NewHoverOver == 4441) { cameraController.RotateToGameWordSearch(); }
        }
        // however, if we already transitioned to that game, go start it up
        if (GameState == 2)
        {
            // WordDice
            if (Input.GetMouseButtonDown(0) && NewHoverOver == 8881) { SetGameState(31); }
            // Solver
            if (Input.GetMouseButtonDown(0) && NewHoverOver == 7771) { SetGameState(32); }
            // Anagram
            //if (Input.GetMouseButtonDown(0) && NewHoverOver == 6661) { SetGameState(33); }
            // WordDrop
            //if (Input.GetMouseButtonDown(0) && NewHoverOver == 5551) { SetGameState(34); }
            // WordSearch
            if (Input.GetMouseButtonDown(0) && NewHoverOver == 4441) { SetGameState(35); }
        }
    }

    #endregion


    #region State Controls
    // main function for all controllers
    public void SetGameState(int i)
    {
        int prevState = GameState;
        GameState = i;

        switch (i)
        {
            case 0: break;
            case 1:
                {
                    // called at Start() and by cameraController once it has finished its lerp back out of game area
                    // 1 = at table (choosing)
                    break;
                }
            case 2:
                {
                    // called by 'Play' Button in inspector
                    // 2 = transition to game area
                    cameraController.PlayClicked();
                    UIController.PlayClicked();
                    break;
                }
            case 5:
                {
                    // called by 'Quit' Button in inspector
                    // 5 = transitioning from a game to 1 again
                    cameraController.QuitClicked();
                    UIController.QuitClicked();
                    ReEnableAllGames();
                    OnThisGameQuit(prevState);
                    break;
                }
            case 31:
                {
                    // called in 'CheckClicks()'
                    // 31 = Playing WordDice
                    WordDice.KickOff();
                    DisableOtherGames(WordDice.gameObject);
                    break;
                }
            case 32:
                {
                    // called in 'CheckClicks()'
                    // 32 = Playing Solver
                    solverController.KickOff();
                    DisableOtherGames(solverController.gameObject);
                    break;
                }
            case 33:
                {
                    // called in 'CheckClicks()'
                    // 33 = Playing Anagram
                    break;
                }
            case 34:
                {
                    // called in 'CheckClicks()'
                    // 34 = Playing WordDrop
                    break;
                }
            case 35:
                {
                    // called in 'CheckClicks()'
                    // 35 = Playing WordSearch
                    if (!wordSearchController.isInitialised) wordSearchController.Initialise();
                    else wordSearchController.Restart();
                    DisableOtherGames(wordSearchController.gameObject);
                    break;
                }
        }
    }

    void DisableOtherGames(GameObject thisController)
    {
        if (thisController != WordDice.gameObject) WordDice.gameObject.SetActive(false);
        if (thisController != wordSearchController.gameObject) wordSearchController.gameObject.SetActive(false);
        //if (thisController != anagramController.gameObject) anagramController.gameObject.SetActive(false);
        //if (thisController != wordDropController.gameObject) wordDropController.gameObject.SetActive(false);
        if (thisController != solverController.gameObject) solverController.gameObject.SetActive(false);
    }

    void ReEnableAllGames()
    {
        WordDice.gameObject.SetActive(true);
        wordSearchController.gameObject.SetActive(true);
        //anagramController.gameObject.SetActive(true);
        //wordDropController.gameObject.SetActive(true);
        solverController.gameObject.SetActive(true);
    }

    void OnThisGameQuit(int i)
    {
        switch(i)
        {
            case 31:
                {
                    // we just quit WordDice, do something special
                    WordDice.TidyUp();
                    break;
                }
            case 32:
                {
                    // we just quit Solver, do something special
                    solverController.TidyUp();
                    break;
                }
            case 33:
                {
                    // we just quit Anagram, do something special
                    break;
                }
            case 34:
                {
                    // we just quit WordDrop, do something special
                    break;
                }
            case 35:
                {
                    // we just quit WordSearch, do something special
                    wordSearchController.TidyUp();
                    break;
                }
        }
    }

    #endregion


    #region PlayerStats manipulation
    // Any game/GUI SHOULD use this to save any changes to gc.player (the PlayerStats Struct)
    public void SaveStats()
    {
        playerManager.SavePlayer(player);
    }
    #endregion
}
