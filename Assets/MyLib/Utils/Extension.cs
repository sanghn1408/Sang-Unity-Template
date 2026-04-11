using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;

public static class Extension
{
    public static bool TryParse<T>(this Enum theEnum, string valueToParse, out T returnValue)
    {
        returnValue = default(T);
        if (Enum.IsDefined(typeof(T), valueToParse))
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            returnValue = (T)converter.ConvertFromString(valueToParse);
            return true;
        }
        return false;
    }

    public static void DrawGizmosCircle(this Transform trans, Vector3 offset, float rangle, Color color)
    {
        Gizmos.color = color;
        int _nbSide = 100;

        for (int i = 0; i < _nbSide - 1; i++)
        {
            Vector3 pos = new Vector3(Mathf.Cos(2 * Mathf.PI * i / _nbSide) * rangle, 0f, Mathf.Sin(2 * Mathf.PI * i / _nbSide) * rangle);
            Vector3 pos1 = new Vector3(Mathf.Cos(2 * Mathf.PI * (i + 1) / _nbSide) * rangle, 0f, Mathf.Sin(2 * Mathf.PI * (i + 1) / _nbSide) * rangle);

            Gizmos.DrawLine(pos + offset, pos1 + offset);
        }
    }

    public static void DrawGizmosCircle(this Transform trans, Vector3 offset, Quaternion dir, float rangle, Color color)
    {
        Gizmos.color = color;
        int _nbSide = 100;

        for (int i = 0; i < _nbSide - 1; i++)
        {
            Vector3 pos = new Vector3(Mathf.Cos(2 * Mathf.PI * i / _nbSide) * rangle, 0f, Mathf.Sin(2 * Mathf.PI * i / _nbSide) * rangle);
            Vector3 pos1 = new Vector3(Mathf.Cos(2 * Mathf.PI * (i + 1) / _nbSide) * rangle, 0f, Mathf.Sin(2 * Mathf.PI * (i + 1) / _nbSide) * rangle);

            pos = dir * pos;
            pos1 = dir * pos1;

            pos += offset;
            pos1 += offset;

            Gizmos.DrawLine(pos, pos1);
        }
    }

    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}
