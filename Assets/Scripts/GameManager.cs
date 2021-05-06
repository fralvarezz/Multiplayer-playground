using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager gm; //This is a globally accessible singleton

    private bool _whiteHasMoved;
    private bool _blackHasMoved;

    public GameObject whitePlayer;
    public GameObject blackPlayer;
    
    private Board board;
    public GameObject boardHolder;
    public GameObject boardPrefab;

    public GameObject pieceManagerPrefab;
    public PieceManager pieceManager;

    
    //Create singleton
    void Awake()
    {
        if(gm != null)
            GameObject.Destroy(gm);
        else
            gm = this;
		
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        boardHolder = GameObject.FindWithTag("ParentCanvas");
        
        MakeBoard();
        pieceManager.Setup(board);
    }
    
    public void MakeBoard()
    {
        pieceManager = boardHolder.GetComponentInChildren<PieceManager>();
        
        if (board)
        {
            DestroyImmediate(board.gameObject);
        }
        //Make new board
        GameObject gameBoard = Instantiate(boardPrefab, new Vector3(0, 0, 0), Quaternion.identity, boardHolder.transform);
        board = gameBoard.GetComponent<Board>();
        //Position the board
        RectTransform rectTransform = board.GetComponent<RectTransform>();
        rectTransform.SetParent( boardHolder.transform );
        Vector2 childAnchor = new Vector2( 0.5f, 0.5f );
        rectTransform.anchorMax = childAnchor;
        rectTransform.anchorMin = childAnchor;
        //Call Create
        board.Create();
        board.transform.SetAsFirstSibling();
    }

    private void FixedUpdate()
    {
        if (isServer)
        {
            CheckFinishedMoves();
            if(_whiteHasMoved && _blackHasMoved)
                MakeMovesServer();
        }
    }
    

    [Server]
    private void CheckFinishedMoves()
    {
        foreach (var connection in NetworkServer.connections)
        {
            PlayerChess playerChess = connection.Value.identity.GetComponent<PlayerChess>();

            if (playerChess.finishedTurn)
            {
                if (playerChess.team == PlayerTeam.WHITE)
                    _whiteHasMoved = true;
                else
                    _blackHasMoved = true;
            }
        }
        //Debug.Log(_whiteHasMoved);
        //Debug.Log(_blackHasMoved);
    }

    [Server]
    private void MakeMovesServer()
    {
        Debug.Log("Makemovesserver is being called");

        Vector2Int prevCellWhiteVec = Vector2Int.zero;
        Vector2Int nextCellWhiteVec = Vector2Int.zero;
        Vector2Int prevCellBlackVec = Vector2Int.zero;
        Vector2Int nextCellBlackVec = Vector2Int.zero;

        if (_blackHasMoved && _whiteHasMoved)
        {
            //Maybe do it on server and then tell players
            foreach (var connection in NetworkServer.connections)
            {
                PlayerChess playerChess = connection.Value.identity.GetComponent<PlayerChess>();
                playerChess.finishedTurn = false;
                if (playerChess.team == PlayerTeam.WHITE)
                {
                    prevCellWhiteVec = playerChess.previousCellPos;
                    nextCellWhiteVec = playerChess.nextCellPos;
                }
                else
                {
                    prevCellBlackVec = playerChess.previousCellPos;
                    nextCellBlackVec = playerChess.nextCellPos;
                }
            }
        }
        
        MakeMoves(prevCellWhiteVec, nextCellWhiteVec, prevCellBlackVec, nextCellBlackVec);

    }
    
    [ClientRpc]
    private void MakeMoves(Vector2Int prevCellWhiteVec, Vector2Int nextCellWhiteVec, Vector2Int prevCellBlackVec, Vector2Int nextCellBlackVec)
    {
        Debug.Log("Makemoves is being called");
        
        Cell prevCellWhite = null;
        Cell nextCellWhite = null;
        Cell prevCellBlack = null;
        Cell nextCellBlack = null;

        
        //WHITE MOVES
        prevCellWhite = board.Cells[prevCellWhiteVec.x, prevCellWhiteVec.y];
        nextCellWhite = board.Cells[nextCellWhiteVec.x, nextCellWhiteVec.y];
        BasePiece white = prevCellWhite.currentPieceWhite;

        if (!white)
        {
            white = nextCellWhite.currentPieceWhite;
        }

        prevCellWhite.MoveOutPiece(Color.white);
        nextCellWhite.AddPiece(white, Color.white);

        if(white)
            white.transform.position = nextCellWhite.transform.position;
        
        //BLACK MOVES
        prevCellBlack = board.Cells[prevCellBlackVec.x, prevCellBlackVec.y];
        nextCellBlack = board.Cells[nextCellBlackVec.x, nextCellBlackVec.y];

        BasePiece black = prevCellBlack.currentPieceBlack;
        if (!black)
        {
            black = nextCellBlack.currentPieceBlack;
        }

        prevCellBlack.MoveOutPiece(Color.black);
        nextCellBlack.AddPiece(black, Color.black);

        if(black)
            black.transform.position = nextCellBlack.transform.position;
        
        
        //board.Cells[playerChess.previousCellPos.x, playerChess.previousCellPos.y]
        Debug.Log("About to go newturn");
        pieceManager.NewTurn();
        
        Debug.Log("Both players moved, resetting");
        _blackHasMoved = false;
        _whiteHasMoved = false;

    }

}
