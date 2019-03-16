using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameTaco;

public class FakeTournaments : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void OpenFakeTournaments()
	{
		TacoManager.ShowPanel(PanelNames.MyTournamentsFakePanel);
	}
}
