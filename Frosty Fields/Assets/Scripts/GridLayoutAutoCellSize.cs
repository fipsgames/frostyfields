using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridLayoutAutoCellSize : MonoBehaviour
{
    GridLayoutGroup glg;
    // Start is called before the first frame update
    void Start()
    {
        glg = gameObject.GetComponent<GridLayoutGroup>();
        float w = gameObject.GetComponent<RectTransform>().rect.width;
        
        ////Logger.Log("WIDTH = " + w , this, true);
        float cellSize = ((w - glg.padding.right - glg.padding.left) - ((glg.constraintCount-1) * glg.spacing.x)) / glg.constraintCount;
        glg.cellSize = new Vector2(cellSize,cellSize);
        ////Logger.Log("Set CEll size = " + cellSize , this, true);
    }

}
