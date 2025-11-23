using System.Collections.Generic;
using UnityEngine;

public class HeartsUI : MonoBehaviour
{
    [SerializeField] List<GameObject> hearts;
    public void RemoveHeart()
    {
        Destroy(hearts[hearts.Count - 1]);
        hearts.RemoveAt(hearts.Count - 1);
    }
}
