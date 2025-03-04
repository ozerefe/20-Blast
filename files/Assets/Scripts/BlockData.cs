using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockData
{
    public string colorName;     // Kolay ayırt etmek için
    public int colorID;
    public Sprite defaultIcon;   // Normal durumda görünecek ikon
    public Sprite iconA;         // Grup büyüklüğü conditionA üzerindeyse
    public Sprite iconB;
    public Sprite iconC;
}

