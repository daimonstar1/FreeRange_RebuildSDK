using System;
using UnityEngine;

[Serializable]
public struct RectTransformSettings {
	public Vector2 anchoredPosition;
	public Vector2 anchorMax;
	public Vector2 anchorMin;
	public Vector2 pivot;
	public Vector2 sizeDelta;
	public Vector3 localScale;
	public Vector3 localEulerAngles;

    public RectTransformSettings(RectTransformSettings source) {
        anchoredPosition = source.anchoredPosition;
        anchorMax = source.anchorMax;
        anchorMin = source.anchorMin;
        pivot = source.pivot;
        sizeDelta = source.sizeDelta;
        localScale = source.localScale;
        localEulerAngles = source.localEulerAngles;
    }

    public RectTransformSettings(RectTransform rTrans) {
        anchoredPosition = rTrans.anchoredPosition;
        anchorMax = rTrans.anchorMax;
        anchorMin = rTrans.anchorMin;
        pivot = rTrans.pivot;
        sizeDelta = rTrans.sizeDelta;
        localScale = rTrans.localScale;
        localEulerAngles = rTrans.localEulerAngles;
	}

    public static RectTransformSettings Lerp(RectTransformSettings start, RectTransformSettings end, float t, bool ignoreX=false, bool ignoreY=false, bool ignoreScale=false) {
        RectTransformSettings retVal = new RectTransformSettings();

        Vector2 anchoredPosStart = Vector2.zero;
        Vector2 anchoredPosEnd = Vector2.zero;

        Vector2 anchoredMaxStart = Vector2.zero;
        Vector2 anchoredMaxEnd = Vector2.zero;

        Vector2 anchoredMinStart = Vector2.zero;
        Vector2 anchoredMinEnd = Vector2.zero;

        Vector2 pivotStart = Vector2.zero;
        Vector2 pivotEnd = Vector2.zero;

        Vector2 sizeDeltaStart = Vector2.zero;
        Vector2 sizeDeltaEnd = Vector2.zero;

        Vector3 localScaleStart = Vector3.zero;
        Vector3 localScaleEnd = Vector3.zero;

        anchoredPosStart = start.anchoredPosition;
        anchoredMaxStart = start.anchorMax;
        anchoredMinStart = start.anchorMin;
        pivotStart = start.pivot;
        sizeDeltaStart = start.sizeDelta;
        localScaleStart = start.localScale;

        if(ignoreX && !ignoreY) {
            anchoredPosEnd = new Vector2(start.anchoredPosition.x, end.anchoredPosition.y);
            anchoredMaxEnd = new Vector2(start.anchorMax.x, end.anchorMax.y);
            anchoredMinEnd = new Vector2(start.anchorMin.x, end.anchorMin.y);
            pivotEnd = new Vector2(start.pivot.x, end.pivot.y);
            sizeDeltaEnd = new Vector2(start.sizeDelta.x, end.sizeDelta.y);
            localScaleEnd = new Vector3(start.localScale.x, end.localScale.y);
        }
        else if(ignoreY && !ignoreX) {
            anchoredPosEnd = new Vector2(end.anchoredPosition.x, start.anchoredPosition.y);
            anchoredMaxEnd = new Vector2(end.anchorMax.x, start.anchorMax.y);
            anchoredMinEnd = new Vector2(end.anchorMin.x, start.anchorMin.y);
            pivotEnd = new Vector2(end.pivot.x, start.pivot.y);
            sizeDeltaEnd = new Vector2(end.sizeDelta.x, start.sizeDelta.y);
            localScaleEnd = new Vector3(end.localScale.x, start.localScale.y);
        }
        else if(!ignoreY && !ignoreX) {
            anchoredPosEnd = end.anchoredPosition;
            anchoredMaxEnd = end.anchorMax;
            anchoredMinEnd = end.anchorMin;
            pivotEnd = end.pivot;
            sizeDeltaEnd = end.sizeDelta;
            localScaleEnd = end.localScale;
        }

		retVal.anchoredPosition = Vector2.Lerp(anchoredPosStart, anchoredPosEnd, t);
		retVal.anchorMax = Vector2.Lerp(anchoredMaxStart, anchoredMaxEnd, t);
		retVal.anchorMin = Vector2.Lerp(anchoredMinStart,anchoredMinEnd, t);
		retVal.pivot = Vector2.Lerp(pivotStart, pivotEnd, t);
		retVal.sizeDelta = Vector2.Lerp(sizeDeltaStart, sizeDeltaEnd, t);
		if(!ignoreScale) retVal.localScale = Vector3.Lerp(localScaleStart, localScaleEnd, t);

		retVal.localEulerAngles.x = Mathf.LerpAngle(start.localEulerAngles.x, end.localEulerAngles.x, t);
		retVal.localEulerAngles.y = Mathf.LerpAngle(start.localEulerAngles.y, end.localEulerAngles.y, t);
		retVal.localEulerAngles.z = Mathf.LerpAngle(start.localEulerAngles.z, end.localEulerAngles.z, t);

        return retVal;
    }

