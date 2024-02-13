using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetValidator.AssetValidatorEditor
{

	public partial class AssetsValidatorEditor : EditorWindow
	{
		private int listAssetHeight, listMaterialHeight, listAssetsUsingMatHeight, listMaterialsUsingtextureHeight;

		private const int LIST_ASSET_HEIGTH = 242;
		private const int LIST_MATERIAL_HEIGTH = 363;
		private const int LIST_ASSETS_USING_MAT_HEIGTH = 460;
		private const int LIST_MAT_USING_TEX_HEIGTH = 601;

		private const int HEIGHT_SUPP_CONSOLE = 150;
		private const int HEIGHT_SUPP_RULES_SELECTED = 12;
		private const int HEIGHT_SUPP_PARENT_PREFAB = 20;

		private const int HEIGHT_MIN_LIST = 125;

		private Vector2 assetScrollPos;
		private Vector2 materialScrollPos;
		private Vector2 assetUsersScrollPos;
		private Vector2 errorScrollPos;
		private Vector2 textureScrollPos;
		private Vector2 materialUsersScrollPos;

		public const string BuiltinExtraResources = "Resources/unity_builtin_extra";
		public const string BuiltinResources = "Library/unity default resources";

		private Action actionChoose = Action.Nothing;
		enum Action { OpenScene, SelectedWithChild, SelectedWithoutChild, Nothing };

		private void CheckAssetMenu()
		{
			listAssetHeight = LIST_ASSET_HEIGTH;
			listMaterialHeight = LIST_MATERIAL_HEIGTH;
			listAssetsUsingMatHeight = LIST_ASSETS_USING_MAT_HEIGTH;
			listMaterialsUsingtextureHeight = LIST_MAT_USING_TEX_HEIGTH;

			if (checkAssetLimitDone)
			{
				listAssetHeight += HEIGHT_SUPP_CONSOLE;
				listMaterialHeight += HEIGHT_SUPP_CONSOLE;
				listAssetsUsingMatHeight += HEIGHT_SUPP_CONSOLE;
				listMaterialsUsingtextureHeight += HEIGHT_SUPP_CONSOLE;
			}

			DisplayActions();

			if (checkAssetDone == false)
				return;

			UpdateTextErrors();

			EditorGUILayout.BeginHorizontal();
			{
				DisplayGlobalInformations();

				DisplayLightInformations();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			{
				DisplayAssetList();

				if (assetSelected == true)
				{
					EditorGUILayout.BeginVertical();
					{
						DisplayAssetSection();

						EditorGUILayout.BeginHorizontal();
						{
							DisplayMaterialList();

							EditorGUILayout.BeginVertical();
							{
								if (materialSelected == true)
								{
									DisplayMaterialSection();

									EditorGUILayout.BeginHorizontal();
									{
										DisplayAssetsUsingThisMatList();

										DisplayTextureList();

										if (textureSelected == true)
										{
											EditorGUILayout.BeginVertical();
											{
												DisplayTextureSection();

												DisplayMaterialsUsingThisTexList();
											}
											EditorGUILayout.EndVertical();
										}
									}
									EditorGUILayout.EndHorizontal();
								}
							}
							EditorGUILayout.EndVertical();
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
			}
			EditorGUILayout.EndHorizontal();

			if (checkAssetLimitDone)
				DisplayConsole();
		}

		#region Check Asset Display

		private void DisplayActions()
		{
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUILayout.BeginVertical(GUILayout.Width(400));
				{
					if (actionChoose == Action.Nothing)
						EditorGUILayout.LabelField("<b>Please select your gameobjects</b>", mainTextStyle);
					else if (string.IsNullOrEmpty(currentScene) == false)
						EditorGUILayout.LabelField("<b>Current scene : " + currentScene + "</b>", mainTextStyle);

					if (GUILayout.Button("Get current scenes", mainButtonStyle))
					{
						actionChoose = Action.OpenScene;
						ScansAssets();
					}

					if (GUILayout.Button("Get selected gameobjects", mainButtonStyle))
					{
						actionChoose = Action.SelectedWithChild;
						ScansAssets();
					}

					/*if (GUILayout.Button("Add selected gameobject (without children)", mainButtonStyle))
					{
						actionChoose = Action.SelectedWithoutChild;
						ScansAssets();
					}*/
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical(GUILayout.Width(550));
				{
					if (checkAssetDone == true)
					{
						if (string.IsNullOrEmpty(currentFileRules) || currentAssetRules == null)
						{
							EditorGUILayout.BeginVertical();
							{
								EditorGUILayout.LabelField("<b>Please select a rules file in Rules section</b>", mainTextStyle);
								GUILayout.Button("Apply rule", buttonDesactivatedStyle);
							}
							EditorGUILayout.EndVertical();
						}
						else
						{
							EditorGUILayout.BeginVertical();
							{
								EditorGUILayout.LabelField("<b>Current rules file : " + currentFileRules + "</b>", mainTextStyle);
								EditorGUILayout.LabelField("<b>Current rule selected : " + currentAssetRules.name + "</b>", mainTextStyle);
								if (GUILayout.Button("Apply rule", checkLimitButtonStyle))
									ApplyRule();
							}
							EditorGUILayout.EndVertical();

							listAssetHeight += HEIGHT_SUPP_RULES_SELECTED;
							listMaterialHeight += HEIGHT_SUPP_RULES_SELECTED;
							listAssetsUsingMatHeight += HEIGHT_SUPP_RULES_SELECTED;
							listMaterialsUsingtextureHeight += HEIGHT_SUPP_RULES_SELECTED;
						}
					}
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();

			DrawLine(1, new Color(0.5f, 0.5f, 0.5f, 1), 5, 5);
		}

		private void DisplayGlobalInformations()
		{
			EditorGUILayout.BeginVertical(backgroundBlocWithHeightStyle, GUILayout.Width(600));
			{
				EditorGUILayout.LabelField("<b>Global information : </b>", titleStyle);
				EditorGUILayout.BeginHorizontal(contentBlocStyle);
				{
					EditorGUILayout.BeginVertical(GUILayout.Width(450));
					{
						EditorGUILayout.LabelField("GameObject scanned : " + nbGameobjectCheck, normalTextStyle);
						EditorGUILayout.LabelField("Mesh found : " + nbMeshFilterFound, normalTextStyle);
						EditorGUILayout.LabelField("Renderer found : " + nbRendererFound, normalTextStyle);

						string baseTxt;
						if (string.IsNullOrEmpty(sceneInformations.renderPipelineAsset) == false)
							baseTxt = "Render Pipeline Asset use in project : ";
						else
							baseTxt = "Render Pipeline Asset not find";

						if (string.IsNullOrEmpty(renderPipelineAssetError))
							EditorGUILayout.LabelField(baseTxt + sceneInformations.renderPipelineAsset, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>" + baseTxt + sceneInformations.renderPipelineAsset + "</b>", normalErrorTextStyle);
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical(GUILayout.Width(150));
					{
						if (string.IsNullOrEmpty(globalVertexError))
							EditorGUILayout.LabelField("Total vertices : " + globalVertexCount, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Total vertices  : " + globalVertexCount + "</b>", normalErrorTextStyle);

						if (string.IsNullOrEmpty(globalTrianglesError))
							EditorGUILayout.LabelField("Total triangles : " + globalTrianglesCount, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Total triangles : " + globalTrianglesCount + "</b>", normalErrorTextStyle);

						if (string.IsNullOrEmpty(globalSubmeshError))
							EditorGUILayout.LabelField("Total submeshs : " + globalSubmeshCount, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Total submeshs : " + globalSubmeshCount + "</b>", normalErrorTextStyle);

						if (string.IsNullOrEmpty(globalMaterialError))
							EditorGUILayout.LabelField("Total materials : " + globalMaterialsCount, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Total materials : " + globalMaterialsCount + "</b>", normalErrorTextStyle);
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayLightInformations()
		{
			EditorGUILayout.BeginVertical(backgroundBlocWithHeightStyle);
			{
				EditorGUILayout.LabelField("<b>Scene light information : </b>", titleStyle);
				EditorGUILayout.BeginHorizontal(contentBlocStyle);
				{
					EditorGUILayout.BeginVertical(GUILayout.Width(250));
					{
						EditorGUILayout.LabelField("Number of lightmaps : " + sceneInformations.nbLightmaps, normalTextStyle);

						if (string.IsNullOrEmpty(lightmapsLengthError))
							EditorGUILayout.LabelField("Total length lightmaps : " + sceneInformations.sumLightmapsLength + " Mo", normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Total length lightmaps : " + sceneInformations.sumLightmapsLength + " Mo</b>", normalErrorTextStyle);

						if (string.IsNullOrEmpty(lightmapsRealLengthError))
							EditorGUILayout.LabelField("Total real length lightmaps : " + sceneInformations.sumRealLightmapsLength + " Mo", normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Total real length lightmaps : " + sceneInformations.sumRealLightmapsLength + " Mo</b>", normalErrorTextStyle);
					}
					EditorGUILayout.EndVertical();

					float minSize = 400;

					float maxSize = position.width - 250;
					if (maxSize < minSize)
						maxSize = minSize;

					EditorGUILayout.BeginVertical(GUILayout.MinWidth(minSize), GUILayout.MaxWidth(maxSize));
					{
						if (sceneInformations.presenceOfLightSettings)
						{
							if (string.IsNullOrEmpty(mixedLightingModeError))
								EditorGUILayout.LabelField("Mixed Lighting Mode use : " + sceneInformations.lightMode, normalTextStyle);
							else
								EditorGUILayout.LabelField("<b>Mixed Lighting Mode use : " + sceneInformations.lightMode + "</b>", normalErrorTextStyle);

							if (string.IsNullOrEmpty(directionalModeError))
								EditorGUILayout.LabelField("Directional mode in Lightmapping Settings use : " + sceneInformations.directionalMode, normalTextStyle);
							else
								EditorGUILayout.LabelField("<b>Directional mode in Lightmapping Settings use : " + sceneInformations.directionalMode + "</b>", normalErrorTextStyle);
						}
						else
						{
							if (checkAssetLimitDone == false)
								EditorGUILayout.LabelField("No instance of LightingSettings found", normalTextStyle);
							else
								EditorGUILayout.LabelField("<b>No instance of LightingSettings found</b>", normalErrorTextStyle);
						}

						string pathLights = sceneInformations.pathLightmaps;
						EditorGUILayout.BeginHorizontal();
						{
							// Texture2D texture = 
							Texture2D texture = folderImage;
							if (texture == null)
								Debug.LogError(TAG + "Image not find");
							else
							{
								if (GUILayout.Button(texture, miniButton))
								{
									if (string.IsNullOrEmpty(pathLights) == false)
										OpenAssetInFolder(pathLights);
								}
							}

							EditorGUILayout.LabelField("Path folder lights : " + pathLights, normalTextStyle);
						}
						EditorGUILayout.EndHorizontal();

						if (string.IsNullOrEmpty(presenceOfLightsRealtimeError))
							EditorGUILayout.LabelField("Presence of lights realtime : " + (sceneInformations.presenceOfRealtimeLight == 1 ? "Yes" : "No"), normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Presence of lights realtime : " + (sceneInformations.presenceOfRealtimeLight == 1 ? "Yes" : "No") + "</b>", normalErrorTextStyle);

					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayAssetList()
		{
			int height_min_list = HEIGHT_MIN_LIST;
			if (assetSelected == true)
			{
				int diff = listMaterialHeight - listAssetHeight;
				if (string.IsNullOrEmpty(currentAsset.prefabName) == false)
					height_min_list += diff + HEIGHT_SUPP_PARENT_PREFAB;
				else
					height_min_list += diff;
			}
			if (materialSelected == true)
			{
				int diff = listAssetsUsingMatHeight - listMaterialHeight;
				height_min_list += diff;
			}
			if (textureSelected == true)
			{
				int diff = listMaterialsUsingtextureHeight - listAssetsUsingMatHeight;
				height_min_list += diff;
			}

			float height = position.height - listAssetHeight;
			if (height < height_min_list)
				height = height_min_list;

			EditorGUILayout.BeginVertical(marginLeftTopSectionStyle, GUILayout.Width(200), GUILayout.Height(height));
			{
				EditorGUILayout.BeginVertical(backgroundStyle);
				{
					EditorGUILayout.LabelField("<b>Assets list : </b>", titleStyle);
					assetScrollPos = EditorGUILayout.BeginScrollView(assetScrollPos);
					{
						bool currentAssetDisplayed = false;

						if (listAssetError != null)
						{
							foreach (AssetInformations assetInfos in listAssetError)
							{
								if (currentAsset != null && currentAsset == assetInfos)
								{
									if (GUILayout.Button(assetInfos.assetName, buttonActiveStyle))
										AssetButtonAction(assetInfos);
									currentAssetDisplayed = true;
								}
								else
								{
									if (GUILayout.Button(assetInfos.assetName, buttonErrorStyle))
										AssetButtonAction(assetInfos);
								}
							}
						}

						if (listAssetValid != null)
						{
							foreach (AssetInformations assetInfos in listAssetValid)
							{
								if (currentAssetDisplayed == false && currentAsset != null && currentAsset == assetInfos)
								{
									if (GUILayout.Button(assetInfos.assetName, buttonActiveStyle))
										AssetButtonAction(assetInfos);
								}
								else
								{
									if (GUILayout.Button(assetInfos.assetName, buttonStyle))
										AssetButtonAction(assetInfos);
								}
							}
						}
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayAssetSection()
		{
			EditorGUILayout.BeginVertical(backgroundBlocStyle);
			{
				EditorGUILayout.LabelField("<b>Asset " + currentAsset.assetName + "</b>", titleStyle);

				EditorGUILayout.BeginVertical(contentBlocStyle);
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.BeginVertical(GUILayout.Width(200));
						{
							if (string.IsNullOrEmpty(vertexError))
								EditorGUILayout.LabelField("Vertices : " + currentAsset.vertexCount, normalTextStyle);
							else
								EditorGUILayout.LabelField("<b>Vertices : " + currentAsset.vertexCount + "</b>", normalErrorTextStyle);

							if (string.IsNullOrEmpty(trianglesError))
								EditorGUILayout.LabelField("Triangles : " + currentAsset.trianglesCount, normalTextStyle);
							else
								EditorGUILayout.LabelField("<b>Triangles : " + currentAsset.trianglesCount + "</b>", normalErrorTextStyle);
						}
						EditorGUILayout.EndVertical();

						EditorGUILayout.BeginVertical(GUILayout.Width(200));
						{
							if (string.IsNullOrEmpty(submeshError))
								EditorGUILayout.LabelField("Submesh : " + currentAsset.submeshCount, normalTextStyle);
							else
								EditorGUILayout.LabelField("<b>Submesh : " + currentAsset.submeshCount + "</b>", normalErrorTextStyle);

							if (string.IsNullOrEmpty(materialsError))
								EditorGUILayout.LabelField("Material : " + currentAsset.materialsCount, normalTextStyle);
							else
								EditorGUILayout.LabelField("<b>Material : " + currentAsset.materialsCount + " </b>", normalErrorTextStyle);
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginVertical();
					{
						EditorGUILayout.BeginHorizontal();
						{
							Texture2D texture = folderImage;
							if (texture == null)
								Debug.LogError(TAG + "Image not find");
							else if (currentAsset.meshPath != BuiltinResources && currentAsset.meshPath != BuiltinExtraResources)
							{
								if (GUILayout.Button(texture, miniButton))
								{
									if (string.IsNullOrEmpty(currentAsset.meshPath) == false)
										OpenAssetInFolder(currentAsset.meshPath);
								}
							}

							if (string.IsNullOrEmpty(meshPathError))
								EditorGUILayout.LabelField("Mesh path : " + currentAsset.meshPath, normalTextStyle);
							else
								EditorGUILayout.LabelField("<b>Mesh path : " + currentAsset.meshPath + "</b>", normalErrorTextStyle);
						}
						EditorGUILayout.EndHorizontal();

						if (string.IsNullOrEmpty(currentAsset.prefabName) == false)
						{
							EditorGUILayout.LabelField("Nearest Prefab name : " + currentAsset.prefabName, normalTextStyle);

							EditorGUILayout.BeginHorizontal();
							{
								Texture2D texture = folderImage;
								if (texture == null)
									Debug.LogError(TAG + "Image not find");
								else if (currentAsset.prefabPath != BuiltinResources && currentAsset.prefabPath != BuiltinExtraResources)
								{
									if (GUILayout.Button(texture, miniButton))
									{
										if (string.IsNullOrEmpty(currentAsset.prefabPath) == false)
											OpenAssetInFolder(currentAsset.prefabPath);
									}
								}

								if (string.IsNullOrEmpty(prefabPathError))
									EditorGUILayout.LabelField("Nearest prefab path : " + currentAsset.prefabPath, normalTextStyle);
								else
									EditorGUILayout.LabelField("<b>Nearest prefab path : " + currentAsset.prefabPath + "</b>", normalErrorTextStyle);
							}
							EditorGUILayout.EndHorizontal();

							listMaterialHeight += HEIGHT_SUPP_PARENT_PREFAB;
							listAssetsUsingMatHeight += HEIGHT_SUPP_PARENT_PREFAB;
							listMaterialsUsingtextureHeight += HEIGHT_SUPP_PARENT_PREFAB;
						}
						else
							EditorGUILayout.LabelField("No prefab find", normalTextStyle);
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayMaterialList()
		{
			int height_min_list = HEIGHT_MIN_LIST;

			if (materialSelected == true)
			{
				int diff = listAssetsUsingMatHeight - listMaterialHeight;
				height_min_list += diff;
			}
			if (textureSelected == true)
			{
				int diff = listMaterialsUsingtextureHeight - listAssetsUsingMatHeight;
				height_min_list += diff;
			}

			float height = position.height - listMaterialHeight;
			if (height < height_min_list)
			{
				height = height_min_list;
			}

			EditorGUILayout.BeginVertical(marginLeftTopSectionStyle, GUILayout.Width(200), GUILayout.Height(height));
			{
				EditorGUILayout.BeginVertical(backgroundStyle, GUILayout.Height(height - 5));
				{
					EditorGUILayout.LabelField("<b>Materials list : </b>", titleStyle);
					materialScrollPos = EditorGUILayout.BeginScrollView(materialScrollPos);
					{
						List<Material> listMaterial = currentAsset.listMaterials;

						// Get MaterialInformations
						List<MaterialInformations> listMatInfosError = new List<MaterialInformations>();
						List<MaterialInformations> listMatInfosValid = new List<MaterialInformations>();
						foreach (Material mat in listMaterial)
						{
							if (listMaterialError != null)
							{
								foreach (MaterialInformations matInfos in listMaterialError)
								{
									if (matInfos.IsEqual(mat))
									{
										listMatInfosError.Add(matInfos);
									}
								}
							}

							if (listMaterialValid != null)
							{
								foreach (MaterialInformations matInfos in listMaterialValid)
								{
									if (matInfos.IsEqual(mat))
									{
										listMatInfosValid.Add(matInfos);
									}
								}
							}
						}

						if (listMatInfosError != null && listMatInfosError.Count > 0)
							listMatInfosError.Sort();

						if (listMatInfosValid != null && listMatInfosValid.Count > 0)
							listMatInfosValid.Sort();

						bool currentMaterialDisplayed = false;

						// Display material have error
						for (int i = 0; i < listMatInfosError.Count; i++)
						{
							Material mat = listMatInfosError[i].myMaterial;
							if (currentMaterial != null && currentMaterial == mat)
							{
								if (GUILayout.Button(mat.name, buttonActiveStyle))
									MaterialButtonAction(listMatInfosError[i]);
								currentMaterialDisplayed = true;
							}
							else
							{
								if (GUILayout.Button(mat.name, buttonErrorStyle))
									MaterialButtonAction(listMatInfosError[i]);
							}
						}

						// Display material not have error
						for (int i = 0; i < listMatInfosValid.Count; i++)
						{
							Material mat = listMatInfosValid[i].myMaterial;
							if (currentMaterialDisplayed == false && currentMaterial != null && currentMaterial == mat)
							{
								if (GUILayout.Button(mat.name, buttonActiveStyle))
									MaterialButtonAction(listMatInfosValid[i]);
							}
							else
							{
								if (GUILayout.Button(mat.name, buttonStyle))
									MaterialButtonAction(listMatInfosValid[i]);
							}
						}

					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayMaterialSection()
		{
			EditorGUILayout.BeginVertical(backgroundBlocStyle);
			{
				EditorGUILayout.LabelField("<b>Material " + currentMaterial.name + "</b>", titleStyle);

				EditorGUILayout.BeginVertical(contentBlocStyle);
				{
					EditorGUILayout.LabelField("Material occurrences : " + currentMaterialInfos.listOfUsers.Count, normalTextStyle);

					if (string.IsNullOrEmpty(shaderError))
						EditorGUILayout.LabelField("Shader : " + currentMaterialInfos.materialShader, normalTextStyle);
					else
						EditorGUILayout.LabelField("<b>Shader: " + currentMaterialInfos.materialShader + " </b>", normalErrorTextStyle);

					EditorGUILayout.BeginHorizontal();
					{
						Texture2D texture = folderImage;
						if (texture == null)
							Debug.LogError(TAG + "Image not find");
						else if (currentMaterialInfos.materialPath != BuiltinResources && currentMaterialInfos.materialPath != BuiltinExtraResources)
						{
							if (GUILayout.Button(texture, miniButton))
							{
								if (string.IsNullOrEmpty(currentMaterialInfos.materialPath) == false)
									OpenAssetInFolder(currentMaterialInfos.materialPath);
							}
						}

						if (string.IsNullOrEmpty(materialPathError))
							EditorGUILayout.LabelField("Material path : " + currentMaterialInfos.materialPath, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Material path : " + currentMaterialInfos.materialPath + "</b>", normalErrorTextStyle);
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayAssetsUsingThisMatList()
		{
			int height_min_list = HEIGHT_MIN_LIST;

			if (textureSelected == true)
			{
				int diff = listMaterialsUsingtextureHeight - listAssetsUsingMatHeight;
				height_min_list += diff;
			}

			float height = position.height - listAssetsUsingMatHeight;
			if (height < height_min_list)
				height = height_min_list;

			EditorGUILayout.BeginVertical(marginLeftTopSectionStyle, GUILayout.Width(200), GUILayout.Height(height)); // Material list asset users
			{
				EditorGUILayout.BeginVertical(backgroundStyle, GUILayout.Height(height - 5));
				{
					EditorGUILayout.LabelField("<b>Assets using this material : </b>", titleStyle);
					assetUsersScrollPos = EditorGUILayout.BeginScrollView(assetUsersScrollPos);
					{
						foreach (AssetInformations elem in currentMaterialInfos.listOfUsers)
						{
							if (Selection.activeGameObject != null && Selection.activeGameObject == elem.myAsset)
							{
								if (GUILayout.Button(elem.assetName, buttonActiveStyle))
									AssetUserButtonAction(elem);
							}
							else
							{
								if (GUILayout.Button(elem.assetName, buttonStyle))
									AssetUserButtonAction(elem);
							}
						}
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayTextureList()
		{
			int height_min_list = HEIGHT_MIN_LIST;

			if (textureSelected == true)
			{
				int diff = listMaterialsUsingtextureHeight - listAssetsUsingMatHeight;
				height_min_list += diff;
			}

			float height = position.height - listAssetsUsingMatHeight;
			if (height < height_min_list)
				height = height_min_list;

			EditorGUILayout.BeginVertical(marginLeftTopSectionStyle, GUILayout.Width(200), GUILayout.Height(height));
			{
				EditorGUILayout.BeginVertical(backgroundStyle, GUILayout.Height(height - 5));
				{
					EditorGUILayout.LabelField("<b>Texture list : </b>", titleStyle);
					textureScrollPos = EditorGUILayout.BeginScrollView(textureScrollPos);
					{
						bool currentTextureDisplayed = false;

						if (listTextureError != null)
						{
							foreach (TextureInformations textureInfos in listTextureError)
							{
								if (currentMaterialInfos.listTextureInformations.Contains(textureInfos))
								{
									if (currentTextureInfos != null && currentTextureInfos == textureInfos)
									{
										if (GUILayout.Button(textureInfos.textureName, buttonActiveStyle))
											TextureButtonAction(textureInfos);
										currentTextureDisplayed = true;
									}
									else
									{
										if (GUILayout.Button(textureInfos.textureName, buttonErrorStyle))
											TextureButtonAction(textureInfos);
									}
								}
							}
						}

						if (listTextureValid != null)
						{
							foreach (TextureInformations textureInfos in listTextureValid)
							{
								if (currentMaterialInfos.listTextureInformations.Contains(textureInfos))
								{
									if (currentTextureDisplayed == false && currentTextureInfos != null && currentTextureInfos == textureInfos)
									{
										if (GUILayout.Button(textureInfos.textureName, buttonActiveStyle))
											TextureButtonAction(textureInfos);
									}
									else
									{
										if (GUILayout.Button(textureInfos.textureName, buttonStyle))
											TextureButtonAction(textureInfos);
									}
								}
							}
						}
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayTextureSection()
		{
			EditorGUILayout.BeginVertical(backgroundBlocStyle);
			{
				EditorGUILayout.LabelField("<b>Texture " + currentTexture.name + "</b>", titleStyle);

				EditorGUILayout.BeginHorizontal(contentBlocStyle); // Texture infos
				{
					EditorGUILayout.BeginVertical(GUILayout.Width(200));
					{
						if (string.IsNullOrEmpty(widthTextureError) && string.IsNullOrEmpty(heightTextureError))
							EditorGUILayout.LabelField("Size : " + currentTextureInfos.textureWidth + "x" + currentTextureInfos.textureHeight, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Size : " + currentTextureInfos.textureWidth + "x" + currentTextureInfos.textureHeight + "</b>", normalErrorTextStyle);

						EditorGUILayout.LabelField("Type : " + currentTextureInfos.textureType, normalTextStyle);

						if (string.IsNullOrEmpty(useCrunchCompressionError))
							EditorGUILayout.LabelField("Use crunch compression : " + (currentTextureInfos.useCrunchCompression == 1 ? "Yes" : "No"), normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Use crunch compression : " + (currentTextureInfos.useCrunchCompression == 1 ? "Yes" : "No") + "</b>", normalErrorTextStyle);

						if (string.IsNullOrEmpty(lengthTextureError))
							EditorGUILayout.LabelField("Length : " + currentTextureInfos.textureLength + " Mo", normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Length : " + currentTextureInfos.textureLength + " Mo</b>", normalErrorTextStyle);
					}
					EditorGUILayout.EndVertical();

					EditorGUILayout.BeginVertical();
					{
						if (string.IsNullOrEmpty(realWidthTextureError) && string.IsNullOrEmpty(realHeightTextureError))
							EditorGUILayout.LabelField("Real Size : " + currentTextureInfos.textureRealWidth + "x" + currentTextureInfos.textureRealHeight, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Real Size : " + currentTextureInfos.textureRealWidth + "x" + currentTextureInfos.textureRealHeight + "</b>", normalErrorTextStyle);

						if (string.IsNullOrEmpty(extensionTextureError))
							EditorGUILayout.LabelField("Extension : " + currentTextureInfos.textureExtension, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Extension : " + currentTextureInfos.textureExtension + "</b>", normalErrorTextStyle);

						if (string.IsNullOrEmpty(maxTextureSizeError))
							EditorGUILayout.LabelField("Max texture size : " + currentTextureInfos.maxTextureSize, normalTextStyle);
						else
							EditorGUILayout.LabelField("<b>Max texture size : " + currentTextureInfos.maxTextureSize + "</b>", normalErrorTextStyle);
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal(contentBlocStyle);
				{
					Texture2D texture = folderImage;
					if (texture == null)
						Debug.LogError(TAG + "Image not find");
					else if (currentTextureInfos.texturePath != BuiltinResources && currentTextureInfos.texturePath != BuiltinExtraResources)
					{
						if (GUILayout.Button(texture, miniButton))
						{
							if (string.IsNullOrEmpty(currentTextureInfos.texturePath) == false)
								OpenAssetInFolder(currentTextureInfos.texturePath);
						}
					}

					if (string.IsNullOrEmpty(texturePathError))
						EditorGUILayout.LabelField("Texture path : " + currentTextureInfos.texturePath, normalTextStyle);
					else
						EditorGUILayout.LabelField("<b>Texture path : " + currentTextureInfos.texturePath + "</b>", normalErrorTextStyle);
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		private void DisplayMaterialsUsingThisTexList()
		{
			float height = position.height - listMaterialsUsingtextureHeight;
			if (height < HEIGHT_MIN_LIST)
				height = HEIGHT_MIN_LIST;

			EditorGUILayout.BeginVertical(marginLeftTopSectionStyle, GUILayout.Width(200), GUILayout.Height(height));
			{
				EditorGUILayout.BeginVertical(backgroundStyle, GUILayout.Height(height - 5));
				{
					EditorGUILayout.LabelField("<b>Materials using this texture : </b>", titleStyle);

					materialUsersScrollPos = EditorGUILayout.BeginScrollView(materialUsersScrollPos);
					{
						foreach (MaterialInformations matInfos in currentTextureInfos.listOfUsers)
						{
							if (GUILayout.Button(matInfos.materialName, buttonStyle))
								MaterialUserButtonAction(matInfos);
						}
					}
					EditorGUILayout.EndScrollView();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndVertical();
		}

		#endregion
	}

}
