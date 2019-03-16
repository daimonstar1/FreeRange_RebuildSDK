
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FRG.Core {

    public static class Util {
        // private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

        /// <summary>
        /// AssetManagerRef Get extension
        /// </summary>
        public static T As<T>(this AssetManagerRef ref_) where T : UnityEngine.Object {
            if(!ref_.IsValid) return default(T);
            return AssetManager.TryGet<T>(ref_);
        }

        public static IEnumerable<string> GetLocalizedStrings(this IEnumerable<LocalizedStringEntry> entries_) {
            var itor = entries_.GetEnumerator();
            while(itor.MoveNext()) yield return itor.Current.ToLocalizedString();
        }

        public static void SetQualityLevel(string name) {
            for(int i = 0;i < QualitySettings.names.Length;i++) {
                if(string.Equals(QualitySettings.names[i], name)) {
                    QualitySettings.SetQualityLevel(i, true);
                    return;
                }
            }

            UnityEngine.Debug.LogError("Couldn't find quality setting: " + name);
        }

        public static string RemoveInvalidDirectoryPathCharacters(string potentiallyInvalidFilePath_) {
            string invalid = new string(System.IO.Path.GetInvalidPathChars());
            for(int c = 0;c < invalid.Length;++c) {
                potentiallyInvalidFilePath_ = potentiallyInvalidFilePath_.Replace(invalid[c].ToString(), "");
            }
            return potentiallyInvalidFilePath_;
        }

        public static string RemoveInvalidFileNameCharacters(string potentiallyInvalidFileName_) {
            string invalid = new string(System.IO.Path.GetInvalidFileNameChars());
            for(int c = 0;c < invalid.Length;++c) {
                potentiallyInvalidFileName_ = potentiallyInvalidFileName_.Replace(invalid[c].ToString(), "");
            }
            return potentiallyInvalidFileName_;
        }

        public static Vector2 ClampRayToRect(Vector2 pointOnRay, Rect rect, bool forceToEdge = false) {
            if(!forceToEdge && rect.Contains(pointOnRay)) {
                return pointOnRay;
            }
            Vector2 max = new Vector2(rect.xMax, rect.yMax);

            //force point out of rectangle
            Vector2 pointOnRay_local = (pointOnRay - rect.center).normalized * max.magnitude * 2f;

            //avoid divide by zero cases
            if(pointOnRay_local.magnitude == 0) {
                return pointOnRay;
            }
            if(pointOnRay_local.x == 0) {
                return pointOnRay_local.y > 0 ? new Vector2(0, rect.yMax) : new Vector2(0, rect.yMin);
            }
            if(pointOnRay_local.y == 0) {
                return pointOnRay_local.x > 0 ? new Vector2(0, rect.xMax) : new Vector2(0, rect.xMin);
            }

            Vector2 edgeToRayRatios = (max - rect.center);
            edgeToRayRatios.x /= Mathf.Abs(pointOnRay_local.x);
            edgeToRayRatios.y /= Mathf.Abs(pointOnRay_local.y);

            return (edgeToRayRatios.x < edgeToRayRatios.y) ?
                new Vector2(pointOnRay_local.x > 0 ? rect.xMax : rect.xMin, pointOnRay_local.y * edgeToRayRatios.x + rect.center.y) :
                new Vector2(pointOnRay_local.x * edgeToRayRatios.y + rect.center.x, pointOnRay_local.y > 0 ? rect.yMax : rect.yMin);
        }

        //public static void AddShadowAndOutline(this UnityEngine.UI.Text target)
        //{
        //    //create shadow if it doesnt exist
        //    //check if shadows are outlines; since Outline : Shadow
        //    var allShadows = target.GetComponents<UnityEngine.UI.Shadow>();
        //    UnityEngine.UI.Shadow existingShadow = null;
        //    for (int s = 0; s < allShadows.Length; ++s)
        //    {
        //        if (!(allShadows[s] is UnityEngine.UI.Outline))
        //        {
        //            existingShadow = allShadows[s];
        //        }
        //    }
        //    var shadow = existingShadow ?? target.GetOrAddComponent<UnityEngine.UI.Shadow>();
        //    var outline = target.GetOrAddComponent<UnityEngine.UI.Outline>();

        //    if (target is FRG.Core.UI.UIText)
        //    {
        //        var uiText = target as FRG.Core.UI.UIText;
        //        uiText.shadow = shadow;
        //        uiText.outline = outline;
        //        shadow.enabled = true;
        //        outline.enabled = true;
        //        shadow.effectColor = Color.black;
        //        outline.effectColor = Color.black;
        //    }
        //}

        /// <summary>
        /// Gets the full path of the given object in the scene as well as the file system, if applicable.
        /// Also prints type information.
        /// </summary>
        public static string GetObjectPath(UnityEngine.Object obj) {
            return GetObjectPath(obj, false);
        }

        /// <summary>
        /// Gets the full path of the given object in the scene as well as the file system, if applicable.
        /// Also prints type information.
        /// </summary>
        public static string GetObjectPath(UnityEngine.Object obj, bool useFullPath) {
            if(ReferenceEquals(obj, null)) {
                return "<null>";
            }

            string type;
            if(useFullPath) {
                type = ReflectionUtil.CSharpFullName(obj.GetType());
            }
            else {
                type = ReflectionUtil.CSharpName(obj.GetType());
            }

            // Destroyed objects still have some data.
            if(obj == null) {
                return "<destroyed " + type + ">";
            }

            using(Pooled<StringWriter> pooled = RecyclingPool.SpawnStringWriter()) {
                StringWriter writer = pooled.Value;

                string assetPath = "";
#if UNITY_EDITOR
                assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
#endif
                bool isPersistent = !string.IsNullOrEmpty(assetPath);

                Transform transform = null;
                Component component = obj as Component;
                GameObject gameObject = obj as GameObject;
                if(gameObject == null && component != null) gameObject = component.gameObject;
                if(gameObject != null) transform = gameObject.transform;

                if(transform != null) {
                    List<string> stringList = RecyclingPool.SpawnRaw<List<string>>();
                    try {
                        Transform iter = transform;
                        for(;iter != null;iter = iter.parent) {
                            string name = iter.gameObject.name;
                            if(name.IsNullOrEmpty()) {
                                stringList.Add("<unnamed>");
                            }
                            else {
                                stringList.Add(name);
                            }
                        }
                        writer.Write('/');
                        stringList.Reverse();
                        ArrayUtil.Join(writer, "/", stringList);
                    }
                    finally {
                        RecyclingPool.DespawnRaw(stringList);
                    }
                }
                else {
                    string name = obj.name;
                    if(name.IsNullOrEmpty()) {
                        writer.Write("<unnamed>");
                    }
                    else {
                        writer.Write(name);
                    }
                }

                writer.Write(" (");
                writer.Write(type);

                // No good way to tell the difference between prefabs and scene objects at runtime
#if UNITY_EDITOR
                if(component != null) {
                    writer.Write(" in ");
                }
                else if(transform != null && transform.parent != null) {
                    if(transform.parent.parent != null) {
                        writer.Write(" descendent of ");
                    }
                    else {
                        writer.Write(" child of ");
                    }
                }
                else if(!UnityEditor.AssetDatabase.IsMainAsset(obj)) {
                    writer.Write(" in ");
                }
                else {
                    writer.Write(" ");
                }

                if(ReflectionUtil.IsEditorAssembly(obj.GetType().Assembly)) {
                    writer.Write("Editor-only ");
                }

                if(isPersistent) {
                    writer.Write("Persistent ");
                }

                if(transform != null) {
                    UnityEditor.PrefabType prefabType = UnityEditor.PrefabUtility.GetPrefabType(obj);
                    if(prefabType == UnityEditor.PrefabType.None) {
                        writer.Write("Object");
                    }
                    else {
                        writer.Write(ReflectionUtil.GetInspectorDisplayName(prefabType.ToString()));
                    }
                }
                else {
                    writer.Write("Asset");
                }
#endif
                bool inBrackets = false;

                if(isPersistent) {
                    writer.Write(inBrackets ? "; " : " [");
                    inBrackets = true;
                    if(!useFullPath) {
                        string reducedPath = Path.GetFileName(assetPath);
                        if(!string.IsNullOrEmpty(reducedPath)) assetPath = reducedPath;
                    }
                    writer.Write(assetPath);
                }

                if(gameObject != null) {
                    Scene scene = gameObject.scene;
                    if(scene.IsValid()) {
                        string sceneName = scene.name;
                        if(!string.IsNullOrEmpty(sceneName) && !string.Equals(sceneName, "DontDestroyOnLoad", StringComparison.OrdinalIgnoreCase)) {
                            writer.Write(inBrackets ? "; " : " [");
                            inBrackets = true;
                            writer.Write("scene ");
                            writer.Write(sceneName);
                        }
                    }
                }
                if(inBrackets) { writer.Write("]"); }

                if(obj.hideFlags != HideFlags.None) {
                    writer.Write(", ");
                    writer.Write(obj.hideFlags.ToString());
                }

                writer.Write(')');

                return writer.ToString();
            }
        }

        public static List<T> FindComponentsOnGameObjects<T>(IEnumerable<GameObject> gos) {
            List<T> result = new List<T>();
            foreach(var go in gos) {
                var comp = go.GetComponent<T>();
                // note we have to compare to "null" since some components that had a shortcut before
                // like .camera or .collider will return a null object instead of null.. wtf
                if(comp != null && comp.ToString() != "null")
                    result.Add(comp);
            }
            return result;
        }

        public static List<GameObject> FilterRootGameObjects(IEnumerable<GameObject> gos, bool isFilterRemovingThem = false) {
            var filterMatchedGos = new List<GameObject>();
            var filterUnMatchedGos = new List<GameObject>();
            foreach(var go in gos) {
                if(go == null)
                    continue;
                if(go.transform.root == go.transform/* && go.scene.name != "DontDestroyOnLoad"*/)
                    filterMatchedGos.Add(go);
                else
                    filterUnMatchedGos.Add(go);
            }
            if(isFilterRemovingThem)
                return filterUnMatchedGos;
            else
                return filterMatchedGos;
        }

        public static Vector2 Abs(this Vector2 vec) {
            return new Vector2(vec.x < 0f ? -vec.x : vec.x,
                               vec.y < 0f ? -vec.y : vec.y);
        }

        public static Vector3 Abs(this Vector3 vec) {
            return new Vector3(vec.x < 0f ? -vec.x : vec.x,
                               vec.y < 0f ? -vec.y : vec.y,
                               vec.z < 0f ? -vec.z : vec.z);
        }

        public static T GetOrAddComponent<T>(this Component object_) where T : UnityEngine.Component {
            if(object_ == null) return null;
            else {
                var ret = object_.GetComponent<T>();
                if(ret == null) ret = object_.gameObject.AddComponent<T>();
                return ret;
            }
        }

        public static T GetOrAddComponent<T>(this GameObject object_) where T : UnityEngine.Component {
            if(object_ == null) return null;
            else {
                var ret = object_.GetComponent<T>();
                if(ret == null) ret = object_.AddComponent<T>();
                return ret;
            }
        }

        /// <summary>
        /// Return all child components of type T via depth-first-search. Recursion will halt if the specified validator function returns false
        /// </summary>
        /// <typeparam name="T">Type of component to search for</typeparam>
        /// <param name="trans">Parent transform to search</param>
        /// <param name="continueRecursion">Function required to return true to continue recursion</param>
        /// <param name="addToResults">Function requred to return true to add a component to the results list</param>
        public static List<T> GetComponentsInChildren<T>(this Transform trans, Func<Transform, bool> continueRecursion, Func<T, bool> addToResults)
            where T : UnityEngine.Component {

            //initialize the return list
            var ret = new List<T>();

            //call the body function to populate the results list
            trans.GetComponentsInChildren(ret, continueRecursion, addToResults);

            //return
            return ret;
        }

        /// <summary>
        /// Find all child components of type T via depth-first-search. Recursion will halt if the specified validator function returns false
        /// </summary>
        /// <typeparam name="T">Type of component to search for</typeparam>
        /// <param name="trans">Parent transform to search</param>
        /// <param name="results">Buffer list to populate with the results</param>
        /// <param name="continueRecursion">Function required to return true to continue recursion</param>
        /// <param name="addToResults">Function requred to return true to add a component to the results list</param>
        public static void GetComponentsInChildren<T>(this Transform trans, List<T> results, Func<Transform, bool> continueRecursion, Func<T, bool> addToResults)
            where T : UnityEngine.Component {

            //disallow a null buffer list
            if(results == null) {
                throw new ArgumentNullException("results");
            }

            //disallow null transform
            if(trans == null) {
                throw new ArgumentNullException("trans");
            }

            //if no recursion-validator was specified, always return true
            if(continueRecursion == null) {
                continueRecursion = t => true;
            }

            //if to results-validator was specified, always return true
            if(addToResults == null) {
                addToResults = t => true;
            }

            //iterate through all of this object's children
            for(int i = 0;i < trans.childCount;++i) {
                var child = trans.GetChild(i);
                //ensure no null children
                if(child != null) {
                    //ensure that the child has the desired component, and that the validator returns true
                    T comp = child.GetComponent<T>();

                    //if the results-validator returns true, add this child to the results list
                    if(comp != null && addToResults.Invoke(comp)) {
                        results.Add(comp);
                    }

                    //if the recursion-validator returns true, continue recursing down this child
                    if(continueRecursion.Invoke(child)) {
                        child.GetComponentsInChildren(results, continueRecursion, addToResults);
                    }
                }
            }
        }

        private static readonly Vector3 _vec3_noZ = new Vector3(1f, 1f, 0f);

        /// <summary>
        /// Resets position, rotation, and scale of a transform
        /// </summary>
        public static void ResetTransform(Transform transform_) {
            if(transform_ is RectTransform) {
                ((RectTransform)transform_).anchoredPosition = Vector2.zero;
                transform_.localPosition = Vector3.Scale(transform_.localPosition, _vec3_noZ);
            }
            else {
                transform_.localPosition = Vector3.zero;
            }

            transform_.localScale = Vector3.one;
            transform_.localRotation = Quaternion.identity;
        }

        private static Vector3[] localCorners = new Vector3[4];
        public static Vector2 GetRectTransformLocalCenter(RectTransform t) {
            Vector2 pos = Vector2.zero;
            if(t == null) return pos;

            t.GetLocalCorners(localCorners);
            if(localCorners != null && localCorners.Length > 0) {
                pos /= localCorners.Length;
            }
            return pos;
        }

        private static Vector3[] corners = new Vector3[4];
        public static Vector2 GetRectTransformCenterOnScreen(RectTransform t, Canvas canvas = null) {
            Vector2 pos = Vector2.zero;
            if(t == null) return pos;

            if(canvas == null) canvas = t.gameObject.GetComponentInParent<Canvas>();

            Camera cam = null;
            if(canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) {
                cam = canvas.worldCamera;
                if(cam == null) cam = Camera.main;
            }

            t.GetWorldCorners(corners);
            if(corners != null && corners.Length > 0) {
                for(int i = 0;i < corners.Length;i++) {
                    pos += RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
                }
                pos /= corners.Length;
            }
            return pos;
        }

        public static Vector2 GetRectTransformDimensions(RectTransform t) {
            Vector2 size = Vector2.zero;
            if(t == null) return size;
            t.GetLocalCorners(corners);
            return corners[2] - corners[0];
        }

        public static Vector2 GetDimensions(RectTransform t) {
            Vector2 pos = Vector2.zero;
            if(t == null) return pos;

            t.GetLocalCorners(corners);

            return new Vector2(Mathf.Abs(corners[2].x - corners[0].x), Mathf.Abs(corners[2].y - corners[0].y));
        }

        public static bool Beholding(Vector3 beauty) {
            return Beholding(Camera.main, beauty, defaultViewportRect);
        }
        private static Rect defaultViewportRect = new Rect(0f, 0f, 1f, 1f);
        public static bool Beholding(Camera camera, Vector3 beauty) {
            return Beholding(camera, beauty, defaultViewportRect);
        }
        public static bool Beholding(Camera camera, Vector3 beauty, Rect viewRect) {
            Vector3 viewOfBeauty = camera.WorldToViewportPoint(beauty);
            if(viewOfBeauty.z < 0f) return false;
            return viewRect.Contains(viewOfBeauty);
        }

        public static bool SegmentIntersectsRect(Vector2 a1, Vector2 a2, Rect rect, out Vector2 intersection) {

            Vector2 bottomLeft = new Vector2(rect.xMin, rect.yMin);
            Vector2 bottomRight = new Vector2(rect.xMax, rect.yMin);

            //bottom segment
            if(SegmentIntersection(a1, a2, bottomLeft, bottomRight, out intersection)) return true;

            Vector2 upperLeft = new Vector2(rect.xMin, rect.yMax);

            //left segment
            if(SegmentIntersection(a1, a2, bottomLeft, upperLeft, out intersection)) return true;

            Vector2 upperRight = new Vector2(rect.xMax, rect.yMax);

            //top segment
            if(SegmentIntersection(a1, a2, upperLeft, upperRight, out intersection)) return true;

            //right
            if(SegmentIntersection(a1, a2, bottomRight, upperRight, out intersection)) return true;

            return false;
        }

        // a1 is line1 start, a2 is line1 end, b1 is line2 start, b2 is line2 end
        public static bool SegmentIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection) {
            intersection = Vector2.zero;

            Vector2 b = a2 - a1;
            Vector2 d = b2 - b1;
            float bCrossD = b.x * d.y - b.y * d.x;

            // if b cross d == 0, it means the lines are parallel so have infinite intersection points
            if(bCrossD == 0) return false;

            Vector2 c = b1 - a1;
            float t = (c.x * d.y - c.y * d.x) / bCrossD;
            if(t < 0 || t > 1) return false;

            float u = (c.x * b.y - c.y * b.x) / bCrossD;
            if(u < 0 || u > 1) return false;

            intersection = a1 + t * b;

            return true;
        }

        //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
        //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
        //same plane, use ClosestPointsOnTwoLines() instead.
        public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {

            Vector3 lineVec3 = linePoint2 - linePoint1;
            Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
            Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

            float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

            //is coplanar, and not parrallel
            if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f) {
                float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
                intersection = linePoint1 + (lineVec1 * s);
                return true;
            }
            else {
                intersection = Vector3.zero;
                return false;
            }
        }

        public static void Quadratic( float a, float b, float c, out float t1, out float t2 ) {
            if ( Mathf.Abs(a) < .01f ) {
                t1 = t2 = 0;
                return;
            }
            float sqrPart = b*b - 4*a*c;
            if ( sqrPart < 0 ) {
                t1 = t2 = 0;
                return;
            }
            sqrPart = Mathf.Sqrt( sqrPart );
            t1 = ( -b + sqrPart ) / (2*a) ;
            t2 = ( -b - sqrPart ) / (2*a) ;

        }

        public static Vector3 Intercept(Vector3 targetPos, Vector3 targetVel, Vector3 hunterPos, float hunterSpeed) {

            float soldierSpeed = targetVel.magnitude;
            Vector3 targetToZombie = hunterPos - targetPos;
            float dist = targetToZombie.magnitude;
            float a = hunterSpeed * hunterSpeed - soldierSpeed * soldierSpeed;
            float b = 2 * Vector3.Dot(targetToZombie, targetVel);
            float c = -dist * dist;

            float t1, t2;
            Util.Quadratic(a, b, c, out t1, out t2);

            if(t1 < 0 && t2 < 0) return targetPos;
            float t = t1;
            if(t1 < 0) t = t2;
            else if(t2 > 0 && t2 < t1) t = t2;

            t = Mathf.Min(4, t);
            Vector3 retVect = targetPos + targetVel * t;

            return retVect;

        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("CONTEXT/RectTransform/Bake Anchors")]
        public static void BakeAnchors() {
            RectTransform target = UnityEditor.Selection.activeGameObject.transform as RectTransform;
            if(target != null) {
                BakeAnchors(target, true, true);
            }
        }
        [UnityEditor.MenuItem("CONTEXT/RectTransform/Bake Anchors (X Axis Only)")]
        public static void BakeAnchors_XOnly() {
            RectTransform target = UnityEditor.Selection.activeGameObject.transform as RectTransform;
            if(target != null) {
                BakeAnchors(target, true, false);
            }
        }
        [UnityEditor.MenuItem("CONTEXT/RectTransform/Bake Anchors (Y Axis Only)")]
        public static void BakeAnchors_YOnly() {
            RectTransform target = UnityEditor.Selection.activeGameObject.transform as RectTransform;
            if(target != null) {
                BakeAnchors(target, false, true);
            }
        }

        [UnityEditor.MenuItem("CONTEXT/RectTransform/Bake Anchors (Include Children)")]
        public static void BakeAnchors_Recursive() {
            var target = UnityEditor.Selection.activeGameObject.transform as RectTransform;
            if(target != null) {
                BakeAnchors_Recursive(target, true, true);
            }
        }

        [UnityEditor.MenuItem("CONTEXT/RectTransform/Bake Anchors (Include Children) (X Axis Only)")]
        public static void BakeAnchors_Recursive_XOnly() {
            var target = UnityEditor.Selection.activeGameObject.transform as RectTransform;
            if(target != null) {
                BakeAnchors_Recursive(target, true, false);
            }
        }

        [UnityEditor.MenuItem("CONTEXT/RectTransform/Bake Anchors (Include Children) (Y Axis Only)")]
        public static void BakeAnchors_Recursive_YOnly() {
            var target = UnityEditor.Selection.activeGameObject.transform as RectTransform;
            if(target != null) {
                BakeAnchors_Recursive(target, false, true);
            }
        }