	public static void LoadSettings(RectTransform rTrans, RectTransformSettings settings, bool ignoreX=false, bool ignoreY=false, bool ignoreScale=false) {
		if(!SettingsAreDifferent(rTrans, settings, ignoreX, ignoreY)) return;

        if(ignoreX && !ignoreY) {
		    rTrans.anchorMax = new Vector2(rTrans.anchorMax.x, settings.anchorMax.y);
		    rTrans.anchorMin = new Vector2(rTrans.anchorMin.x, settings.anchorMin.y);
		    rTrans.pivot = new Vector2(rTrans.pivot.x, settings.pivot.y);
		    rTrans.sizeDelta = new Vector2(rTrans.sizeDelta.x, settings.sizeDelta.y);
		    if(!ignoreScale) rTrans.localScale = new Vector3(rTrans.localScale.x, settings.localScale.y, settings.localScale.z);
		    rTrans.anchoredPosition = new Vector2(rTrans.anchoredPosition.x, settings.anchoredPosition.y);
        }
        else if(ignoreY && !ignoreX) {
		    rTrans.anchorMax = new Vector2(settings.anchorMax.x, rTrans.anchorMax.y);
		    rTrans.anchorMin = new Vector2(settings.anchorMin.x, rTrans.anchorMin.y);
		    rTrans.pivot = new Vector2(settings.pivot.x, rTrans.pivot.y);
		    rTrans.sizeDelta = new Vector2(settings.sizeDelta.x, rTrans.sizeDelta.y);
		    if(!ignoreScale) rTrans.localScale = new Vector3(settings.localScale.x, rTrans.localScale.y, settings.localScale.z);
		    rTrans.anchoredPosition = new Vector2(settings.anchoredPosition.x, rTrans.anchoredPosition.y);
        }
        else if(!ignoreY && !ignoreX) {
		    rTrans.anchorMax = settings.anchorMax;
		    rTrans.anchorMin = settings.anchorMin;
		    rTrans.pivot = settings.pivot;
		    rTrans.sizeDelta = settings.sizeDelta;
		    if(!ignoreScale) rTrans.localScale = settings.localScale;
		    rTrans.anchoredPosition = settings.anchoredPosition;
        }

        rTrans.localEulerAngles = settings.localEulerAngles;
    }

	public static bool SettingsAreDifferent(RectTransform rTrans, RectTransformSettings settings, bool ignoreX=false, bool ignoreY=false, bool ignoreScale=false) {

        if(ignoreX && !ignoreY) {
            if(rTrans.anchoredPosition.y != settings.anchoredPosition.y) 	return true;
            if(rTrans.anchorMax.y != settings.anchorMax.y) 					return true;
            if(rTrans.anchorMin.y != settings.anchorMin.y) 					return true;
            if(rTrans.pivot.y != settings.pivot.y) 							return true;
            if(rTrans.sizeDelta.y != settings.sizeDelta.y) 					return true;
            if(!ignoreScale && rTrans.localScale.y != settings.localScale.y)    return true;
            if(!ignoreScale && rTrans.localScale.z != settings.localScale.z) 	return true;
        }
        else if(ignoreY && !ignoreX) {
            if(rTrans.anchoredPosition.x != settings.anchoredPosition.x) 	return true;
            if(rTrans.anchorMax.x != settings.anchorMax.x) 					return true;
            if(rTrans.anchorMin.x != settings.anchorMin.x) 					return true;
            if(rTrans.pivot.x != settings.pivot.x) 							return true;
            if(rTrans.sizeDelta.x != settings.sizeDelta.x) 					return true;
            if(!ignoreScale && rTrans.localScale.x != settings.localScale.x)    return true;
            if(!ignoreScale && rTrans.localScale.z != settings.localScale.z)    return true;
        }
        else if(!ignoreY && !ignoreX) {
		    if(rTrans.anchoredPosition != settings.anchoredPosition) 		return true;
		    if(rTrans.anchorMax != settings.anchorMax) 						return true;
		    if(rTrans.anchorMin != settings.anchorMin) 						return true;
		    if(rTrans.pivot != settings.pivot) 								return true;
		    if(rTrans.sizeDelta != settings.sizeDelta) 						return true;
            if(!ignoreScale && rTrans.localScale != settings.localScale) 	return true;
        }

        if(rTrans.localEulerAngles != settings.localEulerAngles) 	        return true;

		return false;
	}


}