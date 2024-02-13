using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using XRTools;

namespace AssetValidator.AssetValidatorEditor
{

	public partial class AssetsValidatorEditor : EditorWindow
	{
		private static List<AssetRulesResult> listErrorResult;

		private List<AssetInformations> listAssetError;
		private List<AssetInformations> listAssetValid;

		private List<MaterialInformations> listMaterialError;
		private List<MaterialInformations> listMaterialValid;

		private List<TextureInformations> listTextureError;
		private List<TextureInformations> listTextureValid;

		private string globalVertexError = "";
		private string globalTrianglesError = "";
		private string globalSubmeshError = "";
		private string globalMaterialError = "";
		private string renderPipelineAssetError = "";
		private string globalPathError = "";
		private string globalNumberError = "";
		private bool globalError = false;

		private string vertexError = "";
		private string trianglesError = "";
		private string submeshError = "";
		private string materialsError = "";
		private string meshPathError = "";
		private string prefabPathError = "";
		private bool assetError = false;

		private string mainWidthTextureSizeError = "";
		private string mainHeightTextureSizeError = "";
		private string shaderError = "";
		private string materialPathError = "";
		private bool materialError = false;

		private string widthTextureError = "";
		private string heightTextureError = "";
		private string realWidthTextureError = "";
		private string realHeightTextureError = "";
		private string extensionTextureError = "";
		private string lengthTextureError = "";
		private string useCrunchCompressionError = "";
		private string maxTextureSizeError = "";
		private string texturePathError = "";
		private bool textureError = false;

		private string lightmapsLengthError = "";
		private string lightmapsRealLengthError = "";
		private string mixedLightingModeError = "";
		private string directionalModeError = "";
		private string presenceOfLightsRealtimeError = "";
		private bool sceneError = false;
		private int nbErrors = 0;

		private void ApplyRule()
		{
			ScansAssets(true);

			// check informations
			listErrorResult = assetRulesManager.ApplyRules(currentAssetRules, listAssetInformations, sceneInformations);

			// initialise / reinitialise list
			listAssetValid = new List<AssetInformations>();
			listMaterialValid = new List<MaterialInformations>();
			listTextureValid = new List<TextureInformations>();

			listAssetError = new List<AssetInformations>();
			listMaterialError = new List<MaterialInformations>();
			listTextureError = new List<TextureInformations>();

			listAssetValid.AddRange(listAssetInformations);
			listMaterialValid.AddRange(listMaterialInformations);
			listTextureValid.AddRange(listTextureInformations);

			// Get all errors find
			GetErrorFind();

			// debug display asset errors
			if (debugError)
				DisplayError();

			checkAssetLimitDone = true;
		}

		public void IsSceneDirty()
		{
			if (SceneManager.GetActiveScene() == null)
				return;

			if (SceneManager.GetActiveScene().isDirty)
			{
				//Debug.Log("cannot apply the rules when the scene is not saved");
				EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
			}
		}

		private void GenerateJsonToken(List<AssetRulesResult> listAssetError)
		{
			string date_time;
			bool availableUpload;
			string project_id;
			List<string> lstError = new List<string>();

			date_time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			project_id = EditorPrefs.GetString(AssetValidatorParams.RULE_PROJECT_ID_KEY);

			if (listAssetError == null)
				availableUpload = true;
			else
				availableUpload = false;

			//créer le dictionnaire
			Dictionary<string, object> dico = new Dictionary<string, object>();

			dico["date"]			= date_time;
			dico["availableUpload"] = availableUpload;
			dico["errors"]			= GetListError();
			dico["projetId"]		= project_id;

			string json = XRTools.IN_MiniJSON.Json.Serialize(dico);

			Debug.Log("project ID :" + project_id);

			string path = Application.dataPath.Replace("Assets", ("ImmersiveNow" + Path.DirectorySeparatorChar + "Build" + Path.DirectorySeparatorChar + "Addressable"));

			if (!string.IsNullOrEmpty(path))
			{
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}

			File.WriteAllText(path + "/" + project_id + "_AssetValidator.json", json);

			Dictionary<string, object> newDico = (Dictionary<string, object>)(XRTools.IN_MiniJSON.Json.Deserialize(json));
			
			string startTime = IN_Utils.GetStr(newDico, "date");
			DateTime m_StartTime;
			if (string.IsNullOrEmpty(startTime) == false)
            {
				m_StartTime = DateTime.Parse(startTime);
			}
			else
            {
				Debug.Log("The date format is wrong");
            }				
		}