#endif

        public static void BakeAnchors(RectTransform target, bool xAxis, bool yAxis) {
            RectTransform parent = target.parent as RectTransform;

            if(target != null && parent != null) {
                Vector2 parentSize = parent.rect.size;
                Vector2 thisMin = target.anchoredPosition
                                + Vector2.Scale(new Vector2(target.anchorMin.x - 0.5f, target.anchorMin.y - 0.5f), parentSize)
                                - (target.sizeDelta / 2f);
                Vector2 thisMax = target.anchoredPosition
                                + Vector2.Scale(new Vector2(target.anchorMax.x - 0.5f, target.anchorMax.y - 0.5f), parentSize)
                                + (target.sizeDelta / 2f);

                Vector2 parentMin = parentSize / -2f;

                Vector2 targetMin = Vector2.Scale((thisMin - parentMin), new Vector2(1f / parentSize.x, 1f / parentSize.y));
                Vector2 targetMax = Vector2.Scale((thisMax - parentMin), new Vector2(1f / parentSize.x, 1f / parentSize.y));

                if(xAxis && yAxis) {
                    target.anchorMin = targetMin;
                    target.anchorMax = targetMax;
                    target.anchoredPosition = Vector2.zero;
                    target.localScale = Vector3.one;
                    target.sizeDelta = Vector2.zero;
                }
                else if(xAxis && !yAxis) {
                    target.anchorMin = new Vector2(targetMin.x, target.anchorMin.y);
                    target.anchorMax = new Vector2(targetMax.x, target.anchorMax.y);
                    target.anchoredPosition = new Vector2(0f, target.anchoredPosition.y);
                    target.localScale = new Vector3(1f, target.localScale.y, 1f);
                    target.sizeDelta = new Vector2(0f, target.sizeDelta.y);
                }
                else if(!xAxis && yAxis) {
                    target.anchorMin = new Vector2(target.anchorMin.x, targetMin.y);
                    target.anchorMax = new Vector2(target.anchorMax.x, targetMax.y);
                    target.anchoredPosition = new Vector2(target.anchoredPosition.x, 0f);
                    target.localScale = new Vector3(target.localScale.x, 1f, 1f);
                    target.sizeDelta = new Vector2(target.sizeDelta.x, 0f);
                }

            }
        }

        public static void BakeAnchors_Recursive(RectTransform target, bool xAxis, bool yAxis) {
            if(target != null) {

                foreach(var child in target.GetComponentsInChildren<RectTransform>(true)) {
                    if(child == null || child.GetInstanceID() == target.GetInstanceID()) continue;
                    BakeAnchors_Recursive(child, xAxis, yAxis);
                }

                BakeAnchors(target, xAxis, yAxis);
            }
        }

        public enum AnchorPreset {
            TopLeft, TopMiddle, TopRight,
            MiddleLeft, MiddleCenter, MiddleRight,
            BottomLeft, BottomMiddle, BottomRight,
            TopStretchX, BottomStretchX, MiddleStretchX,
            LeftStretchY, RightStretchY, MiddleStretchY,
            FullStretchXY
        };

        public static void ResetAnchors(this RectTransform rect_, Transform parent_, AnchorPreset settings_) {
            if(rect_ == null) return;

            rect_.SetParent(parent_, false);

            switch(settings_) {
                case AnchorPreset.TopLeft:
                    rect_.anchorMin = new Vector2(0f, 1f);
                    rect_.anchorMax = new Vector2(0f, 1f);
                    break;
                case AnchorPreset.TopMiddle:
                    rect_.anchorMin = new Vector2(0.5f, 1f);
                    rect_.anchorMax = new Vector2(0.5f, 1f);
                    break;
                case AnchorPreset.TopRight:
                    rect_.anchorMin = new Vector2(1f, 1f);
                    rect_.anchorMax = new Vector2(1f, 1f);
                    break;
                case AnchorPreset.MiddleLeft:
                    rect_.anchorMin = new Vector2(0f, 0.5f);
                    rect_.anchorMax = new Vector2(0f, 0.5f);
                    break;
                case AnchorPreset.MiddleCenter:
                    rect_.anchorMin = new Vector2(0.5f, 0.5f);
                    rect_.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                case AnchorPreset.MiddleRight:
                    rect_.anchorMin = new Vector2(1f, 0.5f);
                    rect_.anchorMax = new Vector2(1f, 0.5f);
                    break;
                case AnchorPreset.BottomLeft:
                    rect_.anchorMin = new Vector2(0f, 0f);
                    rect_.anchorMax = new Vector2(0f, 0f);
                    break;
                case AnchorPreset.BottomMiddle:
                    rect_.anchorMin = new Vector2(0.5f, 0f);
                    rect_.anchorMax = new Vector2(0.5f, 0f);
                    break;
                case AnchorPreset.BottomRight:
                    rect_.anchorMin = new Vector2(1f, 0f);
                    rect_.anchorMax = new Vector2(1f, 0f);
                    break;
                case AnchorPreset.TopStretchX:
                    rect_.anchorMin = new Vector2(0f, 1f);
                    rect_.anchorMax = new Vector2(1f, 1f);
                    rect_.sizeDelta = new Vector2(0f, rect_.sizeDelta.y);
                    break;
                case AnchorPreset.MiddleStretchX:
                    rect_.anchorMin = new Vector2(0f, 0.5f);
                    rect_.anchorMax = new Vector2(1f, 0.5f);
                    rect_.sizeDelta = new Vector2(0f, rect_.sizeDelta.y);
                    break;
                case AnchorPreset.BottomStretchX:
                    rect_.anchorMin = new Vector2(0f, 0f);
                    rect_.anchorMax = new Vector2(1f, 0f);
                    rect_.sizeDelta = new Vector2(0f, rect_.sizeDelta.y);
                    break;
                case AnchorPreset.LeftStretchY:
                    rect_.anchorMin = new Vector2(0f, 0f);
                    rect_.anchorMax = new Vector2(0f, 1f);
                    rect_.sizeDelta = new Vector2(rect_.sizeDelta.x, 0f);
                    break;
                case AnchorPreset.MiddleStretchY:
                    rect_.anchorMin = new Vector2(0.5f, 0f);
                    rect_.anchorMax = new Vector2(0.5f, 1f);
                    rect_.sizeDelta = new Vector2(rect_.sizeDelta.x, 0f);
                    break;
                case AnchorPreset.RightStretchY:
                    rect_.anchorMin = new Vector2(1f, 0f);
                    rect_.anchorMax = new Vector2(1f, 1f);
                    rect_.sizeDelta = new Vector2(rect_.sizeDelta.x, 0f);
                    break;
                case AnchorPreset.FullStretchXY:
                    rect_.anchorMin = new Vector2(0f, 0f);
                    rect_.anchorMax = new Vector2(1f, 1f);
                    rect_.sizeDelta = new Vector2(0f, 0f);
                    break;
            }

            rect_.anchoredPosition3D = Vector3.zero;
            rect_.localScale = Vector3.one;
            rect_.localRotation = Quaternion.identity;
            rect_.localPosition = Vector3.Scale(rect_.localPosition, _vec3_noZ);
        }

        public static void SetNavigationMode(this UnityEngine.UI.Button button_, UnityEngine.UI.Navigation.Mode mode_) {
            try {
                UnityEngine.UI.Navigation newNav = new UnityEngine.UI.Navigation();
                System.Reflection.FieldInfo navMode = newNav.GetType().GetField("mode");
                if(navMode != null) {
                    navMode.SetValue(newNav, mode_);
                }
                button_.navigation = newNav;
            }
            catch(Exception e) {
                ReflectionUtil.CheckDangerousException(e);

                //logger.Warn("Failure to set button navigation mode.", e);
            }
        }

        /// <summary> Return this Vector3 with it's X component set to 0 </summary>
        public static Vector3 ZeroX(this Vector3 vec_) { return new Vector3(0f, vec_.y, vec_.z); }
        /// <summary> Return this Vector3 with it's Y component set to 0 </summary>
        public static Vector3 ZeroY(this Vector3 vec_) { return new Vector3(vec_.x, 0f, vec_.z); }
        /// <summary> Return this Vector3 with it's Z component set to 0 </summary>
        public static Vector3 ZeroZ(this Vector3 vec_) { return new Vector3(vec_.x, vec_.y, 0f); }

        /// <summary> Return this Vector3 with it's X & Y components set to 0 </summary>
        public static Vector3 ZeroXY(this Vector3 vec_) { return new Vector3(0f, 0f, vec_.z); }
        /// <summary> Return this Vector3 with it's X & Z components set to 0 </summary>
        public static Vector3 ZeroXZ(this Vector3 vec_) { return new Vector3(0f, vec_.y, 0f); }
        /// <summary> Return this Vector3 with it's Y & Z components set to 0 </summary>
        public static Vector3 ZeroYZ(this Vector3 vec_) { return new Vector3(vec_.x, 0f, 0f); }

        public static Vector2 SetX(this Vector2 vec, float newX) { return new Vector2(newX, vec.y); }
        public static Vector2 SetY(this Vector2 vec, float newY) { return new Vector2(vec.x, newY); }

        public static Vector3 SetX(this Vector3 vec, float newX) { return new Vector3(newX, vec.y, vec.z); }
        public static Vector3 SetY(this Vector3 vec, float newY) { return new Vector3(vec.x, newY, vec.z); }
        public static Vector3 SetZ(this Vector3 vec, float newZ) { return new Vector3(vec.x, vec.y, newZ); }

        public static Vector4 SetX(this Vector4 vec, float newX) { return new Vector4(newX, vec.y, vec.z, vec.w); }
        public static Vector4 SetY(this Vector4 vec, float newY) { return new Vector4(vec.x, newY, vec.z, vec.w); }
        public static Vector4 SetZ(this Vector4 vec, float newZ) { return new Vector4(vec.x, vec.y, newZ, vec.w); }
        public static Vector4 SetW(this Vector4 vec, float newW) { return new Vector4(vec.x, vec.y, vec.z, newW); }

        private static bool hasCapturedOriginalDpi = false;
        private static int originalResolution;
        private static float originalDpi;

        private static void CaptureInitialDpi() {
            if(!hasCapturedOriginalDpi) {
                originalResolution = Math.Max(Screen.width, Screen.height);
                originalDpi = Screen.dpi;
            }
        }

        /// <summary>
        /// Grabs dpi, adjusted for screen size.
        /// Does not figure in ios reduced screen size.
        /// </summary>
        public static float dpi {
            get {
                float result = Screen.dpi;
                if ( Application.isEditor ) result *= 2;
                if(result <= 0) {
                    return Application.isMobilePlatform ? 100.0f : 144.0f;
                }

                CaptureInitialDpi();

                if(result != originalDpi) {
                    return result;
                }

                float ratio = Math.Max(Screen.width, Screen.height) / (float)originalResolution;
                return result * ratio;
            }
        }

        public static void SetResolutionSafe(int width, int height, bool fullscreen) {
            CaptureInitialDpi();
            Screen.SetResolution(width, height, fullscreen);
        }

        public static void SetResolutionSafe(int width, int height, bool fullscreen, int refreshRate) {
            CaptureInitialDpi();
            Screen.SetResolution(width, height, fullscreen, refreshRate);
        }

        /// <summary>
        /// Returns SystemLanguage from provided name (case insensitive). If no match SystemLanguage.Unknown is returned.
        /// </summary>
        public static SystemLanguage GetSystemLanguageFromName(string languageName) {
            foreach(SystemLanguage language in Enum.GetValues(typeof(SystemLanguage))) {
                if(string.Equals(language.ToString(), languageName, StringComparison.OrdinalIgnoreCase)) {
                    return language;
                }
            }
            return SystemLanguage.Unknown;
        }

        /// <summary>
        /// Coroutine which waits (delaySeconds) seconds, then calls the specified function
        /// </summary>
        public static IEnumerator ExecuteAfterDelay(float delaySeconds, Action function) {
            yield return new WaitForSeconds(delaySeconds);
            if(function != null)
                function();
        }

        /// <summary>
        /// Coroutine which executes variable number of functions in sequence, delayed by their key values
        /// </summary>
        public static IEnumerator ExecuteInSequence(params KeyValuePair<float, Action>[] functions_) {
            for(int i = 0;i < functions_.Length;++i) {
                yield return new WaitForSeconds(functions_[i].Key);
                if(functions_[i].Value != null)
                    functions_[i].Value();
            }
        }

