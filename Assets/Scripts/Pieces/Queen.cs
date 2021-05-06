using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Queen : BasePiece
{
    public override void Setup(Color newTeamCollor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        base.Setup(newTeamCollor, newSpriteColor, newPieceManager);
        
        Movement = new Vector3Int(7,7,7);
        
        GetComponent<Image>().sprite = Resources.Load<Sprite>("T_Queen");
        name = "Queen";
    }
}
