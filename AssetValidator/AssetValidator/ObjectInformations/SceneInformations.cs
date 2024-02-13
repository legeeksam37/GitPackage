using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace AssetValidator
{

    public class SceneInformations
    {
        public string renderPipelineAsset { get; set; }

        public MixedLightingMode lightMode { get; set; }
        public LightmapsMode directionalMode { get; set; }

        public List<LightmapInformations> listLightmapsInformations { get; set; }
        public float sumLightmapsLength { get; set; }
        public float sumRealLightmapsLength { get; set; }
        public int nbLightmaps { get; set; }
        public string pathLightmaps { get; set; }

        public int presenceOfRealtimeLight { get; set; }
        public List<Light> lightsRealtime { get; set; }

        public bool presenceOfLightSettings { get; set; }

        public int globalMaterialsCount { get; set; }

        public SceneInformations(int _globalMaterialsCount)
        {
            globalMaterialsCount = _globalMaterialsCount;
        }

        public void CheckScene()
        {
            sumLightmapsLength = 0;
            sumRealLightmapsLength = 0;

            listLightmapsInformations = new List<LightmapInformations>();
            LightmapData[] tab = LightmapSettings.lightmaps;
            foreach (LightmapData lightMapData in tab)
            {
                LightmapInformations light = new LightmapInformations(lightMapData);
                light.CheckLight();
                listLightmapsInformations.Add(light);

                sumLightmapsLength += light.lightLength;
                sumRealLightmapsLength += light.realLightLength;
            }

            nbLightmaps = listLightmapsInformations.Count;

            renderPipelineAsset = "";
            RenderPipelineAsset rpa = GraphicsSettings.renderPipelineAsset;
            if (rpa != null)
                renderPipelineAsset = rpa.GetType().Name;

            LightingSettings lightSettings;
            if (Lightmapping.TryGetLightingSettings(out lightSettings) == false)
                lightSettings = Lightmapping.lightingSettingsDefaults;

            if (lightSettings == null)
            {
                Debug.Log("[SceneInformations] No instance LightingSettings find");
                presenceOfLightSettings = false;
            }
            else
            {
                lightMode = lightSettings.mixedBakeMode;
                directionalMode = lightSettings.directionalityMode;
                presenceOfLightSettings = true;
            }

            pathLightmaps = string.Empty;
            if (listLightmapsInformations.Count > 0)
            {
                string pathLightsNotFinish = listLightmapsInformations[0].lightPath;

                string[] pathLightsTab = pathLightsNotFinish.Split('/');
                for (int i = 0; i < pathLightsTab.Length - 1; i++)
                    pathLightmaps += pathLightsTab[i] + "/";

                if (string.IsNullOrEmpty(pathLightmaps))
                    Debug.Log("SceneInformations lightPath pathLightmaps empty");
                else
                    pathLightmaps = pathLightmaps.Substring(0, pathLightmaps.Length - 1);
            }

            presenceOfRealtimeLight = 0; // Its a bool
            lightsRealtime = new List<Light>();

            Light[] lights = Object.FindObjectsOfType(typeof(Light)) as Light[];
            foreach (Light light in lights)
            {
                if (light.lightmapBakeType == LightmapBakeType.Realtime)
                {
                    presenceOfRealtimeLight = 1;
                    lightsRealtime.Add(light);
                }
            }
        }
    }

}
