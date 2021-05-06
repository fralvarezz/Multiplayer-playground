using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public enum PlayerTeam
{
    WHITE,
    BLACK
}


public class PlayerChess : NetworkBehaviour
{
    [SyncVar]
    public bool finishedTurn;

    public PlayerTeam team;

    public GameManager gameManager;


    [SyncVar]
    public Vector3 wannaMoveTo;
    [SyncVar]
    public GameObject clickedPiece;

    [SyncVar]
    public Cell previousCell;
    [SyncVar]
    public Cell nextCell;

    [SyncVar] 
    public Vector2Int previousCellPos;
    [SyncVar] 
    public Vector2Int nextCellPos;


    [SyncVar]
    public Vector3 newPiecePosition;


    private void FixedUpdate()
    {
        if(isLocalPlayer)
            SetManager();
        if (isLocalPlayer && !finishedTurn)
        {

            if (Input.GetMouseButtonDown(0))
            {
                GameObject clicked = GetClickedPiece();
                if (clicked != null)
                {
                    if (clickedPiece == clicked)
                    {
                        clickedPiece = null;
                    }
                    else if(clickedPiece == null)
                    {
                        clickedPiece = clicked;
                    }
                }
                else if (clickedPiece != null)
                {
                    Vector3 movingto = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    movingto.z = 0;
                    CmdSendMove(movingto, clickedPiece);
                    finishedTurn = true;
                }
                
            }
        }
    }

    [Command]
    private void CmdSendMove(Vector3 newPos, GameObject piece)
    {
        wannaMoveTo = newPos;
        clickedPiece = piece;
        finishedTurn = true;
    }

    [ClientRpc]
    public void ChangeColor(PlayerTeam color)
    {
        team = color;
    }

    private void SetManager()
    {
        if (gameManager == null)
        {
            GameObject gmMng = GameObject.FindGameObjectWithTag("GameManager");
            if (gmMng != null)
            {
                gameManager = gmMng.GetComponent<GameManager>();
            }
        }
    }

    private GameObject GetClickedPiece()
    {
        Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
        RaycastHit hit;

        GameObject clickedObject = null;
        GameObject goToReturn = null;
        
        if( Physics.Raycast( ray, out hit, 100 ) )
        {
            clickedObject = hit.transform.gameObject;
        }

        if (clickedObject != null)
        {
            if (clickedObject.CompareTag("BlackPiece") && team == PlayerTeam.BLACK || 
                clickedObject.CompareTag("WhitePiece") && team == PlayerTeam.WHITE)
            {
                goToReturn = clickedObject;
            }
        }
        return goToReturn;
    }

    [Command]
    public void CmdFinishTurn(Vector2Int previousCell, Vector2Int nextCell, GameObject pieceMoved, Vector3 newPiecePosition)
    {
        
        Debug.Log("Finsih turn calles");
        Debug.Log(previousCell);
        //send coordinates and pray that we can make that instead
        Board board = GameObject.FindWithTag("Board").GetComponent<Board>();

        this.previousCell = board.Cells[previousCell.x, previousCell.y];
        this.nextCell = board.Cells[nextCell.x, nextCell.y];

        this.previousCellPos = previousCell;
        this.nextCellPos = nextCell;

        this.clickedPiece = pieceMoved;
        
        //this.previousCell = previousCell;
        //this.nextCell = nextCell;
        this.newPiecePosition = newPiecePosition;
        finishedTurn = true;
    }
    
}
