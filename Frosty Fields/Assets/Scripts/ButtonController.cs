using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{

    public Sprite unSelectedSprite;
    public Sprite selectedSprite;
    private bool selected;
    // Start is called before the first frame update

    void Start()
    {
        selected = false;
    }

    public void SetSelected(bool selected)
    {
        if (selected == this.selected) return;
        this.selected = selected;
        if (selected)
        {
            gameObject.GetComponent<Button>().GetComponent<Image>().sprite = selectedSprite;
        } else
        {
            gameObject.GetComponent<Button>().GetComponent<Image>().sprite = unSelectedSprite;
        }
    }
}
