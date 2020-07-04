using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBar : MonoBehaviour
{
    [SerializeField]
    private Image fillBar;
    void Start()
    {
        PlayerController.MomentumChangedEvent += OnChange;
    }

    private void OnChange(float fill, float max )
    {
        if (!fillBar)
            return;

        fillBar.fillAmount = fill / max;
    }
}
