using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using FRG.Core;

namespace VRC
{
    public class PerformanceDebugMenu : MonoBehaviour
    {
        public static PerformanceDebugMenu instance { get; private set; }

        // row 1
        [SerializeField] Text text_skybox;
        [SerializeField] Text text_canvases;
        [SerializeField] Text text_animations;
        [SerializeField] Text text_particleSystems;

        // row 2
        [SerializeField] Text text_allRenderers;
        [SerializeField] Text text_vsync;
        [SerializeField] Button[] buttons_AA;

        // row 3
        [SerializeField] Text text_lights;
        [SerializeField] Button[] buttons_maxPixelLights;

        // row 4
        [SerializeField] Text text_shadersOpaque;
        [SerializeField] Text text_shadersCutout;
        [SerializeField] Text text_shadersFade;
        [SerializeField] Text text_shadersTransparent;
        [SerializeField] Text text_shadersOther;

        // row 5
        [SerializeField] Text text_targetFps;

        // row 6
        [SerializeField] Text text_colliders;
        [SerializeField] Text text_physicsTimeStep;
        [SerializeField] Text text_cpuLevel;
        [SerializeField] Text text_gpuLevel;

        List<Renderer> allRenderersCached;
        List<Renderer>[] allRenderersCached_ShadersBlendMode = new List<Renderer>[5];
        List<Canvas> allCanvasesCached;
        List<ParticleSystem> allParticleSystemsCached;
        List<Animator> allAnimationsCached;
        List<GameObject> allSceneObjectsCached;
        List<Collider> allCollidersCached;
        List<Light> allLightsCached;
        Material savedSkybox;

        void Awake()
        {
            instance = this;
            Hide();
        }

        void OnEnable()
        {
            RefreshDisplayValues();
        }

        private void RefreshDisplayValues()
        {
            // row 1
            if (text_skybox != null)
                text_skybox.text = RenderSettings.skybox != null ? "ON" : "OFF";
            if (text_canvases != null)
                text_canvases.text = allCanvasesCached == null ? "ON" : "OFF";
            if (text_animations != null)
                text_animations.text = allAnimationsCached == null ? "ON" : "OFF";
            if (text_particleSystems != null)
                text_particleSystems.text = allParticleSystemsCached == null ? "ON" : "OFF";

            // row 2
            if (text_allRenderers != null)
                text_allRenderers.text = allRenderersCached == null ? "ON" : "OFF";
            RefreshDisplay_VSync();
            RefreshDisplay_AA();

            // row 3
            if (text_lights != null)
                text_lights.text = allLightsCached == null ? "ON" : "OFF";
            RefreshDisplay_PixelLights();

            // shader blend mode
            if (text_shadersOpaque != null)
                text_shadersOpaque.text = allRenderersCached_ShadersBlendMode[ShaderBlendMode_Opaque] == null ? "ON" : "OFF";
            if (text_shadersCutout != null)
                text_shadersCutout.text = allRenderersCached_ShadersBlendMode[ShaderBlendMode_Cutout] == null ? "ON" : "OFF";
            if (text_shadersFade != null)
                text_shadersFade.text = allRenderersCached_ShadersBlendMode[ShaderBlendMode_Fade] == null ? "ON" : "OFF";
            if (text_shadersTransparent != null)
                text_shadersTransparent.text = allRenderersCached_ShadersBlendMode[ShaderBlendMode_Transparent] == null ? "ON" : "OFF";
            if (text_shadersOther != null)
                text_shadersOther.text = allRenderersCached_ShadersBlendMode[ShaderBlendMode_Other] == null ? "ON" : "OFF";

            if (text_targetFps != null)
                text_targetFps.text = "(" + Application.targetFrameRate.ToString() + ")";

            // physics
            if (text_colliders != null)
                text_colliders.text = allCollidersCached == null ? "ON" : "OFF";
            if (text_physicsTimeStep != null)
                text_physicsTimeStep.text = "(" + (1f / Time.fixedDeltaTime).ToString("F2") + ")";

#if VRC_VR_WAVE
            if (text_cpuLevel != null && WaveVR_Render.Instance != null)
                text_cpuLevel.text = ((int)WaveVR_Render.Instance.cpuPerfLevel).ToString();
            if (text_gpuLevel != null && WaveVR_Render.Instance != null)
                text_gpuLevel.text = ((int)WaveVR_Render.Instance.gpuPerfLevel).ToString();
#endif
        }

