using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Knight : BasePiece
{
    public override void Setup(Color newTeamCollor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        base.Setup(newTeamCollor, newSpriteColor, newPieceManager);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("T_Knight");
        name = "Knight";
    }

    private void CreateCellPath(int flipper)
    {
        int currentX = CurrentCell.boardPosition.x;
        int currentY = CurrentCell.boardPosition.y;

        //Left
        MatchesState(currentX - 2, currentY + (1 * flipper));
        
        //Upper left
        MatchesState(currentX - 1, currentY + (2 * flipper));
        
        //Upper right
        MatchesState(currentX + 1, currentY + (2 * flipper));
        
        //Right
        MatchesState(currentX + 2, currentY + (1 * flipper));
    }

    protected override void CheckPathing()
    {
        //Top half
        CreateCellPath(1);
        
        //Bottom half
        CreateCellPath(-1);
    }

    private void MatchesState(int targetX, int targetY)
    {
        CellState cellState = CellState.None;
        cellState = CurrentCell.board.ValidateCell(targetX, targetY, this);

        if (cellState != CellState.Friendly && cellState != CellState.OutOfBounds)
        {
            HighlightedCells.Add(CurrentCell.board.Cells[targetX, targetY]);
        }
    }
}
