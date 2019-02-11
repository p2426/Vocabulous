﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConAnagram : MonoBehaviour
{
    private GC gc;
    public string Anagram;
    [SerializeField]
    private string CurrWord;
    [SerializeField]
    private List<string> AnswersList;
    private List<ConAnagramWord> ToGets;
    public Vector3 AnswerListOffset;
    public int AnswersListWidth;
    public float AnswersListPitch;
    private AnagramLevels AL;
    public GameObject AnswersDisplay;
    public GameObject TilesDisplay;
    [SerializeField]
    private List<string> letters;
    private List<int> selected;
    private List<string> playerAnswers;
    [SerializeField]
    private bool Selecting = false;
    public Vector3 GridCentre;
    public float radius;
    [SerializeField]
    private int GameState = 0; // 0 = on Big Table, 1 = starting, 2 = playing, 3 = ended, awaiting restart
    private int ToFind = 0;

    // Start is called before the first frame update
    void Start()
    {
        AL = GetComponent<AnagramLevels>();
        gc = GC.Instance;
        AnswersList = new List<string>();
        ToGets = new List<ConAnagramWord>();
        letters = new List<string>();
        selected = new List<int>();
        playerAnswers = new List<string>();

        StartGame();
    }

    void StartGame ()
    {
        AnswersList = AL.GetAnagramLevel(gc.player.ALevel);
        Anagram = AnswersList[0];
        ToFind = AnswersList.Count - 1;
        foreach (char c in Anagram)
        {
            letters.Add("" + c);
        }
        shuffle(letters);
        Debug.Log(Anagram);
        DisplayHand();
        DisplayToGets();
        GameState = 2;
    }

    void DisplayHand ()
    {
        int count = letters.Count;
        float angle = 360.0f / (float)count;
        Vector3 offset = new Vector3(radius, 0, 0);
        for (int i = 0; i < count; i++)
        {
            // thanks to https://forum.unity.com/threads/rotating-a-vector-by-an-eular-angle.18485/ (Feb 2019)
            offset = Quaternion.AngleAxis(angle, Vector3.up) * offset;
            string IWant = letters[i] + "_";
            GameObject tile = gc.assets.SpawnTile(IWant, Vector3.zero, false, true);
            tile.transform.parent = TilesDisplay.transform;
            tile.transform.localPosition = GridCentre + offset;
            Con_Tile2 con = tile.GetComponent<Con_Tile2>();
            con.SetFullFaceID(i, i);
        }

    }

    void DisplayToGets()
    {
        int row = 0;
        int count = 0;
        for (int i = 1; i < AnswersList.Count; i++)
        {
            int len = AnswersList[i].Length;
            if (count + len >= AnswersListWidth)
            {
                row++;
                count = 0;
            }
            GameObject ToGet = gc.assets.MakeWordFromTiles(AnswersList[i], Vector3.zero, 1f, true, false, false);
            ToGet.transform.parent = AnswersDisplay.transform;
            ToGet.transform.localPosition = AnswerListOffset + new Vector3(count, 0, row * AnswersListPitch);
            ToGet.AddComponent<ConAnagramWord>();
            ToGet.GetComponent<ConAnagramWord>().myWord = AnswersList[i];
            ToGets.Add(ToGet.GetComponent<ConAnagramWord>());
            count += len + 1;
        }
    }

    private List<string> shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            string tmp = list[i];
            int r = Random.Range(i, list.Count-1);
            list[i] = list[r];
            list[r] = tmp;
        }
        return list;
    }

    public void KickOff()
    {

    }
    
    public void TidyUp()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Selecting)
        {
            CurrWord = "";
            foreach (int i in selected)
            {
                CurrWord += letters[i];
            }
        }

        CheckMouseClicks();
        CheckHoverOver();
    }

    void CheckMouseClicks()
    {
        if (GameState == 2) // game running
        {
            if (!Selecting) // not currently selecting anything
            {
                // Mouse goes down over a non -1 or 9999 IisOverlayTile object (i.e. a valid letter)
                if (Input.GetMouseButtonDown(0) && gc.NewHoverOver != -1 && gc.NewHoverOver != 9999)
                {
                    Selecting = true;
                    selected.Add(gc.NewHoverOver);
                }
            }
            else // are selecting
            {
                if (Input.GetMouseButtonUp(0))
                {
                    Selecting = false;
                    string res = "";
                    foreach (int i in selected)
                    {
                        res = res + letters[i];
                    }
                    selected.Clear();
                    if (AnswersList.Contains(res) && !playerAnswers.Contains(res)) // found standard answer
                    {
                        // ANIMATE - Gratz found one !!
                        // ANIMATE reveal answer
                        Debug.Log("You found: " + res);
                        playerAnswers.Add(res);
                        ToFind--;
                        foreach (ConAnagramWord t in ToGets)
                        {
                            if (t.myWord == res)
                            {
                                t.Roll(0.1f);
                            }
                        }
                        // check for game end
                        if (ToFind == 0)
                        {
                            EndGame();
                        }
                    }
                    else if (gc.maxTrie.CheckWord(res)) // NEW non-standard word
                    {
                        if (!playerAnswers.Contains(res)) // and not already found
                        {
                            // ANIMATE GREAT WORD, new for us
                            Debug.Log("GREAT new word");
                            playerAnswers.Add(res);
                        }
                        else
                        {
                            // ANIMATE - You've already got that one
                            Debug.Log("Already have that one");
                        }
                    }
                    else
                    {
                        // ANIMATE - sorry word not recognised
                        Debug.Log("Not a word");
                    }
                }
            }
        }
        if (GameState == 3) // looking for restart
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (gc.NewHoverOver == 6662) // restart game
                {
                    Debug.Log("Attempting restart");
                    ResetGame();
                    KickOff();
                }
                if (gc.NewHoverOver == 6663) // back to menu
                {
                    Debug.Log("Want to Return to main table ... but not connected yet");
                }
            }
        }
    }

    void CheckHoverOver()
    {
        if (gc.HoverChange && Selecting && GameState == 2) // game running
        {
            if (gc.NewHoverOver == -1) // off grid - reset
            {
                selected.Clear();
                Selecting = false;
            }
            else
            {
                if (gc.NewHoverOver == selected[selected.Count -1]) // back track
                {
                    selected.RemoveAt(selected.Count - 1);
                }
                else
                {
                    if (gc.NewHoverOver < Anagram.Length && !selected.Contains(gc.NewHoverOver))
                    selected.Add(gc.NewHoverOver);
                }
            }

        }
    }

    void EndGame()
    {
        Debug.Log("Game Over .. you got them all");
        gc.player.ALevel++;
        gc.SaveStats();
        ResetGame();
    }

    void ResetGame()
    {
        foreach (Transform child in AnswersDisplay.transform)
        {
            Destroy(child.gameObject);
        }
        Transform[] let = TilesDisplay.GetComponentsInChildren<Transform>();
        for (int i = 1; i < let.Length; i++) // first one is the Overlay Tile 
        {
            Destroy(let[i].gameObject);
        }

        AnswersList = new List<string>();
        ToGets = new List<ConAnagramWord>();
        letters = new List<string>();
        selected = new List<int>();
        playerAnswers = new List<string>();
        StartGame();
    }




    // Legacy script, used to determine Anagram candidates
    void ExamineWords()
    {
        double start = Time.realtimeSinceStartup;
        File_Reader fr = new File_Reader();
        File_Writer fw = new File_Writer();
        List<string> miniDict = new List<string>();
        fr.open("/Dictionaries/anagram_candidates.txt");
        bool reading = true;
        while (reading)
        {
            string word = fr.nextLine();
            if (word == null)
            {
                reading = false;
                fr.close();
            }
            else
            {
                miniDict.Add(word);
            }
        }
        Debug.Log("MiniDict loaded : " + miniDict.Count.ToString() + " words");

        fr.open("/Dictionaries/anagram_candidates.txt");
        fw.open("/Dictionaries/answers.txt");
        reading = true;
        int con = 0;
        while (reading)
        {
            string word = fr.nextLine();
            if (word == null)
            {
                reading = false;
                fr.close();
            }
            else
            {
                if (word.Length >= 4)
                {
                    List<string> sub = gc.assets.SortList(gc.maxTrie.getAnagram(word, false, 3));
                    int num = 0;
                    foreach (string s in sub)
                    {
                        if (miniDict.Contains(s)) num++;
                    }
                    if (num >= word.Length)
                    {
                        string ret = word;
                        foreach (string s in sub)
                        {
                            if (miniDict.Contains(s)) ret = ret + " " + s;
                        }
                        fw.writeLine(ret);
                        con++;
                    }
                }
            }
        }
        fw.close();
        Debug.Log("Words examinied : " + con.ToString() + " candidates in " + (Time.realtimeSinceStartup - start).ToString() + " secs");
    }



}
