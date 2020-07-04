using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeMeter : MonoBehaviour
{
    private TextLable lable;
    private float timer;
    void Start()
    {
        lable = GetComponent<TextLable>();
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        int minutes = (int)(timer / 60f);
        int seconds = (int)(timer % 60f);
        lable.UpdateText( $"{minutes}:{(seconds < 10 ? "0" : "")}{(int)(timer % 60f)}" );
    }
}
