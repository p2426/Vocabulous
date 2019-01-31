﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordSearchTable : MonoBehaviour
{
    private GC gameController;
    public Tile_Controlller startOverlay;
    public GameObject startObjects;
    public bool onHoverOver;
    public Color normalColour, hoveredColour;

    ConDice[] tableDice;
    Vector3[] diceStartPos = new Vector3[10];
    Quaternion[] diceStartRot = new Quaternion[10];

    // Start is called before the first frame update
    void Start()
    {
        gameController = GC.Instance;
        startOverlay.setID(4441);

        tableDice = startObjects.GetComponentsInChildren<ConDice>();

        normalColour = tableDice[0].DiceBody.GetComponent<Renderer>().material.color;

        for (int i = 0; i < tableDice.Length; i++)
        {
            diceStartPos[i] = tableDice[i].gameObject.transform.position + new Vector3(0, 0.5f, 0);
            diceStartRot[i] = tableDice[i].gameObject.transform.rotation;
            tableDice[i].killOverlayTile();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // checking hover over
        if (gameController.HoverChange && gameController.NewHoverOver == 4441 && !onHoverOver)
        {
            onHoverOver = true;
            SetHoverColourOn();
            StartCoroutine(ShiftDiceToReadyPosition(3f));
        }
        if (onHoverOver && gameController.NewHoverOver != 4441)
        {
            onHoverOver = false;
            SetNormalColourOn();
            StartCoroutine(ShiftDiceToStartPosition(3f));
        }
    }

    public void HideStartObjects() { startObjects.SetActive(false); }
    public void HideAll()
    {
        startObjects.SetActive(false);
    }

    public void SetHoverColourOn()
    {
        foreach(ConDice dice in tableDice)
        {
            dice.ChangeDiceColor(hoveredColour);
        }
    }

    public void SetNormalColourOn()
    {
        foreach (ConDice dice in tableDice)
        {
            dice.ChangeDiceColor(normalColour);
        }
    }

    IEnumerator ShiftDiceToReadyPosition(float finishTime)
    {
        float t = 0;
        while (t < finishTime)
        {
            foreach (ConDice dice in tableDice)
            {
                dice.transform.position = Vector3.Lerp(dice.transform.position, dice.transform.parent.transform.position, t / finishTime);
                dice.transform.rotation = Quaternion.Lerp(dice.transform.rotation, dice.transform.parent.transform.rotation, t / finishTime);
            }
            t += Time.deltaTime;
            if (onHoverOver && gameController.NewHoverOver != 4441 || gameController.GameState == 35) yield break;
            yield return null;
        }
        yield break;
    }

    IEnumerator ShiftDiceToStartPosition(float finishTime)
    {
        float t = 0;
        while (t < finishTime)
        {
            for (int i = 0; i < tableDice.Length; i++)
            {
                tableDice[i].transform.position = Vector3.Lerp(tableDice[i].transform.position, diceStartPos[i], t / finishTime);
                tableDice[i].transform.rotation = Quaternion.Lerp(tableDice[i].transform.rotation, diceStartRot[i], t / finishTime);
            }
            t += Time.deltaTime;
            if (gameController.HoverChange && gameController.NewHoverOver == 4441 && !onHoverOver || gameController.GameState == 35) yield break;
            yield return null;
        }
        yield break;
    }
}
