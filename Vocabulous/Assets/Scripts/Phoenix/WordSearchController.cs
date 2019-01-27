﻿/* random letter based on weighting: http://pi.math.cornell.edu/~mec/2003-2004/cryptography/subs/frequencies.html?fbclid=IwAR3gpj-HzjT6s2GQ2wBlYq4eZbdJ7uA6SjFhSrcDYb-CXHBtpaB3cdCjyr0 */

using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class WordSearchController : MonoBehaviour
{
    private GC gameController;
    private GameGrid grid;
    public GameObject diceHolder;
    public GameObject OverlayPrefab;
    public TrieTest trie;
    private int gridXLength = 20;
    private int gridYLength = 20;
    [SerializeField]
    private int minimumLengthWord = 4;
    [SerializeField]
    private int maximumLengthWord = 8;
    private int numberOfWords = 15;
    //                                  will longer length/default these off some research when less tierd
    private int fourLetterWordsCount = 3;
    private int fiveLetterWordsCount = 4;
    private int sixLetterWordsCount = 2;
    private int sevenLetterWordsCount = 2;
    private int eightLetterWordsCount = 2;
    //
    public List<string> foundWords = new List<string>();
    public List<string> unfoundWords = new List<string>();
    public Dictionary<string, uint> weights = new Dictionary<string, uint>
    {
        { "a", 11306 }, { "b", 9764 }, { "c", 17500 }, { "d", 11594 }, { "e", 8016 },{ "f", 7074 },
        { "g", 6525 }, { "h", 7043 }, { "i", 7402 }, { "j", 1759 }, { "k", 2120 },{ "l", 5728 },
        { "m", 10863 }, { "n", 3808 }, { "o", 5490 }, { "p", 16442 }, { "q", 1086 },{ "r", 9993 },
        { "s", 22310 }, { "t", 10543 }, { "u", 8067 }, { "v", 3423 }, { "w", 4005 },{ "x", 237 },
        { "y", 729 }, { "z", 855 }
    };
    private uint totalWeight = 0;
    [SerializeField]
    private bool selecting = false;
    private int initialWordsCounter = 0;
    private string defaultString = "0";
    private int debugPlacedWords = 0;

    public void Initialise()
    {

        /* gamecontroller and initialising variables */
        gameController = GC.Instance;
        trie = gameController.phoenixTrie;
        gridXLength = gameController.gridXLength;
        gridYLength = gameController.gridYLength;
        minimumLengthWord = gameController.minimumLengthWord;
        maximumLengthWord = gameController.maximumLengthWord;
        fourLetterWordsCount = gameController.fourLetterWordsCount;
        fiveLetterWordsCount = gameController.fiveLetterWordsCount;
        sixLetterWordsCount = gameController.sixLetterWordsCount;
        sevenLetterWordsCount = gameController.sevenLetterWordsCount;
        eightLetterWordsCount = gameController.eightLetterWordsCount;

        /* check maximumWordLength against maximum word in dictionary and bounds of grid so everything fits/is a legal word */
        if (maximumLengthWord > 17) maximumLengthWord = 17;
        if (gridXLength < maximumLengthWord)
        {
            maximumLengthWord = gridXLength;
            if (gridYLength < maximumLengthWord)
            {
                maximumLengthWord = gridYLength;
            }
        }

        /* sum weights.Value for 'totalWeight' from start as this wont change */
        foreach (KeyValuePair<string, uint> weight in weights)
        {
            totalWeight = totalWeight + weight.Value;
        }

        /* setup grid and tiles */
        grid = new GameGrid() { dx = gridXLength, dy = gridYLength };
        grid.init();
        grid.directional = true;

        /* populate with dummy data so the grid can check 'legals' for placing words in */
        for (int i = 0; i < gridXLength * gridYLength; i++)
        {
            grid.PopulateBin(i, defaultString);
        }

        /* populate with initial words to be placed 'unfoundWords' List */
        for (int i = 0; i < fourLetterWordsCount; i++) { PopulateInitialWords(4, fourLetterWordsCount, false); }
        for (int i = 0; i < fiveLetterWordsCount; i++) { PopulateInitialWords(5, fiveLetterWordsCount, false); }
        for (int i = 0; i < sixLetterWordsCount; i++) { PopulateInitialWords(6, sixLetterWordsCount, false); }
        for (int i = 0; i < sevenLetterWordsCount; i++) { PopulateInitialWords(7, sevenLetterWordsCount, false); }
        for (int i = 0; i < eightLetterWordsCount; i++) { PopulateInitialWords(8, eightLetterWordsCount, false); }

        /* start placing words in the grid and check their legality (possible recursion for each word) */
        foreach (string word in unfoundWords)
        {
            InsertWordToGrid(word);
        }
        Debug.Log("Finished populating words, successfully placed " + debugPlacedWords + " words out of " + unfoundWords.Count + " on the unfoundWords list");

        /* bosh in some random weighted letters where there are only 'defaultLetters' left */
        for (int i = 0; i < gridXLength * gridYLength; i++)
        {
            if (grid.bins[i] == defaultString) grid.PopulateBin(i, GetRandomLetter(totalWeight));
        }

        /* now pop the cubes in */
        int count = 0;
        for (int z = gridYLength; z > 0; z--)
        {
            for (int x = 0; x < gridXLength; x++)
            {
                GameObject dice = gameController.assets.SpawnDice(grid.bins[count], new Vector3(diceHolder.transform.position.x + x, diceHolder.transform.position.y, diceHolder.transform.position.z + z));
                dice.transform.parent = diceHolder.transform;
                ConDice diceCon = dice.GetComponent<ConDice>();
                diceCon.ID = count;
                diceCon.myGrid = grid;
                count++;
            }
        }
        diceHolder.transform.localRotation = transform.localRotation;
    }

    void Update()
    {
        if (selecting) grid.GetCurrentPath();

        if (gameController != null)
        {
            CheckGCHoverValue();
            InputAndSearch();
        }
    }

    /* recursive populate 'unfoundWords' list */
    void PopulateInitialWords(int len, int numberOfWordsCount, bool basedOnSameAnagram)
    {
        Debug.Log("Populating initial words..");
        /* create a random string to the lenth 'len' we want */
        string s = "";
        for (int i = 0; i < len; i++)
        {
            s = s + GetRandomLetter(totalWeight);
        }
        /* search trie with string 's' and store result with..
         * s = string, anagram = true, exactCompare = false, storeWords = true, lengthOfStoredWords = len, debug = false */
        bool success = trie.SearchString(s, true, false, true, len, false);

        if (trie.lastStoredWords.Count < len) success = false;

        if (success)
        {
            /* grab the amount 'numberOfWordsCount' we need at random if it's not already in the List 'unfoundWords' */
            if (basedOnSameAnagram)
            {
                for (int i = 0; i < numberOfWordsCount; i++)
                {
                    int randIndex = Random.Range(0, trie.lastStoredWords.Count - 1);
                    if (!unfoundWords.Contains(trie.lastStoredWords[randIndex])) unfoundWords.Add(trie.lastStoredWords[randIndex]);
                    /* if the random number made the controller add the same word twice or more, go mental */
                    else
                    {
                        for (int a = 0; a < numberOfWordsCount - i; a++)
                        {
                            if (!unfoundWords.Contains(trie.lastStoredWords[a])) unfoundWords.Add(trie.lastStoredWords[a]);
                        }
                    }
                }
            }
            /* do the same but pick at random for 1 word, not the same anagram for all words */
            else
            {
                int randIndex = Random.Range(0, trie.lastStoredWords.Count);
                if (unfoundWords.Contains(trie.lastStoredWords[randIndex])) success = false;
                else unfoundWords.Add(trie.lastStoredWords[randIndex]);
            }
        }
        /* recursion */
        else
        {
            Debug.Log("Failed to populate words, repopulating");
            PopulateInitialWords(len, numberOfWordsCount, false);
        }
    }

    /* based on accumulated weight > random number, return string 'Key' in Dictionary 'weights' */
    string GetRandomLetter(uint tw)
    {
        float rand = Random.Range(0, tw + 1);
        float accumWeight = 0;
        int count;

        for (count = 0; count < weights.Count; count++)
        {
            accumWeight = accumWeight + weights.Values.ElementAt(count);
            if (accumWeight > rand) break;
        }
        return weights.ElementAt(count).Key;
    }

    /* place words in 'unfoundWords' into the grid at random start position while checking if it would be legal */
    void InsertWordToGrid(string word)
    {
        Debug.Log("-- Function Start with: " + word + " --");
        /* clean up in case of recursion 
         * stringPos is for where we will need to compare which letter is at the current path so we can cross through or not */
        bool success = true;

        /* add first position anywhere */
        grid.AddToPath(Random.Range(0, gridXLength * gridYLength));

        /* add another path at any legal spot and record the direction */
        grid.AddToPath(grid.legals[Random.Range(0, grid.legals.Count)]);

        /* for the rest of the positions, now the 'diagonal' bool will kick in and now there can only be 1 possible legal move */
        for (int i = 0; i < word.Length - 1; i++)
        {
            /* before we do the rest of the loop, are we at an edge with no legal moves? set success to false, make sure to call FINISHPATH() not CLEARPATH() or there will be errors (direction will not be reset) */
            if (grid.legals.Count == 0)
            {
                success = false;
                Debug.Log("Failed to find space for " + word);
                grid.FinishPath();
                break;
            }
            /* now just keep going to the only legal move */
            grid.AddToPath(grid.legals[0]);
            /* check again if we are at edge */
            if (grid.legals.Count == 0)
            {
                success = false;
                Debug.Log("Failed to find space for " + word);
                grid.FinishPath();
                break;
            }
        }


        /* one last check to see if our path consists of any words that have been taken and that do not cross over with the string at the right character */
        if (success)
        {
            for (int i = 0; i < grid.path.Count - 1; i++)
            {
                if (grid.bins[grid.path[i]] != defaultString)
                {
                    if (grid.bins[grid.path[i]] != word[i].ToString())
                    {
                        Debug.Log("Detected trying to put " + word[i] + " onto " + grid.bins[grid.path[i]]);
                        success = false;
                        break;
                    }
                }
            }
        }

        if (success)
        {
            /* succeeded: populate each part of the string for each position in the grid we took */
            for (int i = 0; i < grid.path.Count - 1; i++)
            {
                grid.PopulateBin(grid.path[i], word[i].ToString());
            }
            Debug.Log("Placed word: " + word);
            debugPlacedWords++;
            grid.FinishPath();
        }
        /* reached unsuccessful somewhere, recurse */
        else
        {
            grid.FinishPath();
            Debug.Log("Going recursive");
            InsertWordToGrid(word);
        }
    }

    /* input and trie search */
    void InputAndSearch()
    {
        /* hit a tile, mouse down */
        if (!selecting)
        {
            if (Input.GetMouseButtonDown(0) && gameController.NewHoverOver != -1)
            {
                selecting = true;
                grid.AddToPath(gameController.NewHoverOver);
            }
        }
        /* search string, mouse up */
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                selecting = false;
                bool isFound = false;
                string res = grid.GetCurrentPath();
                if (res.Length >= minimumLengthWord)
                {
                    if (trie.SearchString(res, false, true, false, 0, false))
                    {
                        /* do not use List.Contains(), it will find 'dog' if 'padog' is searched - better to look and find exact string matching */
                        for (int i = 0; i < unfoundWords.Count; i++)
                        {
                            if (unfoundWords[i] == res)
                            {
                                Debug.Log("You got " + res);
                                isFound = true;
                                foundWords.Add(res);
                                unfoundWords.Remove(res);
                                grid.HighlightCurrentPath();
                            }
                        }
                        if (!isFound) Debug.Log("Sorry, " + res + " is not on the list!");
                        grid.FinishPath();
                    }
                    else
                    {
                        Debug.Log("Sorry, " + res + " is not on the list!");
                        grid.FinishPath();
                    }
                }
            }
        }
    }

    /* middle man between GC and this class' grid */
    void CheckGCHoverValue()
    {
        if (gameController.NewHoverOver != gameController.OldHoverOver && selecting)
        {
            if (gameController.NewHoverOver == -1)
            {
                grid.ClearPath();
                selecting = false;
            }
            if (grid.legals.Contains(gameController.NewHoverOver) || grid.GetPathSecondFromEnd() == gameController.NewHoverOver) grid.AddToPath(gameController.NewHoverOver);
        }
    }
}