		private void GetErrorFind()
		{
			if (listErrorResult != null)
			{
				globalVertexError = "";
				globalTrianglesError = "";
				globalSubmeshError = "";
				globalMaterialError = "";
				globalPathError = "";
				globalError = false;

				renderPipelineAssetError = "";
				lightmapsLengthError = "";
				lightmapsRealLengthError = "";
				mixedLightingModeError = "";
				directionalModeError = "";
				presenceOfLightsRealtimeError = "";
				sceneError = false;

				nbErrors = 0;
				globalNumberError = "";

				List<Material> listMatError = new List<Material>();
				List<Texture> listTexError = new List<Texture>();

				bool pathError = false;

				foreach (AssetRulesResult assetErrorResult in listErrorResult)
				{
					AssetInformations assetInfos = assetErrorResult.asset;

					if (assetInfos == null) // Its global error informations
					{
						nbErrors += assetErrorResult.dataMsgs.Count;
						foreach (DataResult dataResult in assetErrorResult.dataMsgs)
						{
							// check global error
							if (dataResult.key.Equals(AssetRules.keyGlobalVertex))
							{
								globalVertexError = dataResult.errorMsg;
								globalError = true;
							}
							else if (dataResult.key.Equals(AssetRules.keyGlobalTriangles))
							{
								globalTrianglesError = dataResult.errorMsg;
								globalError = true;
							}
							else if (dataResult.key.Equals(AssetRules.keyGlobalSubmesh))
							{
								globalSubmeshError = dataResult.errorMsg;
								globalError = true;
							}
							else if (dataResult.key.Equals(AssetRules.keyGlobalMaterial))
							{
								globalMaterialError = dataResult.errorMsg;
								globalError = true;
							}

							// check scene error (mainly light)
							else if (dataResult.key.Equals(AssetRules.keyRenderPipelineAsset))
							{
								renderPipelineAssetError = dataResult.errorMsg;
								sceneError = true;
							}
							else if (dataResult.key.Equals(AssetRules.keyLightmapsLength))
							{
								lightmapsLengthError = dataResult.errorMsg;
								sceneError = true;
							}
							else if (dataResult.key.Equals(AssetRules.keyLightmapsRealLength))
							{
								lightmapsRealLengthError = dataResult.errorMsg;
								sceneError = true;
							}
							else if (dataResult.key.Equals(AssetRules.keyExpectedMixedLightingModes))
							{
								mixedLightingModeError = dataResult.errorMsg;
								sceneError = true;
							}
							else if (dataResult.key.Equals(AssetRules.keyExpectedDirectionalMode))
							{
								directionalModeError = dataResult.errorMsg;
								sceneError = true;
							}
							else if (dataResult.key.Equals(AssetRules.keyPresenceOfLightsRealtime))
							{
								presenceOfLightsRealtimeError = dataResult.errorMsg;
								sceneError = true;
							}
						}
					}
					else
					{
						listAssetError.Add(assetErrorResult.asset);
						listAssetValid.Remove(assetErrorResult.asset);

						foreach (MaterialRulesResult materialErrorResult in assetErrorResult.listMaterialRulesResult)
						{
							listMatError.Add(materialErrorResult.material);
							foreach (TextureRulesResult textureErrorResult in materialErrorResult.listTextureRulesResult)
							{
								listTexError.Add(textureErrorResult.texture);

								nbErrors += textureErrorResult.dataMsgs.Count;
								foreach (DataResult dataResult in textureErrorResult.dataMsgs)
								{
									if (dataResult.key.Equals(AssetRules.keyTexturePath))
										pathError = true;
								}
							}

							nbErrors += materialErrorResult.dataMsgs.Count;
							foreach (DataResult dataResult in materialErrorResult.dataMsgs)
							{
								if (dataResult.key.Equals(AssetRules.keyMaterialPath))
									pathError = true;
							}
						}

						nbErrors += assetErrorResult.dataMsgs.Count;
						foreach (DataResult dataResult in assetErrorResult.dataMsgs)
						{
							string key = dataResult.key;
							if (key.Equals(AssetRules.keyPrefabPath) || key.Equals(AssetRules.keyMeshPath))
								pathError = true;
						}
					}
				}

				foreach (Material mat in listMatError)
				{
					foreach (MaterialInformations matInfos in listMaterialInformations)
					{
						if (matInfos.IsEqual(mat))
						{
							if (listMaterialError.Contains(matInfos) == false)
								listMaterialError.Add(matInfos);
							listMaterialValid.Remove(matInfos);
						}
					}
				}

				foreach (Texture texture in listTexError)
				{
					foreach (TextureInformations textureInfos in listTextureInformations)
					{
						if (textureInfos.IsEqual(texture))
						{
							if (listTextureError.Contains(textureInfos) == false)
								listTextureError.Add(textureInfos);
							listTextureValid.Remove(textureInfos);
						}
					}
				}

				if (pathError)
					globalPathError = "Path problems were found among the checked assets";
				else
					globalPathError = "";

				if (nbErrors == 1)
				{
					globalNumberError = "<b>Total error find : " + nbErrors + "</b>";
				}
				else if (nbErrors > 0)
				{
					globalNumberError = "<b>Total errors find : " + nbErrors + "</b>";
				}
				else
				{
					globalNumberError = "<b>All is fine</b>";
				}


				IsSceneDirty();
				GenerateJsonToken(listErrorResult);
			}
		}

