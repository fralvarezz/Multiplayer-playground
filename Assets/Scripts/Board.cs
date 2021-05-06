using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CellState
{
    None,
    Friendly,
    Enemy,
    Free,
    OutOfBounds
}

public class Board : MonoBehaviour
{

    public GameObject cellPrefab;
    
    //two dimensional array
    public Cell[,] Cells;
    

    public void Create()
    {
        Cells = new Cell[8,8];
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                //create cell
                GameObject newCell = Instantiate(cellPrefab, transform);
                
                //reposition it
                RectTransform rectTransform = newCell.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(x*100 + 50, y*100 + 50);

                Cells[x, y] = newCell.GetComponent<Cell>();
                Color colorCellPromotes = Color.clear;
                if(y == 0)
                    colorCellPromotes = Color.black;
                else if(y == 7)
                    colorCellPromotes = Color.white;

                Cells[x, y].Setup(new Vector2Int(x, y), this, colorCellPromotes);
            }
        }

        for (int x = 0; x < 8; x += 2)
        {
            for (int y = 0; y < 8; y++)
            {
                int offset = (y % 2 != 0) ? 0 : 1;
                int finalX = x + offset;
                
                Cells[finalX, y].GetComponent<Image>().color = new Color32(230,220,187,255);
            }
        }
    }

    public CellState ValidateCell(int targetX, int targetY, BasePiece checkingPiece)
    {
        if (targetX < 0 || targetX > 7)
        {
            return CellState.OutOfBounds;
        }
        
        if (targetY < 0 || targetY > 7)
        {
            return CellState.OutOfBounds;
        }

        Cell targetCell = Cells[targetX, targetY];

        if (checkingPiece.color == Color.white)
        {
            if (targetCell.currentTurnCurrentPieceBlack != null)
                return CellState.Enemy;
            if (targetCell.currentTurnCurrentPieceWhite != null)
                return CellState.Friendly;
        }
        else if(checkingPiece.color == Color.black)
        {
            if (targetCell.currentTurnCurrentPieceWhite != null)
                return CellState.Enemy;
            if (targetCell.currentTurnCurrentPieceBlack != null)
                return CellState.Friendly;
        }

        return CellState.Free;
    }

    public void CleanBoard()
    {
        foreach (var cell in Cells)
        {
            cell.NewTurn();
        }
    }

    public void ResetBoard()
    {
        foreach (var cell in Cells)
        {
            cell.Reset();
        }
    }
}
