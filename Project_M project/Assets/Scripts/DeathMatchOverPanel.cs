using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathMatchOverPanel : MonoBehaviour
{
    [SerializeField] Text title;

    private void OnEnable()
    {
        title.text = $"{(int)Mathf.Round( Camera.main.transform.position.y )}m!";
    }

    public void Restart()
    {
        GameManager.Respawn();
    }
}