#if UNITY_EDITOR
        public static GameObject[] FindSceneGameObjects() {
            return Array.FindAll(Resources.FindObjectsOfTypeAll<GameObject>(), go => !UnityEditor.EditorUtility.IsPersistent(go));
        }

        public static Texture2D LoadBase64PngTexture(string name, string base64Data) {
            // Get image data (PNG) from base64 encoded strings.
            byte[] imageData = Convert.FromBase64String(base64Data);

            // Gather image size from image data.
            int texWidth, texHeight;
            GetPngImageSize(imageData, out texWidth, out texHeight);

            // Generate texture asset.
            var tex = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.name = name;
            tex.filterMode = FilterMode.Point;
            tex.LoadImage(imageData);

            return tex;
        }

        private static void GetPngImageSize(byte[] imageData, out int width, out int height) {
            width = ReadPngInt(imageData, 3 + 15);
            height = ReadPngInt(imageData, 3 + 15 + 2 + 2);
        }

        private static int ReadPngInt(byte[] imageData, int offset) {
            return (imageData[offset] << 8) | imageData[offset + 1];
        }

        public static bool IsReadableTextureFormat(Texture2D source) {
            return (source.format == TextureFormat.ARGB32 || source.format == TextureFormat.RGBA32 || source.format == TextureFormat.RGB24);
        }

        public static bool IsTextureReadWriteEnabled(Texture2D source) {
            try {
                source.GetPixel(0, 0);
                return true;
            }
            catch(UnityException) {
                return false;
            }
        }
