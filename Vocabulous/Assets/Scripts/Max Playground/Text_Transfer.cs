﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Text_Transfer : MonoBehaviour
{
    public File_Reader reader;
    public File_Writer writer;
    private bool _running = false;

    // Start is called before the first frame update
    void Start()
    {
        reader.open("/Dictionaries/testText.txt");
        writer.open("/Dictionaries/testOutput.txt");
        _running = true;
        while (_running)
        {
            check(reader.nextLine());
        }

    }

    void check(string word)
    {
        if (word == null)
        {
            _running = false;
            closeAll();
        }
        else
        {
            writer.writeLine(word);
        }
    }

    void closeAll()
    {
        reader.close();
        writer.close();
        Debug.Log("How's them apples?");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
