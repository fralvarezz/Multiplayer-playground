using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePiece : EventTrigger
{
    [HideInInspector] public Color color = Color.clear;

    protected Cell OriginalCell = null;
    protected Cell CurrentCell = null;

    protected RectTransform RectTransform = null;
    protected PieceManager PieceManager;

    protected Vector3Int Movement = Vector3Int.one;
    protected List<Cell> HighlightedCells = new List<Cell>();

    protected Cell TargetCell = null;
    
    public virtual void Setup(Color newTeamCollor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        PieceManager = newPieceManager;

        color = newTeamCollor;
        GetComponent<Image>().color = newSpriteColor;
        RectTransform = GetComponent<RectTransform>();
    }

    public virtual void Place(Cell newCell)
    {
        gameObject.SetActive(true);
        CurrentCell = newCell;
        OriginalCell = newCell;
        TargetCell = null;
        
        CurrentCell.AddPiece(this, this.color);
        //CurrentCell.currentPiece = this;

        transform.position = newCell.transform.position;
    }

    private void CreateCellPath(int xDirection, int yDirection, int movement)
    {
        int currentX = CurrentCell.boardPosition.x;
        int currentY = CurrentCell.boardPosition.y;

        for (int i = 1; i <= movement; i++)
        {
            currentX += xDirection;
            currentY += yDirection;
            
            CellState cellState = CellState.None;
            cellState = CurrentCell.board.ValidateCell(currentX, currentY, this);

            if (cellState == CellState.Enemy)
            {
                HighlightedCells.Add(CurrentCell.board.Cells[currentX, currentY]);
                break;
            }

            if (cellState != CellState.Free)
                break;
            
            HighlightedCells.Add(CurrentCell.board.Cells[currentX, currentY]);
        }
    }

    protected virtual void CheckPathing()
    {
        //Horizontal
        CreateCellPath(1, 0, Movement.x);
        CreateCellPath(-1, 0, Movement.x);
        
        //Vertical
        CreateCellPath(0, 1, Movement.y);
        CreateCellPath(0, -1, Movement.y);
        
        //Upper diagonal
        CreateCellPath(1, 1, Movement.z);
        CreateCellPath(-1, 1, Movement.z);
        
        //Lower diagonal
        CreateCellPath(-1, -1, Movement.z);
        CreateCellPath(1, -1, Movement.z);
    }

    protected void ShowCells()
    {
        foreach (Cell cell in HighlightedCells)
        {
            cell.outline.enabled = true;
        }
    }

    protected void ClearCells()
    {
        foreach (Cell cell in HighlightedCells)
        {
            cell.outline.enabled = false;
        }
        HighlightedCells.Clear();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        PlayerChess playerChess = NetworkClient.localPlayer.GetComponent<PlayerChess>();
        if (!(color == Color.white && playerChess.team == PlayerTeam.WHITE  || color == Color.black && playerChess.team == PlayerTeam.BLACK))
        {
            return;
        }
        
        base.OnBeginDrag(eventData);
        
        CheckPathing();
        
        ShowCells();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        PlayerChess playerChess = NetworkClient.localPlayer.GetComponent<PlayerChess>();
        if (!(color == Color.white && playerChess.team == PlayerTeam.WHITE  || color == Color.black && playerChess.team == PlayerTeam.BLACK))
        {
            return;
        }
        
        //Follow pointer
        transform.position += (Vector3)eventData.delta;

        foreach (Cell cell in HighlightedCells)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(cell.rectTrans, Input.mousePosition))
            {
                TargetCell = cell;
                break;
            }

            TargetCell = null;
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        
        PlayerChess playerChess = NetworkClient.localPlayer.GetComponent<PlayerChess>();
        if (!(color == Color.white && playerChess.team == PlayerTeam.WHITE  || color == Color.black && playerChess.team == PlayerTeam.BLACK))
        {
            return;
        }
        
        ClearCells();

        if (!TargetCell)
        {
            transform.position = CurrentCell.gameObject.transform.position;
            return;
        }

        if (color == Color.black)
        {
            PieceManager.topPreLog = "Top player moved " + name + " from " + CurrentCell.GetPositionName() + " to " + TargetCell.GetPositionName();
        }
        else if (color == Color.white)
        {
            PieceManager.bottomPreLog = "Bottom player moved " + name + " from " + CurrentCell.GetPositionName() + " to " + TargetCell.GetPositionName();
        }
        //Move();
        MoveOnline();
        //PieceManager.SwitchSides(color);
        PieceManager.ColorMoved(color);
    }

    public void Reset()
    {
        Kill();
        
        Place(OriginalCell);
    }

    public virtual void Kill()
    {
        Debug.Log("Calling kill");
        if (color == Color.white)
            CurrentCell.currentPieceWhite = null;
        else if (color == Color.black)
            CurrentCell.currentPieceBlack = null;
        
        gameObject.SetActive(false);
    }

    protected virtual void Move()
    {
        //If there is an enemy piece, remove it
        //TargetCell.RemovePiece();

        //Clear current
        CurrentCell.MoveOutPiece(color);
        
        //Switch cells
        CurrentCell = TargetCell;
        CurrentCell.AddPiece(this, color);

        transform.position = CurrentCell.transform.position;
        TargetCell = null;
    }
    
    protected virtual void MoveOnline()
    {
        //If there is an enemy piece, remove it
        //TargetCell.RemovePiece();

        Cell currentCellSend;
        Cell nextCellSend;
        
        //Clear current
        CurrentCell.MoveOutPiece(color);
        currentCellSend = CurrentCell;
        
        //currentCellSend.
        
        
        //Switch cells
        CurrentCell = TargetCell;
        CurrentCell.AddPiece(this, color);
        nextCellSend = CurrentCell;
        
        //tell player previous and new cell
        //tell player new position for current piece (maybe piece too)
        PlayerChess playerChess = NetworkClient.localPlayer.GetComponent<PlayerChess>();
        if (color == Color.white && playerChess.team == PlayerTeam.WHITE  || color == Color.black && playerChess.team == PlayerTeam.BLACK)
        {
            playerChess.CmdFinishTurn(currentCellSend.boardPosition, nextCellSend.boardPosition, this.gameObject, CurrentCell.transform.position);
        }

        transform.position = CurrentCell.transform.position;
        TargetCell = null;
    }
}
