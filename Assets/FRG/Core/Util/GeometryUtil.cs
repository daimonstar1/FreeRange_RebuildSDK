using UnityEngine;

public class GeometryUtil {
    public static float PointSegDistance( Vector3 point, Vector3 segA, Vector3 segB ) {
        Vector3 seg = segB - segA;
        float lenSq = seg.sqrMagnitude;
        Vector3 aToPt = point - segA;
        float t = Vector3.Dot( aToPt, seg ) / lenSq;
        if( t < 0 ) {
            return aToPt.magnitude;
        } else if( t > 1 ) {
            return (point - segB).magnitude;
        }
        Vector3 proj = segA + seg * t;
        Vector3 perp = point - proj;
        return perp.magnitude;
    }
}