		private void UpdateTextErrors()
		{
			ResetErrorMsgs();

			if (listErrorResult != null)
			{
				foreach (AssetRulesResult assetErrorResult in listErrorResult)
				{
					AssetInformations assetInfos = assetErrorResult.asset;

					// if current asset selected and its asset error
					if (currentAsset != null && currentAsset == assetInfos)
					{
						// Get asset errors
						foreach (DataResult dataResult in assetErrorResult.dataMsgs)
						{
							if (dataResult.key.Equals(AssetRules.keyVertex))
								vertexError = dataResult.errorMsg;
							else if (dataResult.key.Equals(AssetRules.keyTriangles))
								trianglesError = dataResult.errorMsg;
							else if (dataResult.key.Equals(AssetRules.keySubmesh))
								submeshError = dataResult.errorMsg;
							else if (dataResult.key.Equals(AssetRules.keyMaterial))
								materialsError = dataResult.errorMsg;
							else if (dataResult.key.Equals(AssetRules.keyMeshPath))
								meshPathError = dataResult.errorMsg;
							else if (dataResult.key.Equals(AssetRules.keyPrefabPath))
								prefabPathError = dataResult.errorMsg;

							assetError = true;
						}

						foreach (MaterialRulesResult materialErrorResult in assetErrorResult.listMaterialRulesResult)
						{
							// Get Materials have error
							if (currentMaterialInfos != null && currentMaterial == materialErrorResult.material)
							{
								foreach (DataResult dataResult in materialErrorResult.dataMsgs)
								{
									if (dataResult.key.Equals(AssetRules.keyShader))
										shaderError = dataResult.errorMsg;
									else if (dataResult.key.Equals(AssetRules.keyMaterialPath))
										materialPathError = dataResult.errorMsg;

									else if (dataResult.key.Equals(AssetRules.keyTexturePath))
										texturePathError = dataResult.errorMsg;

									materialError = true;
								}
							}

							foreach (TextureRulesResult textureErrorResult in materialErrorResult.listTextureRulesResult)
							{
								if (currentTextureInfos != null && currentTexture == textureErrorResult.texture)
								{
									foreach (DataResult dataResult in textureErrorResult.dataMsgs)
									{
										if (dataResult.key.Equals(AssetRules.keyWidthTexture))
											widthTextureError = dataResult.errorMsg;
										else if (dataResult.key.Equals(AssetRules.keyHeightTexture))
											heightTextureError = dataResult.errorMsg;
										else if (dataResult.key.Equals(AssetRules.keyRealWidthTexture))
											realWidthTextureError = dataResult.errorMsg;
										else if (dataResult.key.Equals(AssetRules.keyRealHeightTexture))
											realHeightTextureError = dataResult.errorMsg;
										else if (dataResult.key.Equals(AssetRules.keyExtension))
											extensionTextureError = dataResult.errorMsg;
										else if (dataResult.key.Equals(AssetRules.keyLengthTexture))
											lengthTextureError = dataResult.errorMsg;
										else if (dataResult.key.Equals(AssetRules.keyUseCrunchCompression))
											useCrunchCompressionError = dataResult.errorMsg;
										else if (dataResult.key.Equals(AssetRules.keyMaxTextureSize))
											maxTextureSizeError = dataResult.errorMsg;
										else if (dataResult.key.Equals(AssetRules.keyTexturePath))
											texturePathError = dataResult.errorMsg;

										textureError = true;
									}
								}
							}
						}
					}
				}
			}
		}

