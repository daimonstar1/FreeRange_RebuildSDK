using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FRG.Core
{
    public static class WorkSceneHelper
    {
        [MenuItem("GameObject/FRG/Work Scene Helper/Move all Static ParticleSystems here", false, 0)]
        static void MoveAllParticleSystems()
        {
            var go = Selection.activeTransform;
            if (go == null) return;

            var gos = Util.FindAllSceneGameObjects();
            var allParticleSystems = Util.FindComponentsOnGameObjects<ParticleSystem>(gos);

            var staticParticleSystems = new List<ParticleSystem>();
            foreach (var ps in allParticleSystems)
            {
                if (ps.gameObject.isStatic)
                {
                    staticParticleSystems.Add(ps);
                }
            }

            var psGos = new List<GameObject>();
            foreach (var ps in staticParticleSystems)
            {
                ps.transform.parent = go;
                psGos.Add(ps.gameObject);
            }

            Selection.objects = psGos.ToArray();

            Debug.Log("Moved " + staticParticleSystems.Count + " ParticleSystems to " + go.name);
        }

        [MenuItem("GameObject/FRG/Work Scene Helper/Move all Static Baked Lights here", false, 1)]
        static void MoveAllBakedLights()
        {
            var go = Selection.activeTransform;
            if (go == null) return;

            var gos = Util.FindAllSceneGameObjects();
            var allLights = Util.FindComponentsOnGameObjects<Light>(gos);

            var bakedLights = new List<Light>();
            foreach (var light in allLights)
            {
                if (light.lightmapBakeType == LightmapBakeType.Baked && light.gameObject.isStatic)
                {
                    bakedLights.Add(light);
                }
            }

            var lightGos = new List<GameObject>();
            foreach (var light in bakedLights)
            {
                light.transform.parent = go;
                lightGos.Add(light.gameObject);
            }

            Selection.objects = lightGos.ToArray();

            Debug.Log("Moved " + bakedLights.Count + " Lights to " + go.name);
        }

        [MenuItem("GameObject/FRG/Work Scene Helper/Move all Static Mixed Lights here", false, 1)]
        static void MoveAllMixedLights()
        {
            var go = Selection.activeTransform;
            if (go == null) return;

            var gos = Util.FindAllSceneGameObjects();
            var allLights = Util.FindComponentsOnGameObjects<Light>(gos);

            var bakedLights = new List<Light>();
            foreach (var light in allLights)
            {
                if (light.lightmapBakeType == LightmapBakeType.Mixed && light.gameObject.isStatic)
                {
                    bakedLights.Add(light);
                }
            }

            var lightGos = new List<GameObject>();
            foreach (var light in bakedLights)
            {
                light.transform.parent = go;
                lightGos.Add(light.gameObject);
            }

            Selection.objects = lightGos.ToArray();

            Debug.Log("Moved " + bakedLights.Count + " Lights to " + go.name);
        }
    }
}