using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using XRTools;
using XRTools.IN_MiniJSON;

namespace AssetValidator
{
    public struct AssetRulesResult
    {
        public string rulesName;
        public AssetInformations asset;
        public List<DataResult> dataMsgs;
        public List<MaterialRulesResult> listMaterialRulesResult;
    }

    public struct MaterialRulesResult
    {
        public Material material;
        public List<DataResult> dataMsgs;
        public List<TextureRulesResult> listTextureRulesResult;
    }

    public struct TextureRulesResult
    {
        public Texture texture;
        public List<DataResult> dataMsgs;
    }

    public struct DataResult
    {
        public string key;
        public string errorMsg;

        public DataResult(string _key, string _errorMsg)
        {
            key = _key;
            errorMsg = _errorMsg;
        }
    }

    public class AssetsValidator : MonoBehaviour
    {
        public static AssetsValidator instance;

        public const string BuiltinExtraResources = "Resources/unity_builtin_extra";
        public const string BuiltinResources = "Library/unity default resources";
        public List<AssetRules> listAssetRules { get; set; }

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        public AssetsValidator()
        {
            if (instance == null)
                instance = this;
        }

        public void LoadRules(string jsonText)
        {
            listAssetRules = new List<AssetRules>();

            //StreamReader inputStream = new StreamReader(path);
            //string jsonText = inputStream.ReadToEnd();

            Dictionary<string, object> dico = Json.Deserialize(jsonText) as Dictionary<string, object>;
            if (dico.ContainsKey("Rules"))
            {
                List<object> rules = dico["Rules"] as List<object>;
                foreach (object rule in rules)
                {
                    Dictionary<string, object> contentRule = rule as Dictionary<string, object>;

                    AssetRules newRule = new AssetRules();
                    newRule.name = IN_Utils.GetStr(contentRule, "name");

                    newRule.globalNbVertex      = (int)IN_Utils.GetLong(contentRule, AssetRules.keyGlobalVertex);
                    newRule.globalNbTriangles   = (int)IN_Utils.GetLong(contentRule, AssetRules.keyGlobalTriangles);
                    newRule.globalNbSubmesh     = (int)IN_Utils.GetLong(contentRule, AssetRules.keyGlobalSubmesh);
                    newRule.globalNbMaterial    = (int)IN_Utils.GetLong(contentRule, AssetRules.keyGlobalMaterial);
                    newRule.renderPipelineAsset = IN_Utils.GetStr(contentRule, AssetRules.keyRenderPipelineAsset);

                    newRule.assetNbVertex       = (int)IN_Utils.GetLong(contentRule, AssetRules.keyVertex);
                    newRule.assetNbTriangles    = (int)IN_Utils.GetLong(contentRule, AssetRules.keyTriangles);
                    newRule.assetNbSubmesh      = (int)IN_Utils.GetLong(contentRule, AssetRules.keySubmesh);
                    newRule.assetNbMaterial     = (int)IN_Utils.GetLong(contentRule, AssetRules.keyMaterial);
                    newRule.meshPath            = IN_Utils.GetStr(contentRule, AssetRules.keyMeshPath);
                    newRule.prefabPath          = IN_Utils.GetStr(contentRule, AssetRules.keyPrefabPath);

                    newRule.expectedShaders     = IN_Utils.GetListString(contentRule, AssetRules.keyShader);
                    newRule.materialPath        = IN_Utils.GetStr(contentRule, AssetRules.keyMaterialPath);

                    newRule.widthTexture        = (int)IN_Utils.GetLong(contentRule, AssetRules.keyWidthTexture);
                    newRule.heightTexture       = (int)IN_Utils.GetLong(contentRule, AssetRules.keyHeightTexture);
                    newRule.realWidthTexture    = (int)IN_Utils.GetLong(contentRule, AssetRules.keyRealWidthTexture);
                    newRule.realHeightTexture   = (int)IN_Utils.GetLong(contentRule, AssetRules.keyRealHeightTexture);
                    newRule.expectedExtensions  = IN_Utils.GetListString(contentRule, AssetRules.keyExtension);
                    newRule.lengthTexture       = IN_Utils.GetFloat(contentRule, AssetRules.keyLengthTexture);
                    newRule.useCrunchCompression = (int)IN_Utils.GetLong(contentRule, AssetRules.keyUseCrunchCompression, -1);
                    newRule.maxTextureSize      = (int)IN_Utils.GetLong(contentRule, AssetRules.keyMaxTextureSize);
                    newRule.texturesPath        = IN_Utils.GetStr(contentRule, AssetRules.keyTexturePath);

                    newRule.lightmapsLength             = IN_Utils.GetFloat(contentRule, AssetRules.keyLightmapsLength);
                    newRule.lightmapsRealLength         = IN_Utils.GetFloat(contentRule, AssetRules.keyLightmapsRealLength);
                    newRule.expectedMixedLightingModes = IN_Utils.GetListString(contentRule, AssetRules.keyExpectedMixedLightingModes);
                    newRule.directionalMode             = IN_Utils.GetStr(contentRule, AssetRules.keyExpectedDirectionalMode);
                    newRule.presenceOfLightsRealtime    = (int)IN_Utils.GetLong(contentRule, AssetRules.keyPresenceOfLightsRealtime, -1);

                    listAssetRules.Add(newRule);
                }
            }
        }

