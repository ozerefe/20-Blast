using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [HideInInspector] public int colorID;
    [HideInInspector] public int row;
    [HideInInspector] public int column;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Blok başlatma metodu
    public void InitBlock(int cID, Sprite icon, int r, int c)
    {
        colorID = cID;
        row = r;
        column = c;
        spriteRenderer.sprite = icon;
    }

    // Blok ikonunu güncellemek istediğimizde kullanacağımız metot
    public void UpdateIcon(Sprite newIcon)
    {
        if (newIcon != null)
        {
            spriteRenderer.sprite = newIcon;
        }
    }
}
