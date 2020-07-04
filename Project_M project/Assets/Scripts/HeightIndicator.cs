using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeightIndicator : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex <= 3)
        {
            gameObject.SetActive( false );
            return;
        }
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        textMesh.text = $"{(int)Mathf.Round(Camera.main.transform.position.y)}m";
    }
}
