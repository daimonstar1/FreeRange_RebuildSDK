using System.Text;
using UnityEngine;

namespace FRG.Core
{
    public class QualitySettingsData
    {
        public string name;
        public ColorSpace activeColorSpace;
        public AnisotropicFiltering anisotropicFiltering;
        public int antiAliasing;
        public int asyncUploadBufferSize;
        public int asyncUploadTimeSlice;
        public bool billboardsFaceCameraPosition;
        public BlendWeights blendWeights;
        public ColorSpace desiredColorSpace;
        public float lodBias;
        public int masterTextureLimit;
        public int maximumLODLevel;
        public int maxQueuedFrames;
        public int particleRaycastBudget;
        public int pixelLightCount;
        public bool realtimeReflectionProbes;
        public float shadowCascade2Split;
        public Vector3 shadowCascade4Split;
        public int shadowCascades;
        public float shadowDistance;
        public float shadowNearPlaneOffset;
        public ShadowProjection shadowProjection;
        public bool softVegetation;
        public int vSyncCount;

        public static QualitySettingsData FromUnity()
        {
            return new QualitySettingsData();
        }

        public QualitySettingsData(QualitySettingsData from)
        {
            activeColorSpace = from.activeColorSpace;
            anisotropicFiltering = from.anisotropicFiltering;
            antiAliasing = from.antiAliasing;
            asyncUploadBufferSize = from.asyncUploadBufferSize;
            asyncUploadTimeSlice = from.asyncUploadTimeSlice;
            billboardsFaceCameraPosition = from.billboardsFaceCameraPosition;
            blendWeights = from.blendWeights;
            lodBias = from.lodBias;
            masterTextureLimit = from.masterTextureLimit;
            maximumLODLevel = from.maximumLODLevel;
            maxQueuedFrames = from.maxQueuedFrames;
            particleRaycastBudget = from.particleRaycastBudget;
            pixelLightCount = from.pixelLightCount;
            realtimeReflectionProbes = from.realtimeReflectionProbes;
            shadowCascade2Split = from.shadowCascade2Split;
            shadowCascade4Split = from.shadowCascade4Split;
            shadowCascades = from.shadowCascades;
            shadowDistance = from.shadowDistance;
            shadowNearPlaneOffset = from.shadowNearPlaneOffset;
            shadowProjection = from.shadowProjection;
            softVegetation = from.softVegetation;
            vSyncCount = from.vSyncCount;
        }

        private QualitySettingsData()
        {
            name = QualitySettings.names[QualitySettings.GetQualityLevel()];
            activeColorSpace = QualitySettings.activeColorSpace;
            anisotropicFiltering = QualitySettings.anisotropicFiltering;
            antiAliasing = QualitySettings.antiAliasing;
            asyncUploadBufferSize = QualitySettings.asyncUploadBufferSize;
            asyncUploadTimeSlice = QualitySettings.asyncUploadTimeSlice;
            billboardsFaceCameraPosition = QualitySettings.billboardsFaceCameraPosition;
            blendWeights = QualitySettings.blendWeights;
            lodBias = QualitySettings.lodBias;
            masterTextureLimit = QualitySettings.masterTextureLimit;
            maximumLODLevel = QualitySettings.maximumLODLevel;
            maxQueuedFrames = QualitySettings.maxQueuedFrames;
            particleRaycastBudget = QualitySettings.particleRaycastBudget;
            pixelLightCount = QualitySettings.pixelLightCount;
            realtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
            shadowCascade2Split = QualitySettings.shadowCascade2Split;
            shadowCascade4Split = QualitySettings.shadowCascade4Split;
            shadowCascades = QualitySettings.shadowCascades;
            shadowDistance = QualitySettings.shadowDistance;
            shadowNearPlaneOffset = QualitySettings.shadowNearPlaneOffset;
            shadowProjection = QualitySettings.shadowProjection;
            softVegetation = QualitySettings.softVegetation;
            vSyncCount = QualitySettings.vSyncCount;
        }

