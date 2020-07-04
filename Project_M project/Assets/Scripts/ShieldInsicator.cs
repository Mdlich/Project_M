using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShieldInsicator : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textField;
    void Start()
    {
        PlayerController.ShieldCountChangedEvent += UpdateLable;
    }

    private void UpdateLable(int value )
    {
        textField.text = value.ToString();
    }
}
