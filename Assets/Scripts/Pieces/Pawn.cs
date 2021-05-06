using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pawn : BasePiece
{
    public bool isFirstMove = true;

    public override void Setup(Color newTeamCollor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        base.Setup(newTeamCollor, newSpriteColor, newPieceManager);

        isFirstMove = true;
        
        Movement = color == Color.white ? new Vector3Int(0,1,1) : new Vector3Int(0, -1, -1);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("T_Pawn");
        name = "Pawn";
    }

    protected override void MoveOnline()
    {
        base.MoveOnline();

        isFirstMove = false;
    }

    private bool MatchesState(int targetX, int targetY, CellState targetState)
    {
        CellState cellState = CellState.None;
        cellState = CurrentCell.board.ValidateCell(targetX, targetY, this);

        if (cellState == targetState)
        {
            HighlightedCells.Add(CurrentCell.board.Cells[targetX, targetY]);
            return true;
        }

        return false;
    }

    protected override void CheckPathing()
    {
        //Target position
        int currentX = CurrentCell.boardPosition.x;
        int currentY = CurrentCell.boardPosition.y;
        
        //Top left
        MatchesState(currentX - Movement.z, currentY + Movement.z, CellState.Enemy);

        //Forward
        if (MatchesState(currentX, currentY + Movement.y, CellState.Free))
        {
            if (isFirstMove)
            {
                MatchesState(currentX, currentY + Movement.y * 2, CellState.Free);
            }
        }
        
        //Top right
        MatchesState(currentX + Movement.z, currentY + Movement.z, CellState.Enemy);
    }

    public override void Place(Cell newCell)
    {
        base.Place(newCell);
        isFirstMove = true;
    }
}