        public List<AssetRulesResult> ApplyRules(AssetRules assetRules, List<AssetInformations> assetsInfos, SceneInformations sceneInformations)
        {
            List<AssetRulesResult> listAssetErrorResult = new List<AssetRulesResult>();
            List<DataResult> listAssetErrorMsg;

            List<MaterialRulesResult> listMaterialErrorResult;
            List<DataResult> listMaterialErrorMsg;

            List<TextureRulesResult> listTextureErrorResult;
            List<DataResult> listTextureErrorMsg;

            int globalNbVertex = 0;
            int globalNbTriangles = 0;
            int globalNbSubmesh = 0;

            foreach (AssetInformations assetInfos in assetsInfos)
            {
                // Asset check
                listAssetErrorMsg = new List<DataResult>();

                int nbVertex = assetInfos.vertexCount;
                int limitNbVertex = assetRules.assetNbVertex;
                if (limitNbVertex != 0 && nbVertex > limitNbVertex)
                {
                    string output = "Max vertices expected : " + limitNbVertex + ", vertices find : " + nbVertex;
                    listAssetErrorMsg.Add(new DataResult(AssetRules.keyVertex, output));
                }

                int nbTriangles = assetInfos.trianglesCount;
                int limitNbTriangles = assetRules.assetNbTriangles;
                if (limitNbTriangles != 0 && nbTriangles > limitNbTriangles)
                {
                    string output = "Max triangles expected : " + limitNbTriangles + ", triangles find : " + nbTriangles;
                    listAssetErrorMsg.Add(new DataResult(AssetRules.keyTriangles, output));
                }

                int nbSubmesh = assetInfos.submeshCount;
                int limitNbSubmesh = assetRules.assetNbSubmesh;
                if (limitNbSubmesh != 0 && nbSubmesh > limitNbSubmesh)
                {
                    string output = "Max submesh expected : " + limitNbSubmesh + ", submesh find : " + nbSubmesh;
                    listAssetErrorMsg.Add(new DataResult(AssetRules.keySubmesh, output));
                }

                int nbMaterial = assetInfos.materialsCount;
                int limitNbMaterial = assetRules.assetNbMaterial;
                if (limitNbMaterial != 0 && nbMaterial > limitNbMaterial)
                {
                    string output = "Max material expected : " + limitNbMaterial + ", material find : " + nbMaterial;
                    listAssetErrorMsg.Add(new DataResult(AssetRules.keyMaterial, output));
                }

                string meshPath = assetInfos.meshPath;
                string meshPathExpected = assetRules.meshPath;
                if (string.IsNullOrEmpty(meshPathExpected) == false && meshPath.Contains(meshPathExpected) == false && meshPath != BuiltinResources && meshPath != BuiltinExtraResources)
                {
                    string output = "Mesh is not in folder expected. Start path expected : " + meshPathExpected + ", path find : " + meshPath;
                    listAssetErrorMsg.Add(new DataResult(AssetRules.keyMeshPath, output));
                }

                string prefabPath = assetInfos.prefabPath;
                if (string.IsNullOrEmpty(prefabPath) == false) // if nearest prefab find go check it
                {
                    string prefabPathExpected = assetRules.prefabPath;
                    if (string.IsNullOrEmpty(prefabPathExpected) == false && prefabPath.Contains(prefabPathExpected) == false && prefabPath != BuiltinResources && prefabPath != BuiltinExtraResources)
                    {
                        string output = "Prefab is not in folder expected. Start path expected : " + prefabPathExpected + ", path find : " + prefabPath;
                        listAssetErrorMsg.Add(new DataResult(AssetRules.keyPrefabPath, output));
                    }
                }

                // Material check
                listMaterialErrorResult = new List<MaterialRulesResult>();
                List<MaterialInformations> listMatInfos = assetInfos.listMaterialInformations;
                for (int i = 0; i < listMatInfos.Count; i++)
                {
                    listMaterialErrorMsg = new List<DataResult>();

                    string shader = listMatInfos[i].materialShader;
                    List<string> expectedShaders = assetRules.expectedShaders;
                    if (expectedShaders != null && expectedShaders.Count != 0 && expectedShaders.Contains(shader) == false)
                    {
                        string output = "Shader expected : ";
                        foreach (string expectedShader in expectedShaders)
                        {
                            output += expectedShader + " or ";
                        }
                        output = output.Substring(0, output.Length - 4);

                        output += ", Shader find : " + shader;
                        listMaterialErrorMsg.Add(new DataResult(AssetRules.keyShader, output));
                    }

                    string materialPath = listMatInfos[i].materialPath;
                    string materialPathExpected = assetRules.materialPath;
                    if (string.IsNullOrEmpty(materialPathExpected) == false && materialPath.Contains(materialPathExpected) == false && materialPath != BuiltinResources && materialPath != BuiltinExtraResources)
                    {
                        string output = "Material is not in folder expected. Start path expected : " + materialPathExpected + ", path find : " + materialPath;
                        listMaterialErrorMsg.Add(new DataResult(AssetRules.keyMaterialPath, output));
                    }

                    // Texture check
                    listTextureErrorResult = new List<TextureRulesResult>();
                    List<TextureInformations> listTextureInfos = listMatInfos[i].listTextureInformations;
                    for (int y = 0; y < listTextureInfos.Count; y++)
                    {
                        listTextureErrorMsg = new List<DataResult>();

                        int widthTexture = listTextureInfos[y].textureWidth;
                        int limitWidthTexture = assetRules.widthTexture;
                        if (limitWidthTexture != 0 && widthTexture > limitWidthTexture)
                        {
                            string output = "Max texture width expected : " + limitWidthTexture + ", texture width find : " + widthTexture;
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyWidthTexture, output));
                        }

                        int heightTexture = listTextureInfos[y].textureHeight;
                        int limitHeightTexture = assetRules.heightTexture;
                        if (limitHeightTexture != 0 && heightTexture > limitHeightTexture)
                        {
                            string output = "Max texture height expected : " + limitWidthTexture + ", texture height find : " + heightTexture;
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyHeightTexture, output));
                        }

