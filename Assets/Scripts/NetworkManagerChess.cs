using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

public class NetworkManagerChess : NetworkManager
{
    public GameObject gameManager;
    
    public GameManager gameMngScript;

    private GameObject whitePlayer;
    private GameObject blackPlayer;
    

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject player = Instantiate(playerPrefab, Vector3.zero, quaternion.identity);

        // add player at correct spawn position
        NetworkServer.AddPlayerForConnection(conn, player);
        if(numPlayers == 1)
            player.GetComponent<PlayerChess>().ChangeColor(PlayerTeam.WHITE);
        else 
            player.GetComponent<PlayerChess>().ChangeColor(PlayerTeam.BLACK);

        // spawn ball if two players
        if (numPlayers == 2)
        {
            GameObject gameMng = Instantiate(gameManager, Vector3.zero, Quaternion.identity);
            Debug.Log("NumPlayer is 2");
            NetworkServer.Spawn(gameMng);
            gameMngScript = gameMng.GetComponent<GameManager>();
            foreach (var connection in NetworkServer.connections)
            {
                Debug.Log("Setting a connections");
                connection.Value.identity.GetComponent<PlayerChess>().gameManager = gameMngScript;
            }

            gameMngScript.blackPlayer = blackPlayer;
            gameMngScript.whitePlayer = whitePlayer;
            
        }
    }

}