        private void RefreshDisplay_VSync()
        {
            if (text_vsync != null)
            {
                if (QualitySettings.vSyncCount == 0)
                {
                    text_vsync.text = "OFF";
                }
                else if (QualitySettings.vSyncCount == 1)
                {
                    text_vsync.text = "ON";
                }
                else
                {
                    text_vsync.text = QualitySettings.vSyncCount.ToString();
                }
            }
        }

        private void RefreshDisplay_PixelLights()
        {
            if (buttons_maxPixelLights == null || buttons_maxPixelLights.Length < 9) return;

            int maxLights = QualitySettings.pixelLightCount;
            foreach (var button in buttons_maxPixelLights)
            {
                button.interactable = true;
            }
            buttons_maxPixelLights[maxLights].interactable = false;
        }

        private void RefreshDisplay_AA()
        {
            if (buttons_AA == null || buttons_AA.Length < 4) return;

            foreach (var button in buttons_AA)
            {
                button.interactable = true;
            }
            switch (QualitySettings.antiAliasing)
            {
                case 0: buttons_AA[0].interactable = false; break;
                case 2: buttons_AA[1].interactable = false; break;
                case 4: buttons_AA[2].interactable = false; break;
                case 8: buttons_AA[3].interactable = false; break;
                default:
                    Debug.LogError("Unhandled AA value, QualitySettings.antiAliasing: " + QualitySettings.antiAliasing);
                    break;
            }
        }

        public void SetAA(int val)
        {
            QualitySettings.antiAliasing = val;
            RefreshDisplayValues();
        }

        public void ToggleVsync()
        {
            if (QualitySettings.vSyncCount == 0)
            {
                QualitySettings.vSyncCount = 1;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
            }
            RefreshDisplayValues();
        }

        public void SetVsync(int val)
        {
            QualitySettings.vSyncCount = val;
            RefreshDisplayValues();
        }

        public void ToggleAllRenderers()
        {
            if (allRenderersCached != null)
            {
                foreach (var comp in allRenderersCached)
                {
                    if (comp != null)
                        comp.enabled = true;
                }
                allRenderersCached = null;
            }
            else
            {
                var gos = FRG.Core.Util.FindAllSceneGameObjects();
                var components = FRG.Core.Util.FindComponentsOnGameObjects<Renderer>(gos);
                foreach (var comp in components)
                {
                    if (comp != null)
                        comp.enabled = false;
                }
                allRenderersCached = new List<Renderer>(components);
            }

            RefreshDisplayValues();
        }

        public void ToggleAllLights()
        {
            if (allLightsCached != null)
            {
                foreach (var comp in allLightsCached)
                {
                    if (comp != null)
                        comp.enabled = true;
                }
                allLightsCached = null;
            }
            else
            {
                var gos = FRG.Core.Util.FindAllSceneGameObjects();
                var components = FRG.Core.Util.FindComponentsOnGameObjects<Light>(gos);
                foreach (var comp in components)
                {
                    if (comp != null)
                        comp.enabled = false;
                }
                allLightsCached = new List<Light>(components);
            }

            RefreshDisplayValues();
        }

        public void ToggleSkybox()
        {
            if (RenderSettings.skybox == null)
            {
                RenderSettings.skybox = savedSkybox;
            }
            else
            {
                savedSkybox = RenderSettings.skybox;
                RenderSettings.skybox = null;
            }

            RefreshDisplayValues();
        }

        public void ToggleAllCanvases()
        {
            if (allCanvasesCached != null)
            {
                foreach (var comp in allCanvasesCached)
                {
                    if (comp != null)
                        comp.gameObject.SetActive(true);
                }
                allCanvasesCached = null;
            }
            else
            {
                var gos = FRG.Core.Util.FindAllSceneGameObjects();
                var components = FRG.Core.Util.FindComponentsOnGameObjects<Canvas>(gos);
                Canvas thisCanvas = GetComponentInParent<Canvas>();
                foreach (var comp in components)
                {
                    // don't disable your own canvas or debug canvases
                    if (comp != null && comp != thisCanvas && !comp.name.Contains("Debug", true))
                        comp.gameObject.SetActive(false);
                }
                allCanvasesCached = new List<Canvas>(components);
            }

            RefreshDisplayValues();
        }

