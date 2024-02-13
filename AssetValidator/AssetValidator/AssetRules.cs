using System.Collections.Generic;

namespace AssetValidator
{

    public class AssetRules
    {
        public string name;

        // keys
        public const string keyGlobalVertex = "globalNbVertex";
        public const string keyGlobalTriangles = "globalNbTriangles";
        public const string keyGlobalSubmesh = "globalNbSubmesh";
        public const string keyGlobalMaterial = "globalNbMaterial";
        public const string keyRenderPipelineAsset = "renderPipelineAsset";

        public const string keyVertex = "assetNbVertex";
        public const string keyTriangles = "assetNbTriangles";
        public const string keySubmesh = "assetNbSubmesh";
        public const string keyMaterial = "assetNbMaterial";
        public const string keyMeshPath = "meshPath";
        public const string keyPrefabPath = "prefabPath";

        public const string keyShader = "expectedShaders";
        public const string keyMaterialPath = "materialPath";

        public const string keyWidthTexture = "widthTexture";
        public const string keyHeightTexture = "heightTexture";
        public const string keyRealWidthTexture = "realWidthTexture";
        public const string keyRealHeightTexture = "realHeightTexture";
        public const string keyExtension = "extensionTexture";
        public const string keyLengthTexture = "lengthTexture";
        public const string keyUseCrunchCompression = "useCrunchCompression";
        public const string keyMaxTextureSize = "maxTextureSize";
        public const string keyTexturePath = "texturePath";

        public const string keyLightmapsLength = "lightmapsLength";
        public const string keyLightmapsRealLength = "lightmapsRealLength";
        public const string keyExpectedMixedLightingModes = "expectedMixedLightingModes";
        public const string keyExpectedDirectionalMode = "expectedDirectionalMode";
        public const string keyPresenceOfLightsRealtime = "presenceOfLightsRealtime";

        // limit global
        public int globalNbVertex { get; set; }
        public int globalNbTriangles { get; set; }
        public int globalNbSubmesh { get; set; }
        public int globalNbMaterial { get; set; }
        public string renderPipelineAsset { get; set; }

        // limit asset
        public int assetNbVertex { get; set; }
        public int assetNbTriangles { get; set; }
        public int assetNbSubmesh { get; set; }
        public int assetNbMaterial { get; set; }
        public string meshPath { get; set; }
        public string prefabPath { get; set; }

        // limit material
        public List<string> expectedShaders { get; set; }
        public string materialPath { get; set; }

        // limit texture
        public int widthTexture { get; set; }
        public int heightTexture { get; set; }
        public int realWidthTexture { get; set; }
        public int realHeightTexture { get; set; }
        public List<string> expectedExtensions { get; set; }
        public float lengthTexture { get; set; }
        public int useCrunchCompression { get; set; }
        public int maxTextureSize { get; set; }
        public string texturesPath { get; set; }

        // limit light
        public float lightmapsLength { get; set; }
        public float lightmapsRealLength { get; set; }
        public List<string> expectedMixedLightingModes { get; set; }
        public string directionalMode { get; set; }
        public int presenceOfLightsRealtime { get; set; }

    }

}
