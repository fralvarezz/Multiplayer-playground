using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PieceManager : MonoBehaviour
{
    [HideInInspector]public bool bothKingsAlive = true;

    public static PieceManager pieceManager; //singleton
    
    public GameObject piecePrefab;
    public Color32 topColor;
    public Color32 bottomColor;

    private List<BasePiece> _whitePieces = null;
    private List<BasePiece> _blackPieces = null;
    
    private List<BasePiece> _promotedPieces = new List<BasePiece>();

    public bool _whiteMoved;
    public bool _blackMoved;

    [HideInInspector] public string topPreLog;
    [HideInInspector] public string bottomPreLog;

    private Board _board;


    private string[] pieceOrder = new string[16]
    {
        "P", "P", "P", "P", "P", "P", "P", "P",
        "R", "KN", "B", "Q", "K", "B", "KN", "R"
    };

    private Dictionary<string, Type> pieceLibrary = new Dictionary<string, Type>()
    {
        {"P", typeof(Pawn)},
        {"R", typeof(Rook)},
        {"KN", typeof(Knight)},
        {"B", typeof(Bishop)},
        {"K", typeof(King)},
        {"Q", typeof(Queen)},
    };

    private void Awake()
    {
        if(pieceManager != null)
                GameObject.Destroy(pieceManager);
        else
            pieceManager = this;
		
        DontDestroyOnLoad(this);
    }

    public void Setup(Board board)
    {
        _whitePieces = CreatePieces(Color.white, bottomColor, board);
        _blackPieces = CreatePieces(Color.black, topColor, board);

        //GameManager.gm.bottomPlayer.coinZone.color = bottomColor;
        //GameManager.gm.topPlayer.coinZone.color = topColor;

        PlacePieces(1, 0, _whitePieces, board);
        PlacePieces(6, 7, _blackPieces, board);

        _board = board;
        NewTurn();
        //GameManager.gm.Log("--Game started--");
    }

    private List<BasePiece> CreatePieces(Color teamColor, Color32 spriteColor, Board board)
    {
        List<BasePiece> newPieces = new List<BasePiece>();

        for (int i = 0; i < pieceOrder.Length; i++)
        {
            GameObject newPieceObject = Instantiate(piecePrefab);
            newPieceObject.transform.SetParent(transform);
            
            newPieceObject.transform.localScale = Vector3.one;
            newPieceObject.transform.localRotation = Quaternion.identity;

            string key = pieceOrder[i];
            Type pieceType = pieceLibrary[key];

            BasePiece newPiece = (BasePiece) newPieceObject.AddComponent(pieceType);
            newPieces.Add(newPiece);
            
            newPiece.Setup(teamColor, spriteColor, this);
        }

        return newPieces;
    }

    private BasePiece CreatePiece(Type pieceType)
    {
        GameObject newPieceObject = Instantiate(piecePrefab);
        newPieceObject.transform.SetParent(transform);
            
        newPieceObject.transform.localScale = Vector3.one;
        newPieceObject.transform.localRotation = Quaternion.identity;

        BasePiece newPiece = (BasePiece) newPieceObject.AddComponent(pieceType);
        return newPiece;
    }

    private void PlacePieces(int pawnRow, int royaltyRow, List<BasePiece> pieces, Board board)
    {
        for (int i = 0; i < 8; i++)
        {
            pieces[i].Place(board.Cells[i, pawnRow]);
            pieces[i+8].Place(board.Cells[i, royaltyRow]);
        }
    }

    private void SetInteractive(List<BasePiece> allPieces, bool value)
    {
        foreach (BasePiece piece in allPieces)
        {
            piece.enabled = value;
        }
    }

    public void SwitchSides(Color color)
    {
        if (!bothKingsAlive)
        {
            ResetPieces();

            bothKingsAlive = true;
            
            color = Color.black;
        }

        bool isBlackTurn = (color == Color.white);
        
        SetInteractive(_whitePieces, false);
        SetInteractive(_blackPieces, false);
    }

    public void NewTurn()
    {
        _board.CleanBoard();
        _whiteMoved = false;
        _blackMoved = false;
        SetInteractive(_whitePieces, true);
        SetInteractive(_blackPieces, true);
        
        //GameManager.gm.bottomPlayer.readyLight.color = Color.red;
        //GameManager.gm.topPlayer.readyLight.color = Color.red;
        //GameManager.gm.Log(topPreLog);
        //GameManager.gm.Log(bottomPreLog);
        topPreLog = "";
        bottomPreLog = "";
        
        if (!bothKingsAlive)
        {
            foreach (BasePiece promotedPiece in _promotedPieces)
            {
                Color promotedPieceColor = promotedPiece.color;
                Destroy(promotedPiece.gameObject);

                if(promotedPieceColor == Color.white)
                    _whitePieces.Remove(promotedPiece);
                else
                    _blackPieces.Remove(promotedPiece);
            }
            _promotedPieces.Clear();
            
            _board.ResetBoard();

            ResetPieces();
            
            bothKingsAlive = true;
            _blackMoved = false;
            _whiteMoved = false;
            NewTurn();
        }
    }

    public void ResetPieces()
    {
        foreach (BasePiece piece in _whitePieces)
        {
            piece.Reset();
        }
        
        foreach (BasePiece piece in _blackPieces)
        {
            piece.Reset();
        }
    }

    public void ColorMoved(Color colorMoved)
    {
        if (colorMoved == Color.white)
        {
            _whiteMoved = true;
            SetInteractive(_whitePieces, false);
            //GameManager.gm.bottomPlayer.readyLight.color = Color.green;
        }
        else if(colorMoved == Color.black)
        {
            _blackMoved = true;
            SetInteractive(_blackPieces, false);
            //GameManager.gm.topPlayer.readyLight.color = Color.green;
        }

        if (_whiteMoved && _blackMoved)
        {
            NewTurn();
        }
    }

    public void CreatePromotedPiece(Cell cell, BasePiece pieceToReplace, Type type)
    {
        var newPiece = CreatePiece(type);
        
        newPiece.Setup(pieceToReplace.color, pieceToReplace.GetComponent<Image>().color, this);
        newPiece.Place(cell);

        if (pieceToReplace.color == Color.white)
        {
            _whitePieces.Add(newPiece);
        }
        else
        {
            _blackPieces.Add(newPiece);
        }
        _promotedPieces.Add(newPiece);
        
        pieceToReplace.Kill();
    }
}
