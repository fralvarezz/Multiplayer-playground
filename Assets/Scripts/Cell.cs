using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class Cell : NetworkBehaviour
{
    public Image outline;
    
    public Vector2Int boardPosition = Vector2Int.zero;
    public Board board = null;
    public RectTransform rectTrans = null;
    
    public BasePiece currentTurnCurrentPieceWhite;
    public BasePiece currentTurnCurrentPieceBlack;
    public BasePiece currentPieceWhite;
    public BasePiece currentPieceBlack;
    private bool _whiteJustLanded;
    private bool _blackJustLanded;

    private Color _colorCellPromotes;
    
    public void Setup(Vector2Int newBoardPosition, Board newBoard, Color colorPromoted)
    {
        boardPosition = newBoardPosition;
        board = newBoard;

        rectTrans = GetComponent<RectTransform>();
        _colorCellPromotes = colorPromoted;
    }
    

    /*
     * Both pieces removed if they stay in the same cell
     */
    public void RemovePieces()
    {
        RemovePiece(Color.white);
        RemovePiece(Color.black);
    }

    public void AddPiece(BasePiece piece, Color color)
    {
        if (color == Color.white)
        {
            currentPieceWhite = piece;
            //currentTurnCurrentPieceWhite = piece;
            _whiteJustLanded = true;
        }

        if (color == Color.black)
        {
            currentPieceBlack = piece;
            //currentTurnCurrentPieceBlack = piece;
            _blackJustLanded = true;
        }
    }

    public void MoveOutPiece(Color color)
    {
        if (color == Color.white)
            currentPieceWhite = null;

        if (color == Color.black)
            currentPieceBlack = null;
    }
    
    public void RemovePiece(Color color)
    {
        if (color == Color.white && currentPieceWhite != null)
        {
            currentPieceWhite.Kill();
            currentTurnCurrentPieceWhite = null;
            currentPieceWhite = null;
        }

        if (color == Color.black && currentPieceBlack != null)
        {
            currentPieceBlack.Kill();
            currentTurnCurrentPieceBlack = null;
            currentPieceBlack = null;
        }
    }

    public void NewTurn()
    {
        ResolveCollision();
        ResolvePromotion();
        currentTurnCurrentPieceWhite = currentPieceWhite;
        currentTurnCurrentPieceBlack = currentPieceBlack;
        _whiteJustLanded = false;
        _blackJustLanded = false;
    }

    private void ResolveCollision()
    {
        if (currentPieceWhite != null && currentPieceBlack != null)
        {
            if(_whiteJustLanded && _blackJustLanded)
                RemovePieces();
            else if(_whiteJustLanded)
                RemovePiece(Color.black);
            else if(_blackJustLanded)
                RemovePiece(Color.white);
        }
    }

    private void ResolvePromotion()
    {
        if (currentPieceWhite != null && currentPieceWhite is Pawn && _colorCellPromotes == Color.white)
        {
            //create and place queen
            PieceManager.pieceManager.CreatePromotedPiece(this, this.currentPieceWhite, typeof(Queen));
        }
        else if (currentPieceBlack != null && currentPieceBlack is Pawn && _colorCellPromotes == Color.black)
        {
            PieceManager.pieceManager.CreatePromotedPiece(this, this.currentPieceBlack, typeof(Queen));
        }
    }

    public string GetPositionName()
    {
        string[] letters = new string[8]{"A", "B", "C", "D", "E", "F", "G", "H"};
        return letters[boardPosition.x] + (boardPosition.y + 1);
    }

    public void Reset()
    {
        currentPieceBlack = null;
        currentPieceWhite = null;
        currentTurnCurrentPieceBlack = null;
        currentTurnCurrentPieceWhite = null;
        _blackJustLanded = false;
        _whiteJustLanded = false;

    }
}