        public void SetTargetFps(int val)
        {
            Application.targetFrameRate = val;
            RefreshDisplayValues();
        }

        public void ToggleParticleSystems()
        {
            if (allParticleSystemsCached != null)
            {
                foreach (var comp in allParticleSystemsCached)
                {
                    if (comp != null)
                        comp.gameObject.SetActive(true);
                }
                allParticleSystemsCached = null;
            }
            else
            {
                var gos = FRG.Core.Util.FindAllSceneGameObjects();
                var components = FRG.Core.Util.FindComponentsOnGameObjects<ParticleSystem>(gos);
                foreach (var comp in components)
                {
                    if (comp != null)
                        comp.gameObject.SetActive(false);
                }
                allParticleSystemsCached = new List<ParticleSystem>(components);
            }
            if (text_particleSystems != null)
                text_particleSystems.text = allParticleSystemsCached == null ? "ON" : "OFF";

            RefreshDisplayValues();
        }

        public void ToggleAnimation()
        {
            if (allAnimationsCached != null)
            {
                foreach (var comp in allAnimationsCached)
                {
                    if (comp != null)
                        comp.enabled = true;
                }
                allAnimationsCached = null;
            }
            else
            {
                var gos = FRG.Core.Util.FindAllSceneGameObjects();
                var components = FRG.Core.Util.FindComponentsOnGameObjects<Animator>(gos);
                foreach (var comp in components)
                {
                    if (comp != null)
                        comp.enabled = false;
                }
                allAnimationsCached = new List<Animator>(components);
            }

            RefreshDisplayValues();
        }

        public void SetMaxPixelLights(int maxPixelLights)
        {
            // change lights that are strictly pixel to auto so we can adjust in quality
            var gos = FRG.Core.Util.FindAllSceneGameObjects();
            var components = FRG.Core.Util.FindComponentsOnGameObjects<Light>(gos);
            foreach (var comp in components)
            {
                if (comp != null && comp.renderMode == LightRenderMode.ForcePixel)
                    comp.renderMode = LightRenderMode.Auto;
            }

            QualitySettings.pixelLightCount = maxPixelLights;

            RefreshDisplayValues();
        }

        private const int ShaderBlendMode_Opaque = 0;
        private const int ShaderBlendMode_Cutout = 1;
        private const int ShaderBlendMode_Fade = 2;
        private const int ShaderBlendMode_Transparent = 3;
        private const int ShaderBlendMode_Other = 4;

        public void ToggleShaders(int blendModeValue)
        {
            if (allRenderersCached_ShadersBlendMode[blendModeValue] != null)
            {
                foreach (var comp in allRenderersCached_ShadersBlendMode[blendModeValue])
                {
                    if (comp != null)
                        comp.enabled = true;
                }
                allRenderersCached_ShadersBlendMode[blendModeValue] = null;
            }
            else
            {
                var gos = FRG.Core.Util.FindAllSceneGameObjects();
                var components = FRG.Core.Util.FindComponentsOnGameObjects<Renderer>(gos);
                foreach (var comp in components)
                {
                    if (comp == null || comp.sharedMaterial == null) continue;

                    bool isStandardShader = comp.sharedMaterial.shader.name.Contains("Standard");
                    bool hasBlendMode = comp.sharedMaterial.HasProperty("_Mode");
                    if (!isStandardShader && blendModeValue == ShaderBlendMode_Other)
                    {
                        comp.enabled = false;
                    }
                    else if (hasBlendMode && comp.sharedMaterial.GetFloat("_Mode") == blendModeValue)
                    {
                        comp.enabled = false;
                    }
                }
                allRenderersCached_ShadersBlendMode[blendModeValue] = new List<Renderer>(components);
            }

            RefreshDisplayValues();
        }

        public void ToggleShaders_Opaque()
        {
            ToggleShaders(ShaderBlendMode_Opaque);
        }

        public void ToggleShaders_Cutout()
        {
            ToggleShaders(ShaderBlendMode_Cutout);
        }

        public void ToggleShaders_Fade()
        {
            ToggleShaders(ShaderBlendMode_Fade);
        }

        public void ToggleShaders_Transparent()
        {
            ToggleShaders(ShaderBlendMode_Transparent);
        }

