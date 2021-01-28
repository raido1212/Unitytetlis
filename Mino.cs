using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mino
{
    // default private
    public enum MinoType
    {
        Empty,
        Wall,
        I,
        J,
        L,
        S,
        Z,
        O,
        T,
        EnumMax
    }

    // { get; private set;}
    // プロパティのときは_もいらんしxでOK

    public Vector3[] relativePos;

    public Color32 minoColor;

    public int minoRotateMax;

    public Mino(int rotate,Color32 color,int rx1,int ry1,int rx2, int ry2, int rx3, int ry3)
    {
        minoRotateMax = rotate;
        minoColor = color;

        relativePos = new Vector3[3];
        relativePos[0].x = rx1;
        relativePos[0].y = ry1;
        relativePos[0].z = 0;

        relativePos[1].x = rx2;
        relativePos[1].y = ry2;
        relativePos[1].z = 0;

        relativePos[2].x = rx3;
        relativePos[2].y = ry3;
        relativePos[2].z = 0;
    }
}