                        int realWidthTexture = listTextureInfos[y].textureRealWidth;
                        int limitRealWidthTexture = assetRules.realWidthTexture;
                        if (limitRealWidthTexture != 0 && realWidthTexture > limitRealWidthTexture)
                        {
                            string output = "Max real texture width expected : " + limitRealWidthTexture + ", real texture width find : " + realWidthTexture;
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyRealWidthTexture, output));
                        }

                        int realheightTexture = listTextureInfos[y].textureRealHeight;
                        int limitRealHeightTexture = assetRules.realHeightTexture;
                        if (limitRealHeightTexture != 0 && realheightTexture > limitRealHeightTexture)
                        {
                            string output = "Max real texture height expected : " + limitRealHeightTexture + ", real texture height find : " + realheightTexture;
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyRealHeightTexture, output));
                        }

                        string extension = listTextureInfos[y].textureExtension;
                        List<string> expectedExtensions = assetRules.expectedExtensions;
                        if (expectedExtensions != null && expectedExtensions.Count != 0 && expectedExtensions.Contains(extension) == false)
                        {
                            string output = "Extension expected : ";
                            foreach (string expectedExtension in expectedExtensions)
                            {
                                output += expectedExtension + " or ";
                            }
                            output = output.Substring(0, output.Length - 4);

                            output += ", Extension find : " + extension;
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyExtension, output));
                        }

                        float lengthTexture = listTextureInfos[y].textureLength;
                        float limitLengthTexture = assetRules.lengthTexture;
                        if (limitLengthTexture != 0 && lengthTexture > limitLengthTexture)
                        {
                            string output = "Length texture expected : " + limitLengthTexture + " Mo, Length texture find : " + lengthTexture + " Mo";
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyLengthTexture, output));
                        }

                        int useCrunchCompression = listTextureInfos[y].useCrunchCompression;
                        int useCrunchCompressionExpected = assetRules.useCrunchCompression;
                        if ((useCrunchCompressionExpected == 0 || useCrunchCompressionExpected == 1) && useCrunchCompression != useCrunchCompressionExpected)
                        {
                            string valueExpected = useCrunchCompressionExpected == 1 ? "Yes" : "No";
                            string valueFind = useCrunchCompression == 1 ? "Yes" : "No";
                            string output = "Use Crunch Compression expected : " + valueExpected + ", Use Crunch Compression find : " + valueFind;
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyUseCrunchCompression, output));
                        }

                        int maxTextureSize = listTextureInfos[y].maxTextureSize;
                        int maxTextureSizeExpected = assetRules.maxTextureSize;
                        if (maxTextureSizeExpected != 0 && maxTextureSize > maxTextureSizeExpected)
                        {
                            string output = "Max texture size expected : " + maxTextureSizeExpected + ", Max texture size find : " + maxTextureSize;
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyMaxTextureSize, output));
                        }

                        string texturePath = listTextureInfos[y].texturePath;
                        string texturePathExpected = assetRules.texturesPath;
                        if (string.IsNullOrEmpty(texturePathExpected) == false && texturePath.Contains(texturePathExpected) == false && texturePath != BuiltinResources && texturePath != BuiltinExtraResources)
                        {
                            string output = "Texture is not in folder expected. Start path expected : " + texturePathExpected + ", path find : " + texturePath;
                            listTextureErrorMsg.Add(new DataResult(AssetRules.keyTexturePath, output));
                        }

                        if (listTextureErrorMsg.Count > 0)
                        {
                            TextureRulesResult result = new TextureRulesResult();
                            result.texture = listMatInfos[i].listTextures[y];
                            result.dataMsgs = listTextureErrorMsg;

                            listTextureErrorResult.Add(result);
                        }
                    }

                    if (listMaterialErrorMsg.Count > 0 || listTextureErrorResult.Count > 0)
                    {
                        MaterialRulesResult result = new MaterialRulesResult();
                        result.material = assetInfos.listMaterials[i];
                        result.dataMsgs = listMaterialErrorMsg;
                        result.listTextureRulesResult = listTextureErrorResult;

                        listMaterialErrorResult.Add(result);
                    }
                }

                if (listAssetErrorMsg.Count > 0 || listMaterialErrorResult.Count > 0)
                {
                    AssetRulesResult result = new AssetRulesResult();
                    result.rulesName = assetRules.name;
                    result.asset = assetInfos;
                    result.dataMsgs = listAssetErrorMsg;
                    result.listMaterialRulesResult = listMaterialErrorResult;

                    listAssetErrorResult.Add(result);
                }

                globalNbVertex += assetInfos.vertexCount;
                globalNbTriangles += assetInfos.trianglesCount;
                globalNbSubmesh += assetInfos.submeshCount;
            }

            // Global asset check
            listAssetErrorMsg = new List<DataResult>();

            int limitGlobalNbVertex = assetRules.globalNbVertex;
            if (limitGlobalNbVertex != 0 && globalNbVertex > limitGlobalNbVertex)
            {
                string output = "Max total vertices expected : " + limitGlobalNbVertex + ", total vertices find : " + globalNbVertex;
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyGlobalVertex, output));
            }

            int limitGlobalNbTriangles = assetRules.globalNbTriangles;
            if (limitGlobalNbTriangles != 0 && globalNbTriangles > limitGlobalNbTriangles)
            {
                string output = "Max total triangles expected : " + limitGlobalNbTriangles + ", total triangles find : " + globalNbTriangles;
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyGlobalTriangles, output));
            }

            int limitGlobalNbSubmesh = assetRules.globalNbSubmesh;
            if (limitGlobalNbSubmesh != 0 && globalNbSubmesh > limitGlobalNbSubmesh)
            {
                string output = "Max total submesh expected : " + limitGlobalNbSubmesh + ", total submesh find : " + globalNbSubmesh;
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyGlobalSubmesh, output));
            }

            int limitGlobalNbMaterial = assetRules.globalNbMaterial;
            if (limitGlobalNbMaterial != 0 && sceneInformations.globalMaterialsCount > limitGlobalNbMaterial)
            {
                string output = "Max total material expected : " + limitGlobalNbMaterial + ", total material find : " + sceneInformations.globalMaterialsCount;
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyGlobalMaterial, output));
            }

            // Global scene informations check
            string renderPipelineAsset = sceneInformations.renderPipelineAsset;
            string renderPipelineAssetExpected = assetRules.renderPipelineAsset;
            if (string.IsNullOrEmpty(renderPipelineAssetExpected) == false && renderPipelineAsset.Equals(renderPipelineAssetExpected) == false)
            {
                string output = "Render pipeline asset expected : " + renderPipelineAssetExpected + ", render pipeline asset find : " + renderPipelineAsset;
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyRenderPipelineAsset, output));
            }

            float lightmapsLength = sceneInformations.sumLightmapsLength;
            float limitLightmapsLength = assetRules.lightmapsLength;
            if (limitLightmapsLength != 0 && lightmapsLength > limitLightmapsLength)
            {
                string output = "Max length lightmaps expected : " + limitLightmapsLength + " Mo, length lightmaps find : " + lightmapsLength + " Mo";
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyLightmapsLength, output));
            }

            float lightmapsRealLength = sceneInformations.sumRealLightmapsLength;
            float limitLightmapsRealLength = assetRules.lightmapsRealLength;
            if (limitLightmapsRealLength != 0 && lightmapsRealLength > limitLightmapsRealLength)
            {
                string output = "Max real length lightmaps expected : " + limitLightmapsRealLength + " Mo, real length lightmaps find : " + lightmapsRealLength + " Mo";
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyLightmapsRealLength, output));
            }

            bool presenceOfLightSettings = sceneInformations.presenceOfLightSettings;

            string mixedLightingMode = sceneInformations.lightMode.ToString();
            List<string> expectedMixedLightingModes = assetRules.expectedMixedLightingModes;
            if (expectedMixedLightingModes != null && expectedMixedLightingModes.Count != 0 && expectedMixedLightingModes.Contains(mixedLightingMode) == false)
            {
                string output;
                if (presenceOfLightSettings)
                {
                    output = "Mixed Lighting Mode expected : ";
                    foreach (string expectedMixedLightingMode in expectedMixedLightingModes)
                    {
                        output += expectedMixedLightingMode + " or ";
                    }
                    output = output.Substring(0, output.Length - 4);

                    output += ", Mixed Lighting Mode find : " + mixedLightingMode;
                }
                else
                {
                    output = "No instance of LightingSettings found. Mixed Lighting Mode not checked.";
                }
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyExpectedMixedLightingModes, output));
            }

            string directionalMode = sceneInformations.directionalMode.ToString();
            string directionalModeExpected = assetRules.directionalMode;
            if (string.IsNullOrEmpty(directionalModeExpected) == false && directionalMode.Equals(directionalModeExpected) == false)
            {
                string output;
                if (presenceOfLightSettings)
                    output = "Directional mode expected : " + directionalModeExpected + ", directional mode find : " + directionalMode;
                else
                    output = "No instance of LightingSettings found. Directional mode not checked.";

                listAssetErrorMsg.Add(new DataResult(AssetRules.keyExpectedDirectionalMode, output));
            }

            int presenceOfLightRealtime = sceneInformations.presenceOfRealtimeLight;
            int presenceOfLightRealtimeExpected = assetRules.presenceOfLightsRealtime;
            if ((presenceOfLightRealtimeExpected == 0 || presenceOfLightRealtimeExpected == 1) && presenceOfLightRealtime != presenceOfLightRealtimeExpected)
            {
                string valueExpected = presenceOfLightRealtimeExpected == 1 ? "Yes" : "No";
                string valueFind = presenceOfLightRealtime == 1 ? "Yes" : "No";
                string output = "Presence of light realtime expected : " + valueExpected + ", presence of light realtime find : " + valueFind;
                listAssetErrorMsg.Add(new DataResult(AssetRules.keyPresenceOfLightsRealtime, output));
            }

            if (listAssetErrorMsg.Count > 0)
            {
                AssetRulesResult result = new AssetRulesResult();
                result.rulesName = assetRules.name;
                result.asset = null;
                result.dataMsgs = listAssetErrorMsg;
                result.listMaterialRulesResult = null;

                listAssetErrorResult.Insert(0, result);
            }

            return listAssetErrorResult;
        }

    }

}
