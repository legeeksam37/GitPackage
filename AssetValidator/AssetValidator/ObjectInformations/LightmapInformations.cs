using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetValidator
{

    public class LightmapInformations
    {
        public LightmapData myLightmapData { get; set; }
        public Texture2D myLight { get; set; }

        public string lightName { get; set; }
        public float lightLength { get; set; }
        public float realLightLength { get; set; }
        public string lightPath { get; set; }

        public LightmapInformations(LightmapData lightmapData)
        {
            myLightmapData = lightmapData;
            if (myLightmapData != null)
                myLight = lightmapData.lightmapColor;
        }

        public void CheckLight()
        {
            if (myLight != null)
            {
                lightName = myLight.name;

                byte[] data = myLight.GetRawTextureData();

                lightLength = (float)(data.Length / 1000000f); // pass octets to Mo
                lightLength = Mathf.Round(lightLength * 100f) / 100f; // 2 number after the comma

                lightPath = AssetDatabase.GetAssetPath(myLight);
                if (string.IsNullOrEmpty(lightPath))
                {
                    realLightLength = 0;
                    Debug.Log("LightmapInformations lightPath path not find");
                }
                else
                {
                    FileInfo fileInfo = new System.IO.FileInfo(lightPath);

                    realLightLength = (float)(fileInfo.Length / 1000000f); // pass octets to Mo
                    realLightLength = Mathf.Round(realLightLength * 100f) / 100f; // 2 number after the comma
                }
            }
        }
    }

}
