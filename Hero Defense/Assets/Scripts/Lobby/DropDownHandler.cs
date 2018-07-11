using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Prototype.NetworkLobby;


public class DropDownHandler : MonoBehaviour
{
        
    public Dropdown dropdown;

    LobbyPlayer lobbyPlayer;       

    

    private void Start()
    {
        lobbyPlayer = GetComponent<LobbyPlayer>();

        PopulateList();
    }


    void PopulateList()
    {
        List<string> names;
        names = new List<string>();

        string[] enumNames = Enum.GetNames(typeof(CharacterEnum));

        for (int i = 0; i < enumNames.Length; i++)
        {
            names.Add(enumNames[i]);
        }
        dropdown.AddOptions(names);
    }

    public void Dropdown_IndexChanged(int index)
    {        
        lobbyPlayer.OnCharChanged(index);
    }
}
