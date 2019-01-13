﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public SC sceneController;
    public GameObject[] neighbouringCubes;
    Renderer cubeRenderer;
    TextMesh textMesh;
    char letter;
    public bool wasSelected;

    void Start()
    {
        /* cache components */
        cubeRenderer = GetComponent<Renderer>();
        textMesh = transform.GetChild(0).GetComponent<TextMesh>();

        /* set available letters */
        string alphabet = "abcdefghijklmnopqrstuvwxyz";
        /* pick random element within the string, and set it to the 3D Test mesh's text value */
        letter = alphabet[Random.Range(0, alphabet.Length)];
        textMesh.text = letter.ToString();
    }

    /* overriding native Unity click on 3D object */
    void OnMouseDown()
    {
        if (sceneController.currentTurn < sceneController.maximumTurns)
        {
            sceneController.currentTurn++;
            wasSelected = true;
            Debug.Log(letter + " clicked, adding to search -- " + System.DateTime.Now);

            /* add letter to the Trie search array */
            sceneController.Trie.TrieAddToSearch(letter);
            /* add myself to list in sceneController so it can change the colour back to default when search is over */
            sceneController.SelectedCubes.Add(this.gameObject);
            /* tell controller to delete any memory of previous neighbour cubes and send this cubes neighbours */
            sceneController.NeighboursDelete();
            foreach (GameObject neighbours in neighbouringCubes) sceneController.NeighbourCubes.Add(neighbours);
            sceneController.NeighboursNew();
            /* change colour to 'highlighted' */
            cubeRenderer.material.color = sceneController.cubeSelectedColour;
        }
    }

    /* called by SC
     * when word is found/completed, change colour a bit */ 
    public IEnumerator cubeFoundWord(float t)
    {
        Material thisMat = GetComponent<Renderer>().material;
        Color defaultColour = sceneController.cubeDefaultColour;
        float colourTimer = 0.0f;
        while (colourTimer < t)
        {
            thisMat.color = Color.Lerp(Color.white, defaultColour, colourTimer / t);
            colourTimer += Time.deltaTime;
            yield return null;
        }
        thisMat.color = defaultColour;
        wasSelected = false;
        yield break;
    }
}