#endif

        public static bool Contains(this string self, string value, bool ignoreCase) {
            return Contains(self, value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        public static bool Contains(this string self, string value, StringComparison comparisonType) {
            if(self == null) throw new ArgumentNullException("self");
            if(value == null) throw new ArgumentNullException("value");
            return self.IndexOf(value, comparisonType) >= 0;
        }

        public static int[] arabicNums = new int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        public static string[] romanNums = new string[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

        public static string GetRomanNumeral(int value) {
            string result = "";
            for(int i = 0;i < 13;i++) {
                while(value >= arabicNums[i]) {
                    result = result + romanNums[i].ToString();
                    value = value - arabicNums[i];
                }
            }
            return result;
        }

        private const string passwordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ0123456789!#$%&()=?";
        /// <summary>
        /// Generates random string from ascii chars without chars that may cause confusion (like zero and letter O)
        /// </summary>
        public static string GenerateRandomPassword(int length) {
            var random = new System.Random();
            using(Pooled<StringWriter> pooled = RecyclingPool.SpawnStringWriter()) {
                StringWriter writer = pooled.Value;

                for(int i = 0;i < length;++i) {
                    writer.Write(passwordChars[random.Next(0, passwordChars.Length)]);
                }
                return writer.ToString();
            }
        }

        /// <summary>
        /// Reads all bytes from a file. Needed as android can't use File.ReadAllBytes() or any other file operations as content is zipped in jar
        /// </summary>
        public static byte[] ReadAllBytes_CrossPlatform(string path) {
            if(Application.platform == RuntimePlatform.Android) {
                WWW filePath = new WWW(path);
                while(!filePath.isDone) {
                    Thread.Sleep(1);
                }
                if(!string.IsNullOrEmpty(filePath.error)) {
                    UnityEngine.Debug.LogError("ReadAllBytes_CrossPlatform(): failed to read path: " + path + "\nError: " + filePath.error);
                    return null;
                }

                return filePath.bytes;
            }

            // all other platforms just use normal
            return File.ReadAllBytes(path);
        }

        /// <summary>
        /// Reorder pixels so we adjust for rotation and flips. Flipping both sides is equal to 180 deg rotation.
        /// Texture3d pixels are flattened array from left to right, bottom to top.
        /// </summary>
        public static void FlipTexturePixels(Color32[] pixels, int width, int height, bool shouldFlipHorizontally, bool shouldFlipVertically) {
            if(!shouldFlipHorizontally && !shouldFlipVertically)
                return;

            // passed wrong dimensions
            if(width * height != pixels.Length) {
                UnityEngine.Debug.LogError("FlipTexturePixels(): width*height does not match pixel array size: " + pixels.Length + ", for: " + width + "x" + height);
                return;
            }

            if(shouldFlipHorizontally && shouldFlipVertically) {
                Array.Reverse(pixels);
                return;
            }

            if(shouldFlipHorizontally) {
                // swap left and right
                for(int rowNum = 0;rowNum < height;rowNum++) {
                    for(int colNum = 0;colNum < width / 2;colNum++) {
                        int pixelPos1 = rowNum * width + colNum;
                        int pixelPos2 = rowNum * width + (width - 1 - colNum);
                        Color temp = pixels[pixelPos1];
                        pixels[pixelPos1] = pixels[pixelPos2];
                        pixels[pixelPos2] = temp;
                    }
                }
            }
            if(shouldFlipVertically) {
                // swap up and down
                for(int rowNum = 0;rowNum < height / 2;rowNum++) {
                    for(int colNum = 0;colNum < width;colNum++) {
                        int pixelPos1 = rowNum * width + colNum;
                        int pixelPos2 = (height - 1 - rowNum) * width + colNum;
                        Color temp = pixels[pixelPos1];
                        pixels[pixelPos1] = pixels[pixelPos2];
                        pixels[pixelPos2] = temp;
                    }
                }
            }
        }

        /// <summary>
        /// Uses standard "over" blend with alpha channel included. c1 is going over c2.
        /// https://en.wikipedia.org/wiki/Alpha_compositing
        /// </summary>
        public static Color32 CombineColor32(Color32 c1, Color32 c2) {
            if(c1.a == 0)
                return c2;

            if(c1.a == byte.MaxValue || c2.a == 0)
                return c1;

            // alpha to 0..1 range
            float c1a = (float)c1.a / (byte.MaxValue - 1);
            float c2a = (float)c2.a / (byte.MaxValue - 1);

            byte red = (byte)((c1.r * c1a + (1 - c1a) * (c2.r * c2a)) / (c1a + c2a * (1 - c1a)));
            byte green = (byte)((c1.g * c1a + (1 - c1a) * (c2.g * c2a)) / (c1a + c2a * (1 - c1a)));
            byte blue = (byte)((c1.b * c1a + (1 - c1a) * (c2.b * c2a)) / (c1a + c2a * (1 - c1a)));
            byte alpha = (byte)((c1a + c2a * (1 - c1a)) * (byte.MaxValue - 1));
            return new Color32(red, green, blue, alpha);
        }

        public enum OverlayCorner {
            TopLeft, TopRight, BottomLeft, BottomRight
        }

        /// <summary>
        /// Loads a texture from a base64 string serialized from a byte array
        /// </summary>
        /// <remarks>
        /// Byte array sequence is as follows: { R,G,B,A,R,G,B,A,R,G,B,A, etc}
        /// Texture size is assumed to be square; and is inferred from the square-root of the color array.
        /// See LoadBase64PngTexture() for a more comprehensive solution
        /// </remarks>
        public static Texture2D LoadSquareTextureFromByteArray(string bytesToBase64, bool isGZipped) {
            if(string.IsNullOrEmpty(bytesToBase64)) {
                return Texture2D.whiteTexture;
            }
            else {
                //if(isGZipped) {
                //    return LoadSquareTextureFromByteArray(Convert.FromBase64String(BitUtil.DecompressStringFromGZipBase64(bytesToBase64)));
                //} else {
                return LoadSquareTextureFromByteArray(Convert.FromBase64String(bytesToBase64));
                //}
            }
        }

        /// <summary>
        /// Loads a texture from a byte array
        /// </summary>
        /// <remarks>
        /// Byte array sequence is as follows: { R,G,B,A,R,G,B,A,R,G,B,A, etc}
        /// Texture size is assumed to be square; and is inferred from the square-root of the color array.
        /// See LoadBase64PngTexture() for a more comprehensive solution
        /// </remarks>
        public static Texture2D LoadSquareTextureFromByteArray(byte[] bytes) {
            if(bytes == null || bytes.Length == 0) {
                return Texture2D.whiteTexture;
            }
            else {
                try {
                    Color[] cols = ColorUtil.FromBytes(bytes);
                    Texture2D ret = new Texture2D((int)Mathf.Sqrt(cols.Length), (int)Mathf.Sqrt(cols.Length));
                    ret.SetPixels(cols);
                    ret.Apply();
                    return ret;
                }
                catch(Exception e) {
                    if(!ReflectionUtil.IsIOException(e)) { throw; }

                    //logger.Error("Failed to load square texture from byte array.", e);
                    return Texture2D.whiteTexture;
                }
            }
        }

        /// <summary>
        /// Add overlay image over another image (textures) by combining values in pixels.
        /// </summary>
        /// <param name="texture">Texture background onto which we will draw overlay corner</param>
        /// <param name="overlayTexture">Texture for the overlay. If it's larger it will be cropped.</param>
        /// <param name="corner">What corner of original texture to cover with overlay.</param>
        /// <param name="shouldFlipOverlayHorizontally">Flip overlay horizontally before applying it.</param>
        /// <param name="shouldFlipOverlayVertically">Flip overlay vertically before applying it.</param>
        /// <param name="contentWidth">
        /// How much pixels to use from the corner. Applied after flipping. Use this if you want only portion of the overlay 
        /// (e.g. overlay texture is larger then original), otherwise use -1
        /// </param>
        /// <param name="contentHeight">
        /// How much pixels to use from the corner. Applied after flipping. Use this if you want only portion of the overlay 
        /// (e.g. overlay texture is larger then original), otherwise use -1
        /// </param>
        public static void AddOverlayCornerTexture(
            Texture2D texture,
            Texture2D overlayTexture,
            OverlayCorner corner,
            bool shouldFlipOverlayHorizontally,
            bool shouldFlipOverlayVertically,
            int contentWidth = -1,
            int contentHeight = -1) {
            if(texture == null || overlayTexture == null) {
                UnityEngine.Debug.LogError("AddOverlayCornerToTexture(): texture or overlay is null.");
                return;
            }

            if(contentWidth == -1 || contentWidth > texture.width)
                contentWidth = texture.width;

            if(contentHeight == -1 || contentHeight > texture.height)
                contentHeight = texture.height;

            Color32[] texturePixels = texture.GetPixels32();
            Color32[] overlayTexturePixels = overlayTexture.GetPixels32();
            Util.FlipTexturePixels(overlayTexturePixels, overlayTexture.width, overlayTexture.height, shouldFlipOverlayHorizontally, shouldFlipOverlayVertically);

            // it's easy to flip bottom left, as flatten texture pixels go left to right, bottom to up, so flip every corner to bottom left then flip it back later
            switch(corner) {
                case OverlayCorner.TopLeft:
                    FlipTexturePixels(texturePixels, texture.width, texture.height, false, true);
                    FlipTexturePixels(overlayTexturePixels, overlayTexture.width, overlayTexture.height, false, true);
                    break;
                case OverlayCorner.TopRight:
                    FlipTexturePixels(texturePixels, texture.width, texture.height, true, true);
                    FlipTexturePixels(overlayTexturePixels, overlayTexture.width, overlayTexture.height, true, true);
                    break;
                case OverlayCorner.BottomLeft:
                    break;
                case OverlayCorner.BottomRight:
                    FlipTexturePixels(texturePixels, texture.width, texture.height, true, false);
                    FlipTexturePixels(overlayTexturePixels, overlayTexture.width, overlayTexture.height, true, false);
                    break;
            }

            // put overlay to bottom left
            for(int i = 0;i < overlayTexturePixels.Length;i++) {
                // column is going lelft to right, means width
                // rows are bottom to top, means height
                int col = i % overlayTexture.width;
                int row = i / overlayTexture.width;

                // ignore pixels from texture that are out of content size
                if(col >= contentWidth || row >= contentHeight)
                    continue;

                int texPixelPos = row * texture.width + col;
                texturePixels[texPixelPos] = Util.CombineColor32(overlayTexturePixels[i], texturePixels[texPixelPos]);
            }

            // flip back to original corner
            switch(corner) {
                case OverlayCorner.TopLeft:
                    FlipTexturePixels(texturePixels, texture.width, texture.height, false, true);
                    FlipTexturePixels(overlayTexturePixels, overlayTexture.width, overlayTexture.height, false, true);
                    break;
                case OverlayCorner.TopRight:
                    FlipTexturePixels(texturePixels, texture.width, texture.height, true, true);
                    FlipTexturePixels(overlayTexturePixels, overlayTexture.width, overlayTexture.height, true, true);
                    break;
                case OverlayCorner.BottomLeft:
                    break;
                case OverlayCorner.BottomRight:
                    FlipTexturePixels(texturePixels, texture.width, texture.height, true, false);
                    FlipTexturePixels(overlayTexturePixels, overlayTexture.width, overlayTexture.height, true, false);
                    break;
            }

            // we're done!
            texture.SetPixels32(texturePixels);
            texture.Apply();
        }

        /// <summary>
        /// Creates a new texture scaled to target width and height (billinear)
        /// Source: a blog on internet
        /// </summary>
        public static Texture2D CreateScaledTexture(Texture2D source, int targetWidth, int targetHeight) {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / targetWidth);
            float incY = (1.0f / targetHeight);
            for(int px = 0;px < rpixels.Length;px++) {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * (px / targetWidth));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }

        /// <summary>
        /// Adjust resolution for better performance.
        /// </summary>
        /// <param name="targetWidth">Target width of resolution to use, height will be calculated based on current screen ratio.</param>
        public static void AdjustScreenResolution(int targetWidth = 1024) {
            if(Screen.width > targetWidth) {
                float reductionMult = targetWidth / (float)Screen.width;
                int targetHeight = Mathf.RoundToInt(Screen.height * reductionMult);

                int setWidth = targetWidth;
                int setHeight = targetHeight;

                bool found = false;
                Resolution bestResolution = default(Resolution);
                foreach(Resolution resolution in Screen.resolutions) {
                    if(resolution.width > targetWidth || found && resolution.width < bestResolution.width) {
                        continue;
                    }

                    if(found && resolution.width == bestResolution.width && Math.Abs(targetHeight - resolution.height) >= Math.Abs(targetHeight - bestResolution.height)) {
                        continue;
                    }

                    setWidth = resolution.width;
                    setHeight = resolution.height;
                    found = true;
                }

                if(bestResolution.width != 0 && bestResolution.height != 0) {
                    found = true;
                }

                int originalWidth = Screen.width;
                int originalHeight = Screen.height;

                Util.SetResolutionSafe(setWidth, setHeight, true);

                int finalWidth = Screen.width;
                int finalHeight = Screen.height;

                UnityEngine.Debug.Log("Set resolution " + (found ? "preset" : "dynamic") + " target=" + setWidth + "x" + setHeight + " original=" + originalWidth + "x" + originalHeight + " final=" + finalWidth + "x" + finalHeight);
            }
        }

        /// <summary>
        /// Write text to a file helper. Uses File.AppendAllText and File.WriteAllText
        /// </summary>
        public static void WriteToFile(string fileName, bool append, bool insertNewLine, object content) {
            if(SystemInfo.deviceType == DeviceType.Handheld) {
                fileName = Path.Combine(Application.persistentDataPath, fileName);
            }
            if(append) {
                File.AppendAllText(fileName, insertNewLine ? content.ToString() + "\n" : content.ToString());
            }
            else {
                File.WriteAllText(fileName, insertNewLine ? content.ToString() + "\n" : content.ToString());
            }
        }

        /// <summary>
        /// Write text to a file helper. Uses File.AppendAllText and File.WriteAllText
        /// </summary>
        public static void WriteToFile(string fileName, bool append, bool insertNewLine, List<object> content) {
            if(append) {
                foreach(var str in content)
                    File.AppendAllText(fileName, insertNewLine ? str.ToString() + "\n" : str.ToString());
            }
            else {
                foreach(var str in content)
                    File.WriteAllText(fileName, insertNewLine ? str.ToString() + "\n" : str.ToString());
            }
        }

        /// <see cref="GetAllChildGameObjects_Recursive(GameObject, List{GameObject}, int)"/>
        public static List<GameObject> GetAllChildGameObjects_Recursive(GameObject parent, int maxDepth = 20) {
            var result = new List<GameObject>();
            GetAllChildGameObjects_Recursive(parent, result, maxDepth);
            return result;
        }

        /// <summary>
        /// Returns all children game objects.
        /// </summary>
        public static List<GameObject> FilterGameObjectsChildren(IEnumerable<GameObject> gos, int maxDepth = 0) {
            var result = new List<GameObject>();
            foreach(var go in gos) {
                Util.GetAllChildGameObjects_Recursive(go, result, maxDepth);
            }
            return result;
        }

        /// <summary>
        /// Loopts through parent's children, adds them to result list and recursivly adds their children also.
        /// </summary>
        /// <param name="maxDepth">How deep will recursion go, it will pickup elements from that depth, zero being first set of children</param>
        public static void GetAllChildGameObjects_Recursive(GameObject parent, List<GameObject> result, int maxDepth = 20) {
            if(maxDepth < 0)
                return;

            foreach(Transform item in parent.transform) {
                // also returns itself for some reason?
                if(item == parent.transform)
                    continue;
                result.Add(item.gameObject);
                GetAllChildGameObjects_Recursive(item.gameObject, result, maxDepth - 1);
            }
        }

        /// <summary>
        /// Use to find all scene game objects. Very slow. Doesn't work on inactive game objects.
        /// </summary>
        public static List<GameObject> FindAllSceneGameObjects() {
            var gos = GameObject.FindObjectsOfType<GameObject>();
            return new List<GameObject>(gos);
        }

        /// <summary>
        /// Return all game objects loaded in the game.
        /// </summary>
        public static List<GameObject> FindAllGameObjects() {
            var gos = Resources.FindObjectsOfTypeAll<GameObject>();
            return new List<GameObject>(gos);
        }

        /// <summary>
        /// From the list of game objects by their name, apply name filter to exclude or include some.
        /// </summary>
        /// <param name="gos">List of game objects you want to filter.</param>
        /// <param name="objectNameFilters">Name to filter by</param>
        /// <param name="isFilterRemovingThem">True to remove matching game objects, false to remove everything else</param>
        /// <param name="isPartial">True if ilter string is partial of the name, False if full name</param>
        /// <param name="isCaseSensitive">True if filter string needs case sensitive match, false if not</param>
        /// <returns>List of game objects filtered by name</returns>
        public static List<GameObject> FilterGameObjectsByName(
            IEnumerable<GameObject> gos,
            IEnumerable<string> objectNameFilters,
            bool isFilterRemovingThem = false,
            bool isPartial = false,
            bool isCaseSensitive = true) {
            var gosMatchingFilter = new List<GameObject>();
            var gosNotMatchingFilter = new List<GameObject>();
            foreach(var go in gos) {
                StringComparison comparison = isCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                bool isFilterMatched = false;
                foreach(var objectNameFilter in objectNameFilters) {
                    if(isPartial) {
                        if(go.name.Contains(objectNameFilter, comparison)) {
                            isFilterMatched = true;
                            break;
                        }
                    }
                    else {
                        if(string.Equals(go.name, objectNameFilter, comparison)) {
                            isFilterMatched = true;
                        }
                    }
                }
                if(isFilterMatched)
                    gosMatchingFilter.Add(go);
                else
                    gosNotMatchingFilter.Add(go);

            }
            if(isFilterRemovingThem)
                return gosNotMatchingFilter;
            else
                return gosMatchingFilter;
        }

        public static void DestroyGameObjects(IList<GameObject> gameObjects) {
            for(int i = 0;i < gameObjects.Count;++i) {
                GameObject.Destroy(gameObjects[i]);
            }
        }

        /// <summary>
        /// Set given game objects enabled or disabled, returns a list of objects that had changed state.
        /// </summary>
        public static List<GameObject> SetGameObjectsEnabled(IList<GameObject> gameObjects, bool isEnabled) {
            var list = new List<GameObject>();
            foreach(var go in gameObjects) {
                if(go.activeSelf != isEnabled) {
                    go.SetActive(isEnabled);
                    list.Add(go);
                }
            }
            return list;
        }

        public static void ReplaceLayerRecursively(GameObject o, int layerToReplace, int newLayer) {

            if(o.layer == layerToReplace) o.layer = newLayer;

            for(int i = 0;i < o.transform.childCount;i++) {
                Transform child = o.transform.GetChild(i);
                if(child == null) continue;
                if(child.gameObject == null) continue;
                if(child == o) continue;

                ReplaceLayerRecursively(child.gameObject, layerToReplace, newLayer);
            }
        }
        
        public static void SetLayerRecursively(GameObject o, int newLayer) {

            o.layer = newLayer;

            for(int i = 0;i < o.transform.childCount;i++) {
                Transform child = o.transform.GetChild(i);
                if(child == null) continue;
                if(child.gameObject == null) continue;
                if(child == o) continue;

                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        #region min max eval

        public static T Min<T>(Func<T, T, int> comparison, params T[] vals) {
            return _MinMax(vals, comparison, false);
        }

        public static T Max<T>(Func<T, T, int> comparison, params T[] vals) {
            return _MinMax(vals, comparison, true);
        }

        public static T _MinMax<T>(IEnumerable<T> vals, Func<T, T, int> comparison, bool max) {
            if(comparison == null) throw new ArgumentNullException("comparer");
            if(vals == null) throw new ArgumentNullException("vals");

            var itor = vals.GetEnumerator();

            if(itor.MoveNext()) {
                T ret = itor.Current;
                while(itor.MoveNext()) {
                    if((comparison.Invoke(ret, itor.Current) * (max ? 1 : -1)) > 0) {
                        ret = itor.Current;
                    }
                }

                return ret;
            }

            return default(T);
        }

        public static T Min<T>(params T[] vals_) where T : IComparable<T> {
            if(vals_ == null || vals_.Length == 0) return default(T);
            T ret = vals_[0];
            for(int i = 1;i < vals_.Length;++i) ret = (ret.CompareTo(vals_[i]) < 0 ? ret : vals_[i]);
            return ret;
        }

        public static T Max<T>(params T[] vals_) where T : IComparable<T> {
            if(vals_ == null || vals_.Length == 0) return default(T);
            T ret = vals_[0];
            for(int i = 1;i < vals_.Length;++i) ret = (ret.CompareTo(vals_[i]) > 0 ? ret : vals_[i]);
            return ret;
        }

        public static T Clamp<T>(T value_, T min_, T max_) where T : IComparable<T> {
            return value_.CompareTo(min_) < 0 ? min_ : value_.CompareTo(max_) > 0 ? max_ : value_;
        }

        #endregion

#if UNITY_EDITOR
        public static string GetScenePath_Editor(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName2 = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName2 == sceneName)
                {
                    return scenePath;
                }
            }

            return null;
        }
#endif

        /// <summary>
        /// Load a scene by name if not already loaded.
        /// </summary>
        public static void LoadScene(string sceneName, LoadSceneMode mode, bool isAsync)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.buildIndex != -1) return;

            if (isAsync)
            {
                SceneManager.LoadSceneAsync(sceneName, mode);
            }
            else
            {
                SceneManager.LoadScene(sceneName, mode);
            }
        }
    }
}