		private void ResetErrorMsgs()
		{
			vertexError = "";
			trianglesError = "";
			submeshError = "";
			materialsError = "";
			meshPathError = "";
			prefabPathError = "";
			assetError = false;

			mainWidthTextureSizeError = "";
			mainHeightTextureSizeError = "";
			shaderError = "";
			materialPathError = "";
			materialError = false;

			widthTextureError = "";
			heightTextureError = "";
			realWidthTextureError = "";
			realHeightTextureError = "";
			extensionTextureError = "";
			lengthTextureError = "";
			useCrunchCompressionError = "";
			maxTextureSizeError = "";
			texturePathError = "";
			textureError = false;
		}

		#region Console display

		private void DisplayConsole()
		{
			EditorGUILayout.BeginVertical();
			{
				DrawLine(1, new Color(0.5f, 0.5f, 0.5f, 1), 5, 5);

				EditorGUILayout.BeginVertical(errorSectionStyle, GUILayout.Height(125));
				{
					EditorGUILayout.BeginVertical(contentBlocStyle);
					{
						EditorGUILayout.LabelField("<b>Console : </b>", titleStyle);
						errorScrollPos = EditorGUILayout.BeginScrollView(errorScrollPos);
						{
							if (nbErrors == 0)
								EditorGUILayout.LabelField(globalNumberError, normalValidTextStyle);
							else
								EditorGUILayout.LabelField(globalNumberError, normalErrorTextStyle);
							if (globalError || sceneError || assetError || materialError || textureError)
							{
								if (globalError)
									DisplayGlobalErrors();

								if (sceneError)
									DisplaySceneErrors();

								if (assetError)
									DisplayAssetSelectedErrors();

								if (materialError)
									DisplayMaterialSelectedErrors();

								if (textureError)
									DisplayTextureSelectedErrors();
							}
							else if (nbErrors != 0)
								EditorGUILayout.LabelField("No errors detected with the selected objects", normalTextStyle);
						}
						EditorGUILayout.EndScrollView();
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private List<string> GetListError()
        {
			List<string> lstError = new List<string>();

			List<string> lstGlobalError = GetGlobalErrorList();
			if (lstGlobalError!=null && lstGlobalError.Count>0)
				lstError.AddRange(lstGlobalError);

			List<string> lstSceneError = GetSceneErrorList();
			if (lstSceneError.Count > 0)
				lstError.AddRange(lstSceneError);

			List<string> lstAssetSelectedError = GetAssetSelectedErrorList();
			if (lstAssetSelectedError.Count > 0)
				lstError.AddRange(lstAssetSelectedError);

			List<string> lstMaterialSelectedError = GetMaterialSelectedErrorList();
			if (lstMaterialSelectedError.Count > 0)
				lstError.AddRange(lstMaterialSelectedError);

			List<string> lstTextureSelectedError = GetTextureSelectedErrorList();
			if (lstTextureSelectedError.Count > 0)
				lstError.AddRange(lstTextureSelectedError);

			return lstError;
        }

		private List<string> GetGlobalErrorList()
        {
			List<string> globalErrorLst = new List<string>();

			if (string.IsNullOrEmpty(globalVertexError) == false)
				globalErrorLst.Add(globalVertexError);
			if (string.IsNullOrEmpty(globalTrianglesError) == false)
				globalErrorLst.Add(globalTrianglesError);
			if (string.IsNullOrEmpty(globalSubmeshError) == false)
				globalErrorLst.Add(globalSubmeshError);
			if (string.IsNullOrEmpty(globalMaterialError) == false)
				globalErrorLst.Add(globalMaterialError);
			if (string.IsNullOrEmpty(globalPathError) == false)
				globalErrorLst.Add(globalPathError);

			return globalErrorLst; 
        }

		private List<string> GetSceneErrorList()
        {
			List<string> lstErrorScene = new List<string>();

			if (string.IsNullOrEmpty(renderPipelineAssetError) == false)
				lstErrorScene.Add(renderPipelineAssetError);
			if (string.IsNullOrEmpty(lightmapsLengthError) == false)
				lstErrorScene.Add(lightmapsLengthError);
			if (string.IsNullOrEmpty(lightmapsRealLengthError) == false)
				lstErrorScene.Add(lightmapsRealLengthError);
			if (string.IsNullOrEmpty(mixedLightingModeError) == false)
				lstErrorScene.Add(mixedLightingModeError);
			if (string.IsNullOrEmpty(directionalModeError) == false)
				lstErrorScene.Add(directionalModeError);
			if (string.IsNullOrEmpty(presenceOfLightsRealtimeError) == false)
				lstErrorScene.Add(presenceOfLightsRealtimeError);

			return lstErrorScene;
		}

		private List<string> GetAssetSelectedErrorList()
		{
			List<string> lstErrorAssetSelected = new List<string>();

			if (string.IsNullOrEmpty(vertexError) == false)
				lstErrorAssetSelected.Add(vertexError);
			if (string.IsNullOrEmpty(trianglesError) == false)
				lstErrorAssetSelected.Add(trianglesError);
			if (string.IsNullOrEmpty(submeshError) == false)
				lstErrorAssetSelected.Add(submeshError);
			if (string.IsNullOrEmpty(materialsError) == false)
				lstErrorAssetSelected.Add(materialsError);
			if (string.IsNullOrEmpty(meshPathError) == false)
				lstErrorAssetSelected.Add(meshPathError);
			if (string.IsNullOrEmpty(prefabPathError) == false)
				lstErrorAssetSelected.Add(prefabPathError);

			return lstErrorAssetSelected;
		}

		private List<string> GetMaterialSelectedErrorList()
		{
			List<string> lstMaterialSelectedError = new List<string>();

			if (string.IsNullOrEmpty(mainWidthTextureSizeError) == false)
				lstMaterialSelectedError.Add(mainWidthTextureSizeError);
			if (string.IsNullOrEmpty(mainHeightTextureSizeError) == false)
				lstMaterialSelectedError.Add(mainHeightTextureSizeError);
			if (string.IsNullOrEmpty(shaderError) == false)
				lstMaterialSelectedError.Add(shaderError);
			if (string.IsNullOrEmpty(materialPathError) == false)
				lstMaterialSelectedError.Add(materialPathError);

			return lstMaterialSelectedError;
		}

		private List<string> GetTextureSelectedErrorList()
		{
			List<string> lstTextureSelectedError = new List<string>();

			if (string.IsNullOrEmpty(widthTextureError) == false)
				lstTextureSelectedError.Add(widthTextureError);
			if (string.IsNullOrEmpty(heightTextureError) == false)
				lstTextureSelectedError.Add(heightTextureError);
			if (string.IsNullOrEmpty(realWidthTextureError) == false)
				lstTextureSelectedError.Add(realWidthTextureError);
			if (string.IsNullOrEmpty(realHeightTextureError) == false)
				lstTextureSelectedError.Add(realHeightTextureError);
			if (string.IsNullOrEmpty(extensionTextureError) == false)
				lstTextureSelectedError.Add(extensionTextureError);
			if (string.IsNullOrEmpty(lengthTextureError) == false)
				lstTextureSelectedError.Add(lengthTextureError);
			if (string.IsNullOrEmpty(useCrunchCompressionError) == false)
				lstTextureSelectedError.Add(useCrunchCompressionError);
			if (string.IsNullOrEmpty(maxTextureSizeError) == false)
				lstTextureSelectedError.Add(maxTextureSizeError);
			if (string.IsNullOrEmpty(texturePathError) == false)
				lstTextureSelectedError.Add(texturePathError);

			return lstTextureSelectedError;
		}

		private void DisplayGlobalErrors()
		{
			EditorGUILayout.LabelField("<b>Errors on the selected set : </b>", littleTitleStyle);

			if (string.IsNullOrEmpty(globalVertexError) == false)
				EditorGUILayout.LabelField(globalVertexError, normalTextStyle);
			if (string.IsNullOrEmpty(globalTrianglesError) == false)
				EditorGUILayout.LabelField(globalTrianglesError, normalTextStyle);
			if (string.IsNullOrEmpty(globalSubmeshError) == false)
				EditorGUILayout.LabelField(globalSubmeshError, normalTextStyle);
			if (string.IsNullOrEmpty(globalMaterialError) == false)
				EditorGUILayout.LabelField(globalMaterialError, normalTextStyle);
			if (string.IsNullOrEmpty(globalPathError) == false)
				EditorGUILayout.LabelField(globalPathError, normalTextStyle);

			EditorGUILayout.Space();
		}

		private void DisplaySceneErrors()
		{
			EditorGUILayout.LabelField("<b>Errors on the current scene : </b>", littleTitleStyle);

			if (string.IsNullOrEmpty(renderPipelineAssetError) == false)
				EditorGUILayout.LabelField(renderPipelineAssetError, normalTextStyle);
			if (string.IsNullOrEmpty(lightmapsLengthError) == false)
				EditorGUILayout.LabelField(lightmapsLengthError, normalTextStyle);
			if (string.IsNullOrEmpty(lightmapsRealLengthError) == false)
				EditorGUILayout.LabelField(lightmapsRealLengthError, normalTextStyle);
			if (string.IsNullOrEmpty(mixedLightingModeError) == false)
				EditorGUILayout.LabelField(mixedLightingModeError, normalTextStyle);
			if (string.IsNullOrEmpty(directionalModeError) == false)
				EditorGUILayout.LabelField(directionalModeError, normalTextStyle);
			if (string.IsNullOrEmpty(presenceOfLightsRealtimeError) == false)
				EditorGUILayout.LabelField(presenceOfLightsRealtimeError, normalTextStyle);

			EditorGUILayout.Space();
		}

		private void DisplayAssetSelectedErrors()
		{
			EditorGUILayout.LabelField("<b>Errors detected on the selected asset : </b>", littleTitleStyle);

			if (string.IsNullOrEmpty(vertexError) == false)
				EditorGUILayout.LabelField(vertexError, normalTextStyle);
			if (string.IsNullOrEmpty(trianglesError) == false)
				EditorGUILayout.LabelField(trianglesError, normalTextStyle);
			if (string.IsNullOrEmpty(submeshError) == false)
				EditorGUILayout.LabelField(submeshError, normalTextStyle);
			if (string.IsNullOrEmpty(materialsError) == false)
				EditorGUILayout.LabelField(materialsError, normalTextStyle);
			if (string.IsNullOrEmpty(meshPathError) == false)
				EditorGUILayout.LabelField(meshPathError, normalTextStyle);
			if (string.IsNullOrEmpty(prefabPathError) == false)
				EditorGUILayout.LabelField(prefabPathError, normalTextStyle);

			EditorGUILayout.Space();
		}

		private void DisplayMaterialSelectedErrors()
		{
			EditorGUILayout.LabelField("<b>Errors detected on the selected material : </b>", littleTitleStyle);

			if (string.IsNullOrEmpty(mainWidthTextureSizeError) == false)
				EditorGUILayout.LabelField(mainWidthTextureSizeError, normalTextStyle);
			if (string.IsNullOrEmpty(mainHeightTextureSizeError) == false)
				EditorGUILayout.LabelField(mainHeightTextureSizeError, normalTextStyle);
			if (string.IsNullOrEmpty(shaderError) == false)
				EditorGUILayout.LabelField(shaderError, normalTextStyle);
			if (string.IsNullOrEmpty(materialPathError) == false)
				EditorGUILayout.LabelField(materialPathError, normalTextStyle);

			EditorGUILayout.Space();
		}

		private void DisplayTextureSelectedErrors()
		{
			EditorGUILayout.LabelField("<b>Errors detected on the selected texture : </b>", littleTitleStyle);

			if (string.IsNullOrEmpty(widthTextureError) == false)
				EditorGUILayout.LabelField(widthTextureError, normalTextStyle);
			if (string.IsNullOrEmpty(heightTextureError) == false)
				EditorGUILayout.LabelField(heightTextureError, normalTextStyle);
			if (string.IsNullOrEmpty(realWidthTextureError) == false)
				EditorGUILayout.LabelField(realWidthTextureError, normalTextStyle);
			if (string.IsNullOrEmpty(realHeightTextureError) == false)
				EditorGUILayout.LabelField(realHeightTextureError, normalTextStyle);
			if (string.IsNullOrEmpty(extensionTextureError) == false)
				EditorGUILayout.LabelField(extensionTextureError, normalTextStyle);
			if (string.IsNullOrEmpty(lengthTextureError) == false)
				EditorGUILayout.LabelField(lengthTextureError, normalTextStyle);
			if (string.IsNullOrEmpty(useCrunchCompressionError) == false)
				EditorGUILayout.LabelField(useCrunchCompressionError, normalTextStyle);
			if (string.IsNullOrEmpty(maxTextureSizeError) == false)
				EditorGUILayout.LabelField(maxTextureSizeError, normalTextStyle);
			if (string.IsNullOrEmpty(texturePathError) == false)
				EditorGUILayout.LabelField(texturePathError, normalTextStyle);

			EditorGUILayout.Space();
		}

		#endregion
	}

}
