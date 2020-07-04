using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsCounter;
    void Update()
    {
        fpsCounter.enabled = GameManager.ShowFPS == 1;
        fpsCounter.text = $@"fps:{(int)Mathf.Round(1f / Time.unscaledDeltaTime)}";
    }
}