        public void ApplyToUnity()
        {
            QualitySettings.anisotropicFiltering = anisotropicFiltering;
            Debug.Log("ApplyToUnity QualitySettings.antiAliasing = "+antiAliasing);
            QualitySettings.antiAliasing = antiAliasing;
            QualitySettings.asyncUploadBufferSize = asyncUploadBufferSize;
            QualitySettings.asyncUploadTimeSlice = asyncUploadTimeSlice;
            QualitySettings.billboardsFaceCameraPosition = billboardsFaceCameraPosition;
            QualitySettings.blendWeights = blendWeights;
            QualitySettings.lodBias = lodBias;
            QualitySettings.masterTextureLimit = masterTextureLimit;
            QualitySettings.maximumLODLevel = maximumLODLevel;
            QualitySettings.maxQueuedFrames = maxQueuedFrames;
            QualitySettings.particleRaycastBudget = particleRaycastBudget;
            QualitySettings.pixelLightCount = pixelLightCount;
            QualitySettings.realtimeReflectionProbes = realtimeReflectionProbes;
            QualitySettings.shadowCascade2Split = shadowCascade2Split;
            QualitySettings.shadowCascade4Split = shadowCascade4Split;
            QualitySettings.shadowCascades = shadowCascades;
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.shadowNearPlaneOffset = shadowNearPlaneOffset;
            QualitySettings.shadowProjection = shadowProjection;
            QualitySettings.softVegetation = softVegetation;
            QualitySettings.vSyncCount = vSyncCount;
        }

        public override string ToString()
        {
            return ToMultilineString();
        }

        public string ToMultilineString(string lineSeparator = "\n")
        {
            string innerLineSeparator = lineSeparator + "    ";
            return "QualitySettingsData:"
                + innerLineSeparator + "name:" + name.ToString()
                + innerLineSeparator + "activeColorSpace:" + activeColorSpace.ToString()
                + innerLineSeparator + "anisotropicFiltering:" + anisotropicFiltering.ToString()
                + innerLineSeparator + "antiAliasing:" + antiAliasing.ToString()
                + innerLineSeparator + "asyncUploadBufferSize:" + asyncUploadBufferSize.ToString()
                + innerLineSeparator + "asyncUploadTimeSlice:" + asyncUploadTimeSlice.ToString()
                + innerLineSeparator + "billboardsFaceCameraPosition:" + billboardsFaceCameraPosition.ToString()
                + innerLineSeparator + "blendWeights:" + blendWeights.ToString()
                + innerLineSeparator + "lodBias:" + lodBias.ToString()
                + innerLineSeparator + "masterTextureLimit:" + masterTextureLimit.ToString()
                + innerLineSeparator + "maximumLODLevel:" + maximumLODLevel.ToString()
                + innerLineSeparator + "maxQueuedFrames:" + maxQueuedFrames.ToString()
                + innerLineSeparator + "particleRaycastBudget:" + particleRaycastBudget.ToString()
                + innerLineSeparator + "pixelLightCount:" + pixelLightCount.ToString()
                + innerLineSeparator + "realtimeReflectionProbes:" + realtimeReflectionProbes.ToString()
                + innerLineSeparator + "shadowCascade2Split:" + shadowCascade2Split.ToString()
                + innerLineSeparator + "shadowCascade4Split:" + shadowCascade4Split.ToString()
                + innerLineSeparator + "shadowCascades:" + shadowCascades.ToString()
                + innerLineSeparator + "shadowDistance:" + shadowDistance.ToString()
                + innerLineSeparator + "shadowNearPlaneOffset:" + shadowNearPlaneOffset.ToString()
                + innerLineSeparator + "shadowProjection:" + shadowProjection.ToString()
                + innerLineSeparator + "softVegetation:" + softVegetation.ToString()
                + innerLineSeparator + "vSyncCount:" + vSyncCount.ToString();
        }
    }
}