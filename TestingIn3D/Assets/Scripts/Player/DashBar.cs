using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashBar : MonoBehaviour
{
    public Slider slider;

    public void SetDash(float dash)
    {
        slider.value = dash;
    }

    public void SetMaxDash(float dash)
    {
        slider.maxValue = dash;
        slider.value = dash;
    }
}