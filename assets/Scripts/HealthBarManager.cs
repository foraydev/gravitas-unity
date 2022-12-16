using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{

    public Slider slider;

    public void setVal(float hp)
    {
        slider.value = hp;
    }

    public void setMaxVal(float hp)
    {
        slider.maxValue = hp;
        slider.value = hp;
    }
}
