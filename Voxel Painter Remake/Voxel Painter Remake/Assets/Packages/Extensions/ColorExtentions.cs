using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtentions  {

    public static Color SetSaturation(Color color, float saturation) {
        float h;
        float s;
        float v;

        Color.RGBToHSV(color, out h, out s, out v);
        s = saturation;

        if (s > 1) {
            //Convert it to clamp
            s = saturation / 255;
        }

        Color outColor = Color.HSVToRGB(h, s, v);
        outColor.a = color.a;
        return outColor;
    }

}
