using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class King : BasePiece
{
    public override void Setup(Color newTeamCollor, Color32 newSpriteColor, PieceManager newPieceManager)
    {
        base.Setup(newTeamCollor, newSpriteColor, newPieceManager);

        Movement = new Vector3Int(1,1,1);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("T_King");
        name = "King";
    }

    public override void Kill()
    {
        base.Kill();

        PieceManager.bothKingsAlive = false;
    }
}
