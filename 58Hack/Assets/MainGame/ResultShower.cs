using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultShower : MonoBehaviour
{
    [SerializeField] RawImage rawImage;
    [SerializeField] TextMeshProUGUI tmp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tmp.text = $"{MainGameManager.lastFaceCount} faces!";
        rawImage.texture = MainGameManager.targetTexture;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ReturnToTitle()
    {
        SceneManager.LoadScene("TakePicture");
    }
}