        public void ToggleShaders_Other()
        {
            ToggleShaders(ShaderBlendMode_Other);
        }

        public void TogglePhsycs_FixedUpdatesPerSecond(int fixedUpdatesPerSecond)
        {
            if (fixedUpdatesPerSecond <= 0)
                Time.fixedDeltaTime = float.MaxValue;
            else
                Time.fixedDeltaTime = 1f / fixedUpdatesPerSecond;

            RefreshDisplayValues();
        }

        public void TogglePhsycs_Colliders()
        {
            if (allCollidersCached != null)
            {
                foreach (var comp in allCollidersCached)
                {
                    if (comp != null)
                        comp.enabled = true;
                }
                allCollidersCached = null;
            }
            else
            {
                var gos = FRG.Core.Util.FindAllSceneGameObjects();
                var components = FRG.Core.Util.FindComponentsOnGameObjects<Collider>(gos);
                foreach (var comp in components)
                {
                    if (comp != null)
                        comp.enabled = false;
                }
                allCollidersCached = new List<Collider>(components);
            }

            RefreshDisplayValues();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #region WaveVR specific

        public void IncreaseCpuLevel()
        {
#if VRC_VR_WAVE
            if (WaveVR_Render.Instance == null) return;

            WaveVR_Render.Instance.cpuPerfLevel = GetIncreasedPerfLevel(WaveVR_Render.Instance.cpuPerfLevel);
            RefreshDisplayValues();
#endif
        }

        public void DecreaseCpuLevel()
        {
#if VRC_VR_WAVE
            if (WaveVR_Render.Instance == null) return;

            WaveVR_Render.Instance.cpuPerfLevel = GetDecreasedPerfLevel(WaveVR_Render.Instance.cpuPerfLevel);
            RefreshDisplayValues();
#endif
        }

        public void IncreaseGpuLevel()
        {
#if VRC_VR_WAVE
            if (WaveVR_Render.Instance == null) return;

            WaveVR_Render.Instance.gpuPerfLevel = GetIncreasedPerfLevel(WaveVR_Render.Instance.gpuPerfLevel);
            RefreshDisplayValues();
#endif
        }

        public void DecreaseGpuLevel()
        {
#if VRC_VR_WAVE
            if (WaveVR_Render.Instance == null) return;

            WaveVR_Render.Instance.gpuPerfLevel = GetDecreasedPerfLevel(WaveVR_Render.Instance.gpuPerfLevel);
            RefreshDisplayValues();
#endif
        }
#if VRC_VR_WAVE
        private WaveVR_Utils.WVR_PerfLevel GetIncreasedPerfLevel(WaveVR_Utils.WVR_PerfLevel lvl)
        {
            switch (lvl)
            {
                case WaveVR_Utils.WVR_PerfLevel.System: return WaveVR_Utils.WVR_PerfLevel.Minimum;
                case WaveVR_Utils.WVR_PerfLevel.Minimum: return WaveVR_Utils.WVR_PerfLevel.Medium;
                case WaveVR_Utils.WVR_PerfLevel.Medium: return WaveVR_Utils.WVR_PerfLevel.Maximum;
                case WaveVR_Utils.WVR_PerfLevel.Maximum: return WaveVR_Utils.WVR_PerfLevel.Maximum;
                default:
                    Debug.LogError("Switch enum value not handled: " + lvl.ToString());
                    return lvl;
            }
        }

        private WaveVR_Utils.WVR_PerfLevel GetDecreasedPerfLevel(WaveVR_Utils.WVR_PerfLevel lvl)
        {
            switch (lvl)
            {
                case WaveVR_Utils.WVR_PerfLevel.System: return WaveVR_Utils.WVR_PerfLevel.System;
                case WaveVR_Utils.WVR_PerfLevel.Minimum: return WaveVR_Utils.WVR_PerfLevel.System;
                case WaveVR_Utils.WVR_PerfLevel.Medium: return WaveVR_Utils.WVR_PerfLevel.Minimum;
                case WaveVR_Utils.WVR_PerfLevel.Maximum: return WaveVR_Utils.WVR_PerfLevel.Medium;
                default:
                    Debug.LogError("Switch enum value not handled: " + lvl.ToString());
                    return lvl;
            }
        }
#endif
#endregion
    }
}
