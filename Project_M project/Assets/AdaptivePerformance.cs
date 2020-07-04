using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class AdaptivePerformance : MonoBehaviour
{
    [SerializeField] private Volume ppVolume;

    private UniversalAdditionalCameraData cameraData;
    private int count = 0;
    private bool adapted;
    private void Awake()
    {
        cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
        PPEnable( GameManager.PPActive == 1 );
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded( Scene arg0, LoadSceneMode arg1 )
    {
        cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
        PPEnable( GameManager.PPActive == 1 );
    }

    void Update()
    {
        bool ppEnabled = GameManager.PPActive == 1;
        if (ppEnabled != ppVolume.enabled)
            PPEnable( ppEnabled );

        if (adapted)
            return;
        if (Time.deltaTime > 0.034)
        {
            count++;
            if (count >= 10)
            {
                adapted = true;
                PPEnable( false );
                SetQuality( 0 );
                GameManager.PPActive = 0;
            }
        }
        else
        {
            count--;
            count = Mathf.Max( 0, count );
        }
    }

    private void PPEnable(bool enable )
    {
        ppVolume.enabled = enable;
        cameraData.renderPostProcessing = enable;
        GameManager.PPActive = enable ? 1 : 0;
    }

    private void SetQuality(int index )
    {
        GameManager.Quality = index;
    }
}
