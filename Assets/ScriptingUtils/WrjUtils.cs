﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrj
{
    public class Utils : MonoBehaviour
    {
        // Returns a component of Type T by finding the existing one, or by instantiating one if not found.
        public static T EnsureComponent<T>(GameObject go) where T : Component
        {
            if (!go.GetComponent<T>())
            {
                go.AddComponent<T>();
            }
            return go.GetComponent<T>();
        }

        // Swap items
        public static void Switcheroo<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static void SetLayerRecursive(GameObject go, string layer)
        {
            if (go == null)
            {
                return;
            }

            go.layer = LayerMask.NameToLayer(layer);

            foreach (Transform t in go.transform)
            {
                SetLayerRecursive(t.gameObject, layer);
            }
        }

        public static float GetPositiveAngle(float angle)
        {
            while (angle <= 0)
            {
                angle += 360;
            }
            return angle % 360;
        }
        public static Vector3 GetPositiveAngle(Vector3 angles)
        {
            return new Vector3(GetPositiveAngle(angles.x), GetPositiveAngle(angles.y), GetPositiveAngle(angles.z));
        }
        // Linear remap; Simplified interface, rather than solving the Lerp/InverseLerp puzzle every time.
        public static float Remap(float value, float sourceMin, float sourceMax, float destMin, float destMax)
        {
            return Mathf.Lerp(destMin, destMax, Mathf.InverseLerp(sourceMin, sourceMax, value));
        }

        // Build a curve by tracing points on lines
        // https://upload.wikimedia.org/wikipedia/commons/3/3d/B%C3%A9zier_2_big.gif
        public static Vector3[] QuadraticBezierCurve(Vector3 origin, Vector3 influence, Vector3 destination, int pointCount, bool throughInfluence = false)
        {
            Vector3[] result = new Vector3[pointCount];
            if (pointCount == 0)
                return result;

            if (throughInfluence)
            {
                influence = influence * 2 - (origin + destination) / 2;
            }

            result[0] = origin;
            for (int i = 1; i < pointCount - 1; i++)
            {
                float t = (1f / pointCount) * i;
                Vector3 point1 = Vector3.Lerp(origin, influence, t);
                Vector3 point2 = Vector3.Lerp(influence, destination, t);
                result[i] = Vector3.Lerp(point1, point2, t);
            }
            result[pointCount - 1] = destination;

            return result;
        }
        public static Vector3[] CubicBezierCurve(Vector3 origin, Vector3 influenceA, Vector3 influenceB, Vector3 destination, int pointCount)
        {
            Vector3[] result = new Vector3[pointCount];
            if (pointCount == 0)
                return result;

            result[0] = origin;
            for (int i = 1; i < pointCount - 1; i++)
            {
                float t = (1f / pointCount) * i;
                Vector3 point1 = Vector3.Lerp(origin, influenceA, t);
                Vector3 point2 = Vector3.Lerp(influenceA, influenceB, t);
                Vector3 point3 = Vector3.Lerp(influenceB, destination, t);

                Vector3 point4 = Vector3.Lerp(point1, point2, t);
                Vector3 point5 = Vector3.Lerp(point2, point3, t);

                result[i] = Vector3.Lerp(point4, point5, t);
            }
            result[pointCount - 1] = destination;

            return result;
        }
        // Coroutine list management stuff...
        public static Utils wrjInstance;
        private List<MapToCurve.MappedCurvePlayer> m_CoroList;
        private void InitializeCoroList()
        {
            m_CoroList = new List<MapToCurve.MappedCurvePlayer>();
        }
        private void AddToCoroList(MapToCurve.MappedCurvePlayer mcp)
        {
            m_CoroList.Add(mcp);
        }
        private void RemoveFromCoroList()
        {
            m_CoroList.RemoveAll(x => x.coroutine == null);
        }
        private void CancelAll()
        {
            foreach (MapToCurve.MappedCurvePlayer mcp in m_CoroList)
            {
                StopCoroutine(mcp.coroutine);
            }
            RemoveFromCoroList();
        }
        private void CancelByTransform(Transform t)
        {
            foreach (MapToCurve.MappedCurvePlayer mcp in m_CoroList)
            {
                if (mcp.transform == t)
                {
                    StopCoroutine(mcp.coroutine);
                }
            }
            RemoveFromCoroList();
        }

        // Get a value as plotted on an Animation Curve
        // Used like Mathf.Lerp, except not linear
        // Handy functions to manipulate transforms/audio over time.
        // Example:
        //      WrjUtils.MapToCurve map = new WrjUtils.MapToCurve();
        //      map.ScaleTransform(transform, transform.localScale * .25f, 5, onDone: SomeMethodThatReturnsVoid, pingPong: 3);

        public class MapToCurve
        {
            AnimationCurve curve;
            public delegate void OnDone();

            public static MapToCurve Linear = new MapToCurve(AnimationCurve.Linear(0, 0, 1, 1));
            public static MapToCurve Ease = new MapToCurve(AnimationCurve.EaseInOut(0, 0, 1, 1));
            public static MapToCurve EaseIn = new MapToCurve(AnimationCurve.EaseInOut(0, 0, 2, 2));

            public class MappedCurvePlayer
            {
                public Coroutine coroutine;
                public Transform transform;
                public void Stop()
                {
                    if (coroutine != null)
                    {
                        wrjInstance.StopCoroutine(coroutine);
                        wrjInstance.RemoveFromCoroList();
                    }
                }
            }
            public MapToCurve()
            {
                curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            }
            public MapToCurve(AnimationCurve c)
            {
                curve = c;
            }

            public static void StopAll()
            {
                if (wrjInstance != null)
                    wrjInstance.CancelAll();
            }
            public static void StopAllOnTransform(Transform tform)
            {
                if (wrjInstance != null)
                    wrjInstance.CancelByTransform(tform);
            }

            public float Lerp(float a, float b, float time)
            {
                return Mathf.LerpUnclamped(a, b, curve.Evaluate(time));
            }
            public Vector3 Lerp(Vector3 a, Vector3 b, float time)
            {
                return Vector3.LerpUnclamped(a, b, curve.Evaluate(time));
            }
            public Quaternion Lerp(Quaternion a, Quaternion b, float time)
            {
                return Quaternion.LerpUnclamped(a, b, curve.Evaluate(time));
            }
            public Color Lerp(Color a, Color b, float time)
            {
                return Color.Lerp(a, b, curve.Evaluate(time));
            }
            public float MirrorLerp(float a, float b, float time)
            {
                float t = Remap(time, -1, 2, 2, -1);
                return Mathf.LerpUnclamped(a, b, Remap(curve.Evaluate(t), -1, 2, 2, -1));
            }
            public Vector3 MirrorLerp(Vector3 a, Vector3 b, float time)
            {
                float t = Remap(time, -1, 2, 2, -1);
                return Vector3.LerpUnclamped(a, b, Remap(curve.Evaluate(t), -1, 2, 2, -1));
            }
            public Quaternion MirrorLerp(Quaternion a, Quaternion b, float time)
            {
                float t = Remap(time, -1, 2, 2, -1);
                return Quaternion.LerpUnclamped(a, b, Remap(curve.Evaluate(t), -1, 2, 2, -1));
            }
            public Color MirrorLerp(Color a, Color b, float time)
            {
                float t = Remap(time, -1, 2, 2, -1);
                return Color.LerpUnclamped(a, b, Remap(curve.Evaluate(t), -1, 2, 2, -1));
            }
            public static float Lerp(AnimationCurve c, float a, float b, float time)
            {
                return Mathf.LerpUnclamped(a, b, c.Evaluate(time));
            }
            public static float MirrorLerp(AnimationCurve c, float a, float b, float time)
            {
                float t = Remap(time, -1, 2, 2, -1);
                return Remap(Mathf.LerpUnclamped(a, b, c.Evaluate(t)), -1, 2, 2, -1);
            }

            // Period-based Manipulation Coroutines...
            public MappedCurvePlayer Scale(Transform tform, Vector3 to, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = tform;
                mcp.coroutine = UtilObject().StartCoroutine(ScaleLocal(mcp, tform, to, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            private IEnumerator ScaleLocal(MappedCurvePlayer mcp, Transform tform, Vector3 to, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                float elapsedTime = 0;
                Vector3 from = tform.localScale;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (tform == null)
                    {
                        StopAllOnTransform(tform);
                        yield return null;
                    }
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        tform.localScale = MirrorLerp(from, to, scrubPos);
                    }
                    else
                    {
                        tform.localScale = Lerp(from, to, scrubPos);
                    }
                }
                tform.localScale = to;
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(ScaleLocal(mcp, tform, from, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(ScaleLocal(mcp, tform, from, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    tform.localScale = from;
                    mcp.coroutine = UtilObject().StartCoroutine(ScaleLocal(mcp, tform, to, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }

            public MappedCurvePlayer Move(Transform tform, Vector3 to, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = tform;
                mcp.coroutine = UtilObject().StartCoroutine(MoveLocal(mcp, tform, to, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            private IEnumerator MoveLocal(MappedCurvePlayer mcp, Transform tform, Vector3 to, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                float elapsedTime = 0;
                Vector3 from = tform.localPosition;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (tform == null)
                    {
                        StopAllOnTransform(tform);
                        yield return null;
                    }
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        tform.localPosition = MirrorLerp(from, to, scrubPos);
                    }
                    else
                    {
                        tform.localPosition = Lerp(from, to, scrubPos);
                    }
                }
                tform.localPosition = to;
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(MoveLocal(mcp, tform, from, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(MoveLocal(mcp, tform, from, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    tform.localPosition = from;
                    mcp.coroutine = UtilObject().StartCoroutine(MoveLocal(mcp, tform, to, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }

            public MappedCurvePlayer MoveWorld(Transform tform, Vector3 to, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = tform;
                mcp.coroutine = UtilObject().StartCoroutine(MoveWorldspace(mcp, tform, to, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            private IEnumerator MoveWorldspace(MappedCurvePlayer mcp, Transform tform, Vector3 to, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                float elapsedTime = 0;
                Vector3 from = tform.position;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (tform == null)
                    {
                        StopAllOnTransform(tform);
                        yield return null;
                    }
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        tform.position = MirrorLerp(from, to, scrubPos);
                    }
                    else
                    {
                        tform.position = Lerp(from, to, scrubPos);
                    }
                }
                tform.position = to;
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(MoveWorldspace(mcp, tform, from, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(MoveWorldspace(mcp, tform, from, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    tform.position = from;
                    mcp.coroutine = UtilObject().StartCoroutine(MoveWorldspace(mcp, tform, to, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }

            public MappedCurvePlayer MoveAlongPath(Transform tform, BezierPath path, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, bool inverse = false, bool align = false, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = tform;
                mcp.coroutine = UtilObject().StartCoroutine(MovePath(mcp, tform, path, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, inverse, align, onDone));
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            private IEnumerator MovePath(MappedCurvePlayer mcp, Transform tform, BezierPath path, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, bool inverse, bool align, OnDone onDone)
            {
                float elapsedTime = 0;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (tform == null)
                    {
                        StopAllOnTransform(tform);
                        yield return null;
                    }
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = inverse ? Remap(elapsedTime, 0, duration, 1, 0) : Remap(elapsedTime, 0, duration, 0, 1);
                    Vector3 look = Vector3.zero;
                    if (mirrorCurve)
                    {
                        tform.position = path.GetPointOnCurve(MirrorLerp(0, 1, scrubPos), ref look);
                    }
                    else
                    {
                        tform.position = path.GetPointOnCurve(Lerp(0, 1, scrubPos), ref look);
                    }
                    if (align && look != tform.position)
                        tform.rotation = Quaternion.LookRotation(tform.position - look);
                }
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(MovePath(mcp, tform, path, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, !inverse, align, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(MovePath(mcp, tform, path, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, !inverse, align, onDone));
                }
                else if (loop > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(MovePath(mcp, tform, path, duration, mirrorCurve, --loop, 0, 0, useTimeScale, inverse, align, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }
            public MappedCurvePlayer Rotate(Transform tform, Vector3 eulerTo, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, bool shortestPath = true, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = tform;
                if (shortestPath)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(RotateLocalQuaternionLerp(mcp, tform, tform.rotation, Quaternion.Euler(eulerTo.x, eulerTo.y, eulerTo.z), duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                }
                else
                {
                    mcp.coroutine = UtilObject().StartCoroutine(RotateLocal(mcp, tform, tform.localEulerAngles, eulerTo, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                }
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            private IEnumerator RotateLocalQuaternionLerp(MappedCurvePlayer mcp, Transform tform, Quaternion from, Quaternion to, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                float elapsedTime = 0;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (tform == null)
                    {
                        StopAllOnTransform(tform);
                        yield return null;
                    }
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        tform.localRotation = MirrorLerp(from, to, scrubPos);
                    }
                    else
                    {
                        tform.localRotation = Lerp(from, to, scrubPos);
                    }
                }
                tform.localRotation = to;
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(RotateLocalQuaternionLerp(mcp, tform, to, from, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(RotateLocalQuaternionLerp(mcp, tform, to, from, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    tform.localRotation = from;
                    mcp.coroutine = UtilObject().StartCoroutine(RotateLocalQuaternionLerp(mcp, tform, from, to, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }
            private IEnumerator RotateLocal(MappedCurvePlayer mcp, Transform tform, Vector3 from, Vector3 to, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                float elapsedTime = 0;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (tform == null)
                    {
                        StopAllOnTransform(tform);
                        yield return null;
                    }
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        tform.localEulerAngles = MirrorLerp(from, to, scrubPos);
                    }
                    else
                    {
                        tform.localEulerAngles = Lerp(from, to, scrubPos);
                    }
                }
                tform.localEulerAngles = to;
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(RotateLocal(mcp, tform, to, from, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(RotateLocal(mcp, tform, to, from, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    tform.localEulerAngles = from;
                    mcp.coroutine = UtilObject().StartCoroutine(RotateLocal(mcp, tform, from, to, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }
            public MappedCurvePlayer FadeAudio(AudioSource audioSource, float targetVol, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = audioSource.transform;
                mcp.coroutine = UtilObject().StartCoroutine(Fade(mcp, audioSource, targetVol, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            private IEnumerator Fade(MappedCurvePlayer mcp, AudioSource audioSource, float targetVol, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                float initVol = audioSource.volume;
                float elapsedTime = 0;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (audioSource == null)
                    {
                        yield return null;
                    }
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        audioSource.volume = MirrorLerp(initVol, targetVol, scrubPos);
                    }
                    else
                    {
                        audioSource.volume = Lerp(initVol, targetVol, scrubPos);
                    }
                }
                audioSource.volume = targetVol;
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(Fade(mcp, audioSource, initVol, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(Fade(mcp, audioSource, initVol, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    audioSource.volume = initVol;
                    mcp.coroutine = UtilObject().StartCoroutine(Fade(mcp, audioSource, targetVol, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }

            public MappedCurvePlayer CrossFadeAudio(AudioSource from, AudioSource to, float targetVol, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = from.transform;
                mcp.coroutine = UtilObject().StartCoroutine(CrossFade(mcp, from, to, targetVol, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            private IEnumerator CrossFade(MappedCurvePlayer mcp, AudioSource from, AudioSource to, float targetVol, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                to.volume = 0;
                float initB = from.volume;
                float elapsedTime = 0;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (to == null || from == null)
                    {
                        yield return null;
                    }
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        to.volume = MirrorLerp(0, targetVol, scrubPos);
                        from.volume = MirrorLerp(initB, 0, scrubPos);
                    }
                    else
                    {
                        to.volume = Lerp(0, targetVol, scrubPos);
                        from.volume = Lerp(initB, 0, scrubPos);
                    }
                }
                to.volume = targetVol;
                from.volume = 0;
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(CrossFade(mcp, to, from, targetVol, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(CrossFade(mcp, to, from, targetVol, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    from.volume = initB;
                    to.volume = 0;
                    mcp.coroutine = UtilObject().StartCoroutine(CrossFade(mcp, from, to, targetVol, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }

            public MappedCurvePlayer FadeAlpha(Transform tform, float to, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = tform;
                mcp.coroutine = UtilObject().StartCoroutine(LerpAlpha(mcp, tform, to, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            private IEnumerator LerpAlpha(MappedCurvePlayer mcp, Transform tform, float to, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                float elapsedTime = 0;
                Material mat = tform.GetComponent<Renderer>().material;
                float from = mat.GetColor("_Color").a;
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.renderQueue = 3000;
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (tform == null)
                    {
                        StopAllOnTransform(tform);
                        yield return null;
                    }
                    Color color = mat.GetColor("_Color");
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        color.a = MirrorLerp(from, to, scrubPos);
                    }
                    else
                    {
                        color.a = Lerp(from, to, scrubPos);
                    }
                    mat.SetColor("_Color", color);
                }
                Color finalColor = mat.GetColor("_Color");
                finalColor.a = to;
                mat.SetColor("_Color", finalColor);
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(LerpAlpha(mcp, tform, from, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(LerpAlpha(mcp, tform, from, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    finalColor.a = from;
                    mat.SetColor("_Color", finalColor);
                    mcp.coroutine = UtilObject().StartCoroutine(LerpAlpha(mcp, tform, to, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }

            public MappedCurvePlayer ChangeColor(Transform tform, Color to, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, OnDone onDone = null)
            {
                MappedCurvePlayer mcp = new MappedCurvePlayer();
                mcp.transform = tform;
                mcp.coroutine = UtilObject().StartCoroutine(LerpColor(mcp, tform, to, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone));
                UtilObject().AddToCoroList(mcp);
                return mcp;
            }
            // Beingn careful not to impact alpha, so this can be used simultaneously with ChangAlpha()
            private IEnumerator LerpColor(MappedCurvePlayer mcp, Transform tform, Color to, float duration, bool mirrorCurve, int loop, int pingPong, int mirrorPingPong, bool useTimeScale, OnDone onDone)
            {
                float elapsedTime = 0;
                Material mat = tform.GetComponent<Renderer>().material;
                Color from = mat.GetColor("_Color");
                while (elapsedTime < duration)
                {
                    yield return new WaitForEndOfFrame();
                    if (tform == null)
                    {
                        StopAllOnTransform(tform);
                        yield return null;
                    }
                    Color color = mat.GetColor("_Color");
                    float desiredDelta = useTimeScale ? Time.deltaTime : Time.unscaledDeltaTime;
                    elapsedTime += desiredDelta;
                    float scrubPos = Remap(elapsedTime, 0, duration, 0, 1);
                    if (mirrorCurve)
                    {
                        color = MirrorLerp(from, to, scrubPos);
                    }
                    else
                    {
                        color = Lerp(from, to, scrubPos);
                    }
                    mat.SetColor("_Color", new Color(color.r, color.g, color.b, mat.GetColor("_Color").a));
                }
                Color finalColor = to;
                finalColor.a = mat.GetColor("_Color").a;
                mat.SetColor("_Color", finalColor);
                if (pingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(LerpColor(mcp, tform, from, duration, mirrorCurve, 0, --pingPong, 0, useTimeScale, onDone));
                }
                else if (mirrorPingPong > 0)
                {
                    mcp.coroutine = UtilObject().StartCoroutine(LerpColor(mcp, tform, from, duration, !mirrorCurve, 0, 0, --mirrorPingPong, useTimeScale, onDone));
                }
                else if (loop > 0)
                {
                    mat.SetColor("_Color", from);
                    mcp.coroutine = UtilObject().StartCoroutine(LerpColor(mcp, tform, to, duration, mirrorCurve, --loop, 0, 0, useTimeScale, onDone));
                }
                else
                {
                    CoroutineComplete(onDone);
                }
            }

            // Matches the position scale and rotation of a sibling transform.
            public MappedCurvePlayer[] MatchSibling(Transform tform, Transform toTform, float duration, bool mirrorCurve = false, int loop = 0, int pingPong = 0, int mirrorPingPong = 0, bool useTimeScale = false, OnDone onDone = null)
            {
                MappedCurvePlayer[] mcpList = new MappedCurvePlayer[3];
                mcpList[0] = Scale(tform, toTform.localScale, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, onDone);
                mcpList[1] = Move(tform, toTform.localPosition, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, null);
                mcpList[2] = Rotate(tform, toTform.localEulerAngles, duration, mirrorCurve, loop, pingPong, mirrorPingPong, useTimeScale, true, null);
                return mcpList;
            }

            // Called when a coroutine is completed. Executes the OnDone method if necessary, and resets default values.
            private void CoroutineComplete(OnDone OnDoneMethod)
            {
                if (OnDoneMethod != null)
                {
                    OnDoneMethod();
                    OnDoneMethod = null;
                }
                wrjInstance.RemoveFromCoroList();
                OnDoneMethod = null;
            }

            // Make sure there's a game object with a WrjUtils component, to run coroutines.
            private Utils UtilObject()
            {
                if (wrjInstance == null)
                {
                    GameObject go = new GameObject("_WrjUtilObject");
                    DontDestroyOnLoad(go);
                    wrjInstance = go.AddComponent<Utils>();
                    wrjInstance.InitializeCoroList();
                }
                return wrjInstance;
            }
        }
    }
    [System.Serializable]
    public class WeightedGameObjects
    {
        public WeightedElement[] objectList;

        public GameObject GetRandom()
        {
            List<int> allOptions = new List<int>();

            for (int i = 0; i < objectList.Length; i++)
            {
                for (int j = 0; j < objectList[i].weight; j++)
                {
                    allOptions.Add(i);
                }
            }
            int weightedRandomIndex = allOptions[UnityEngine.Random.Range(0, allOptions.Count)];

            return objectList[weightedRandomIndex].element;
        }
        [System.Serializable]
        public struct WeightedElement
        {
            public GameObject element;
            public int weight;
        }
    }
}


