using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMeter : MonoBehaviour
{
    public Transform objectToMeasure;
    private TextLable lable;
    void Start()
    {
       lable = GetComponent<TextLable>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!objectToMeasure)
            return;
        lable.UpdateText( $"{Mathf.FloorToInt(objectToMeasure.position.y)}" );
    }
}
