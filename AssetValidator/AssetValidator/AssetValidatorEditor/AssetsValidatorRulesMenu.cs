using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XRWebService;

namespace AssetValidator.AssetValidatorEditor
{

	public partial class AssetsValidatorEditor : EditorWindow
	{
		private Vector2 rulesScrollPos;

		private const string KEY_RULE_AUTH = "IN_RULE_AUTH";
		private const string NAME_RULE_FILE = "immersiveNowRules.json";
		private bool authentificationDone;

		public static XRAddressablePartnerList addressableList;

		private void RulesMenu()
		{
			if (authentificationDone == false)
			{
				bool isAuthentificated = EditorPrefs.GetBool(KEY_RULE_AUTH);
				if (isAuthentificated == true)
				{
					if (addressableList == null)
						Debug.Log(TAG + "Verify project ID");
					else
					{
						string keyRemotePref = EditorPrefs.GetString(AssetValidatorParams.RULE_PROJECT_ID_KEY);
						AssetValidatorParams.RuleProjectID = keyRemotePref;

						List<XRAddressablePartner> list = addressableList.GetPartnerList();
						XRAddressablePartner currentPartner = null;
						foreach (XRAddressablePartner license in list)
						{
							if (keyRemotePref.Equals(license.m_Licence))
							{
								currentPartner = license;
							}
						}

						string url = "";
						if (currentPartner != null)
						{
							url = currentPartner.m_DevBasePath;
							if (string.IsNullOrEmpty(url))
								Debug.Log(TAG + "Project ID not found");
							else
								Debug.Log(TAG + "Project ID found");
							url += NAME_RULE_FILE;
						}
						else
							Debug.Log(TAG + "currentPartner not found");

						INAddressableEditorCoroutine.StartCoroutineOwnerless(GetRulesRequest(url));
					}
				}

				authentificationDone = true;
			}

			DisplaySelectRuleSection();

			if (string.IsNullOrEmpty(currentFileRules) == false)
			{
				DrawLine(1, new Color(0.5f, 0.5f, 0.5f, 1), 5, 5);

				EditorGUILayout.BeginHorizontal();
				{
					DisplayRulesList();

					if (currentAssetRules != null)
						DisplayRulesCriterion();
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		#region Rules display

		private void DisplaySelectRuleSection()
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical();
				{
					if (string.IsNullOrEmpty(currentFileRules) == false)
						EditorGUILayout.LabelField("<b>Current Project ID : " + currentFileRules + "</b>", mainTextStyle);
					else
						EditorGUILayout.LabelField("<b>Please enter your Project ID</b>", mainTextStyle);

					EditorGUILayout.BeginHorizontal(leftPaddingStyle);
					{
						AssetValidatorParams.RuleProjectID = EditorGUILayout.TextField(AssetValidatorParams.RuleProjectID, textFieldStyle, GUILayout.Width(387));

						if (!AssetValidatorParams.RuleProjectID.Equals(EditorPrefs.GetString(AssetValidatorParams.RULE_PROJECT_ID_KEY)))
							EditorPrefs.SetString(AssetValidatorParams.RULE_PROJECT_ID_KEY, AssetValidatorParams.RuleProjectID);

						if (GUILayout.Button("OK", pluginSettingsButtonStyle))
						{
							addressableList = new XRAddressablePartnerList();
							INAddressableEditorCoroutine.StartCoroutine(addressableList.RequestPartnerAsync(AssetValidatorParams.RuleProjectID, OnSuccessAuth, OnFailedAuth), this);
						}
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DisplayRulesList()
		{
			EditorGUILayout.BeginVertical(marginLeftTopSectionStyle, GUILayout.Width(200), GUILayout.Height(position.height - 118)); // Material list asset users
			{
				EditorGUILayout.BeginVertical(backgroundStyle);
				{
					EditorGUILayout.LabelField("<b>Rules in selected file : </b>", titleStyle);
					rulesScrollPos = EditorGUILayout.BeginScrollView(rulesScrollPos);
					{
						foreach (AssetRules assetRules in assetRulesManager.listAssetRules)
						{
							if (currentAssetRules == null)
								currentAssetRules = assetRules; // Select first rules by default

							if (currentAssetRules != null && currentAssetRules == assetRules)
							{
								if (GUILayout.Button(assetRules.name, buttonActiveStyle))
									AssetRulesButtonAction(assetRules);
							}
							else
							{
								if (GUILayout.Button(assetRules.name, buttonStyle))
									AssetRulesButtonAction(assetRules);
							}
						}
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayRulesCriterion()
		{
			// global
			int gVertices = currentAssetRules.globalNbVertex;
			int gTriangles = currentAssetRules.globalNbTriangles;
			int gSubmesh = currentAssetRules.globalNbSubmesh;
			int gMaterial = currentAssetRules.globalNbMaterial;

			// asset
			int vertices = currentAssetRules.assetNbVertex;
			int triangles = currentAssetRules.assetNbTriangles;
			int submesh = currentAssetRules.assetNbSubmesh;
			int material = currentAssetRules.assetNbMaterial;
			string meshPath = currentAssetRules.meshPath;
			string prefabPath = currentAssetRules.prefabPath;

			// material
			List<string> shadersAuthorize = currentAssetRules.expectedShaders;
			string materialPath = currentAssetRules.materialPath;

			// texture
			int widthTexture = currentAssetRules.widthTexture;
			int heightTexture = currentAssetRules.heightTexture;
			int realWidthTexture = currentAssetRules.realWidthTexture;
			int realHeightTexture = currentAssetRules.realHeightTexture;
			List<string> extensionsAuthorize = currentAssetRules.expectedExtensions;
			float lengthTexture = currentAssetRules.lengthTexture;
			int useCrunchCompression = currentAssetRules.useCrunchCompression;
			int maxTextureSize = currentAssetRules.maxTextureSize;
			string texturePath = currentAssetRules.texturesPath;

			// scene (mainly light)
			string renderPipelineAsset = currentAssetRules.renderPipelineAsset;
			float lightmapsLength = currentAssetRules.lightmapsLength;
			float lightmapsRealLength = currentAssetRules.lightmapsRealLength;
			List<string> mixedLightingModesAuthorize = currentAssetRules.expectedMixedLightingModes;
			string directionalMode = currentAssetRules.directionalMode;
			int presenceOfLightsRealtime = currentAssetRules.presenceOfLightsRealtime;

			EditorGUILayout.BeginVertical();
			{
				if (gVertices != 0 || gTriangles != 0 || gSubmesh != 0 || gMaterial != 0)
				{
					EditorGUILayout.BeginVertical(backgroundBlocStyle);
					{
						EditorGUILayout.LabelField("<b>Global rules : </b>", titleStyle);
						CheckAndDisplay("Max total vertices expected : " + gVertices, "0");
						CheckAndDisplay("Max total triangles expected : " + gTriangles, "0");
						CheckAndDisplay("Max total submesh expected : " + gSubmesh, "0");
						CheckAndDisplay("Max total material expected : " + gMaterial, "0");
						EditorGUILayout.Space();
					}
					EditorGUILayout.EndVertical();
				}

				if (string.IsNullOrEmpty(renderPipelineAsset) == false || lightmapsLength != 0 || lightmapsRealLength != 0 || (mixedLightingModesAuthorize != null && mixedLightingModesAuthorize.Count > 0)
					|| string.IsNullOrEmpty(directionalMode) == false || presenceOfLightsRealtime != -1)
				{
					EditorGUILayout.BeginVertical(backgroundBlocStyle);
					{
						EditorGUILayout.LabelField("<b>Scene rules : </b>", titleStyle);
						CheckAndDisplay("Render pipeline asset expected : " + renderPipelineAsset, "");
						if (lightmapsLength != 0)
							EditorGUILayout.LabelField("Max length lightmaps expected : " + lightmapsLength + " Mo", normalTextStyle);
						if (lightmapsRealLength != 0)
							EditorGUILayout.LabelField("Max real length lightmaps expected : " + lightmapsRealLength + " Mo", normalTextStyle);
						if (mixedLightingModesAuthorize != null && mixedLightingModesAuthorize.Count > 0)
						{
							EditorGUILayout.LabelField("<b>Mixed Lighting Mode authorize : </b>", littleTitleStyle);
							foreach (string elem in mixedLightingModesAuthorize)
								EditorGUILayout.LabelField("   " + elem, normalTextStyle);
						}
						CheckAndDisplay("Directional mode expected : " + directionalMode, "");
						if (presenceOfLightsRealtime != -1)
						{
							string valueTxt = presenceOfLightsRealtime == 1 ? "Yes" : "No";
							EditorGUILayout.LabelField("Presence of light realtime expected : " + valueTxt, normalTextStyle);
						}
						EditorGUILayout.Space();
					}
					EditorGUILayout.EndVertical();
				}

				if (vertices != 0 || triangles != 0 || submesh != 0 || material != 0 || !string.IsNullOrEmpty(meshPath) || !string.IsNullOrEmpty(prefabPath))
				{
					EditorGUILayout.BeginVertical(backgroundBlocStyle);
					{
						EditorGUILayout.LabelField("<b>Asset rules : </b>", titleStyle);
						CheckAndDisplay("Max vertices expected : " + vertices, "0");
						CheckAndDisplay("Max triangles expected : " + triangles, "0");
						CheckAndDisplay("Max submesh expected : " + submesh, "0");
						CheckAndDisplay("Max material expected : " + material, "0");
						CheckAndDisplay("Mesh path expected : " + meshPath, "");
						CheckAndDisplay("Prefab path expected : " + prefabPath, "");
						EditorGUILayout.Space();
					}
					EditorGUILayout.EndVertical();
				}

				if (!string.IsNullOrEmpty(materialPath) || (shadersAuthorize != null && shadersAuthorize.Count > 0))
				{
					EditorGUILayout.BeginVertical(backgroundBlocStyle);
					{
						EditorGUILayout.LabelField("<b>Material rules : </b>", titleStyle);
						if (shadersAuthorize != null && shadersAuthorize.Count > 0)
						{
							EditorGUILayout.LabelField("<b>Shaders authorize : </b>", littleTitleStyle);
							foreach (string elem in shadersAuthorize)
								EditorGUILayout.LabelField("   " + elem, normalTextStyle);
						}
						CheckAndDisplay("Material path expected : " + materialPath, "");
						EditorGUILayout.Space();
					}
					EditorGUILayout.EndVertical();
				}

				if (widthTexture != 0 || heightTexture != 0 || realWidthTexture != 0 || realHeightTexture != 0 || (extensionsAuthorize != null && extensionsAuthorize.Count > 0)
					|| useCrunchCompression != -1 || maxTextureSize != 0 || !string.IsNullOrEmpty(texturePath))
				{
					EditorGUILayout.BeginVertical(backgroundBlocStyle);
					{
						EditorGUILayout.LabelField("<b>Texture rules : </b>", titleStyle);
						CheckAndDisplay("Max texture width expected : " + widthTexture, "0");
						CheckAndDisplay("Max texture height expected : " + heightTexture, "0");
						CheckAndDisplay("Max real texture width expected : " + realWidthTexture, "0");
						CheckAndDisplay("Max real texture height expected : " + realHeightTexture, "0");
						if (extensionsAuthorize != null && extensionsAuthorize.Count > 0)
						{
							EditorGUILayout.LabelField("<b>Extensions authorize : </b>", littleTitleStyle);
							foreach (string elem in extensionsAuthorize)
								EditorGUILayout.LabelField("   " + elem, normalTextStyle);
						}
						if (lengthTexture > 0.001f)
							EditorGUILayout.LabelField("Length texture expected : " + lengthTexture + " Mo", normalTextStyle);
						if (useCrunchCompression != -1)
						{
							string valueTxt = useCrunchCompression == 1 ? "Yes" : "No";
							EditorGUILayout.LabelField("Use crunch compression expected : " + valueTxt, normalTextStyle);
						}
						CheckAndDisplay("Max texture size expected : " + maxTextureSize, "0");
						CheckAndDisplay("Texture path expected : " + texturePath, "");
						EditorGUILayout.Space();
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndVertical();
		}

		#endregion
	}

}
