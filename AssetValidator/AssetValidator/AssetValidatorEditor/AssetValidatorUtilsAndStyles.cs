using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetValidator.AssetValidatorEditor
{

	public partial class AssetsValidatorEditor : EditorWindow
	{
		private static GUIStyle horizontalLineStyle;

		private static GUIStyle titleStyle;
		private static GUIStyle littleTitleStyle;
		private static GUIStyle normalTextStyle;
		private static GUIStyle versionTextStyle;
		private static GUIStyle normalErrorTextStyle;
		private static GUIStyle normalValidTextStyle;
		private static GUIStyle mainTextStyle;
		private static GUIStyle textFieldStyle;

		private static GUIStyle backgroundStyle;
		private static GUIStyle backgroundBlocStyle;
		private static GUIStyle backgroundBlocWithHeightStyle;
		private static GUIStyle backgroundTextFieldStyle;

		private static GUIStyle errorSectionStyle;

		private static GUIStyle buttonStyle;
		private static GUIStyle buttonActiveStyle;
		private static GUIStyle buttonErrorStyle;
		private static GUIStyle buttonDesactivatedStyle;
		private static GUIStyle miniButton;
		private static GUIStyle mainButtonStyle;
		private static GUIStyle checkLimitButtonStyle;

		private static GUIStyle pluginSettingsButtonStyle;
		private static GUIStyle leftPaddingStyle;

		private static GUIStyle marginSectionStyle;
		private static GUIStyle contentBlocStyle;
		private static GUIStyle marginLeftTopSectionStyle;
		private static GUIStyle listMarginStyle;

		private static Texture2D textureDarkColor;
		private static Texture2D textureGrayColor;
		private static Texture2D textureGrayLightColor;
		private static Texture2D textureRedColor;
		private static Texture2D textureBlueColor;

		private static bool debug = false;
		private static bool debugError = false;

		private const string base64FolderPNG = "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";

		private static Texture2D folderImage;

		#region Style

		private static void InitializeFolderImage()
        {
			folderImage = new Texture2D(2, 2);
			byte[] data = Convert.FromBase64String(base64FolderPNG);
			folderImage.LoadImage(data);
		}

		private static void InitializeTextures()
		{
			Color32 darkColor = new Color32(40, 40, 40, 255);
			Color32 grayColor = new Color32(50, 50, 50, 255);
			Color32 grayLightColor = new Color32(80, 80, 80, 255);
			Color32 redColor = new Color32(255, 0, 0, 255);
			Color32 blueColor = new Color32(6, 66, 189, 255);

			textureDarkColor = MakeTex(2, 2, darkColor);
			textureGrayColor = MakeTex(2, 2, grayColor);
			textureGrayLightColor = MakeTex(2, 2, grayLightColor);
			textureRedColor = MakeTex(2, 2, redColor);
			textureBlueColor = MakeTex(2, 2, blueColor);
		}

		private static void InitializeStyle()
		{
			// header button
			buttonDesactivatedStyle = new GUIStyle("button");
			buttonDesactivatedStyle.fixedHeight = 30;
			buttonDesactivatedStyle.fixedWidth = 450;
			buttonDesactivatedStyle.normal.background = textureDarkColor;
			buttonDesactivatedStyle.normal.textColor = Color.white;
			buttonDesactivatedStyle.margin = new RectOffset(50, 50, 0, 0);

			mainButtonStyle = new GUIStyle("button");
			mainButtonStyle.fixedWidth = 300;
			mainButtonStyle.margin = new RectOffset(50, 50, 0, 0);

			checkLimitButtonStyle = new GUIStyle("button");
			checkLimitButtonStyle.fixedHeight = 30;
			checkLimitButtonStyle.margin = new RectOffset(50, 50, 0, 0);
			// end header button

			versionTextStyle = new GUIStyle();
			versionTextStyle.fontSize = 12;
			versionTextStyle.richText = true;
			versionTextStyle.normal.background = null;
			versionTextStyle.normal.textColor = Color.white;
			versionTextStyle.clipping = TextClipping.Clip;
			versionTextStyle.alignment = TextAnchor.UpperLeft;
			versionTextStyle.padding = new RectOffset(5, 0, 0, 0);

			horizontalLineStyle = new GUIStyle();
			horizontalLineStyle.normal.background = EditorGUIUtility.whiteTexture;
			horizontalLineStyle.margin = new RectOffset(0, 0, 10, 10);

			titleStyle = new GUIStyle();
			titleStyle.fontSize = 14;
			titleStyle.richText = true;
			titleStyle.normal.textColor = Color.white;
			titleStyle.clipping = TextClipping.Clip;

			littleTitleStyle = new GUIStyle();
			littleTitleStyle.fontSize = 12;
			littleTitleStyle.richText = true;
			littleTitleStyle.normal.textColor = Color.white;
			littleTitleStyle.clipping = TextClipping.Clip;

			normalTextStyle = new GUIStyle();
			normalTextStyle.fontSize = 12;
			normalTextStyle.richText = true;
			normalTextStyle.normal.background = null;
			normalTextStyle.normal.textColor = Color.white;
			normalTextStyle.alignment = TextAnchor.MiddleLeft;
			normalTextStyle.clipping = TextClipping.Clip;

			normalErrorTextStyle = new GUIStyle();
			normalErrorTextStyle.fontSize = 12;
			normalErrorTextStyle.richText = true;
			normalErrorTextStyle.normal.background = null;
			normalErrorTextStyle.normal.textColor = Color.red;
			normalErrorTextStyle.clipping = TextClipping.Clip;

			normalValidTextStyle = new GUIStyle();
			normalValidTextStyle.fontSize = 12;
			normalValidTextStyle.richText = true;
			normalValidTextStyle.normal.background = null;
			normalValidTextStyle.normal.textColor = Color.green;
			normalValidTextStyle.clipping = TextClipping.Clip;

			mainTextStyle = new GUIStyle();
			mainTextStyle.fontSize = 14;
			mainTextStyle.richText = true;
			mainTextStyle.normal.textColor = Color.white;
			mainTextStyle.clipping = TextClipping.Clip;
			mainTextStyle.padding = new RectOffset(50, 0, 0, 0);

			textFieldStyle = new GUIStyle("textField");
			textFieldStyle.fontSize = 12;
			textFieldStyle.richText = true;
			textFieldStyle.clipping = TextClipping.Clip;
			textFieldStyle.margin = new RectOffset(0, 0, 0, 0);

			leftPaddingStyle = new GUIStyle();
			leftPaddingStyle.padding = new RectOffset(50, 0, 0, 0);

			marginSectionStyle = new GUIStyle();
			marginSectionStyle.padding = new RectOffset(0, 0, 7, 0);

			contentBlocStyle = new GUIStyle();
			contentBlocStyle.padding = new RectOffset(5, 0, 2, 0);

			marginLeftTopSectionStyle = new GUIStyle();
			marginLeftTopSectionStyle.padding = new RectOffset(5, 0, 5, 0);

			backgroundStyle = new GUIStyle();
			backgroundStyle.padding = new RectOffset(0, 0, 0, 0);
			backgroundStyle.normal.background = textureDarkColor;

			backgroundTextFieldStyle = new GUIStyle();
			backgroundTextFieldStyle.padding = new RectOffset(0, 0, 0, 0);
			backgroundTextFieldStyle.normal.background = textureGrayLightColor;

			backgroundBlocStyle = new GUIStyle();
			backgroundBlocStyle.margin = new RectOffset(5, 5, 5, 0);
			backgroundBlocStyle.padding = new RectOffset(5, 5, 5, 5);
			backgroundBlocStyle.normal.background = textureDarkColor;

			backgroundBlocWithHeightStyle = new GUIStyle();
			backgroundBlocWithHeightStyle.fixedHeight = 114;
			backgroundBlocWithHeightStyle.margin = new RectOffset(5, 5, 5, 0);
			backgroundBlocWithHeightStyle.padding = new RectOffset(5, 5, 5, 5);
			backgroundBlocWithHeightStyle.normal.background = textureDarkColor;

			errorSectionStyle = new GUIStyle();
			errorSectionStyle.margin = new RectOffset(5, 5, 0, 5);
			errorSectionStyle.padding = new RectOffset(0, 0, 2, 2);
			errorSectionStyle.normal.background = textureDarkColor;

			pluginSettingsButtonStyle = new GUIStyle("button");
			pluginSettingsButtonStyle.fixedWidth = 80;
			pluginSettingsButtonStyle.fixedHeight = 18;
			pluginSettingsButtonStyle.margin = new RectOffset(5, 5, 0, 0);

			// list button
			buttonStyle = new GUIStyle();
			buttonStyle.fixedWidth = 195;
			buttonStyle.fixedHeight = 30;
			buttonStyle.normal.background = textureGrayLightColor;
			buttonStyle.normal.textColor = Color.white;
			buttonStyle.active.background = textureGrayColor;
			buttonStyle.active.textColor = Color.white;
			buttonStyle.margin = new RectOffset(0, 0, 2, 2);
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			buttonStyle.clipping = TextClipping.Clip;
			buttonStyle.padding = new RectOffset(5, 5, 0, 0);

			buttonActiveStyle = new GUIStyle();
			buttonActiveStyle.fixedWidth = 195;
			buttonActiveStyle.fixedHeight = 30;
			buttonActiveStyle.normal.background = textureBlueColor;
			buttonActiveStyle.normal.textColor = Color.white;
			buttonActiveStyle.margin = new RectOffset(0, 0, 2, 2);
			buttonActiveStyle.alignment = TextAnchor.MiddleLeft;
			buttonActiveStyle.clipping = TextClipping.Clip;
			buttonActiveStyle.padding = new RectOffset(5, 5, 0, 0);

			buttonErrorStyle = new GUIStyle();
			buttonErrorStyle.fixedWidth = 195;
			buttonErrorStyle.fixedHeight = 30;
			buttonErrorStyle.normal.background = textureRedColor;
			buttonErrorStyle.normal.textColor = Color.black;
			buttonErrorStyle.active.background = textureGrayColor;
			buttonErrorStyle.active.textColor = Color.white;
			buttonErrorStyle.margin = new RectOffset(0, 0, 2, 2);
			buttonErrorStyle.alignment = TextAnchor.MiddleLeft;
			buttonErrorStyle.clipping = TextClipping.Clip;
			buttonErrorStyle.padding = new RectOffset(5, 5, 0, 0);
			// end list button

			miniButton = new GUIStyle("button");
			miniButton.fixedHeight = 18;
			miniButton.fixedWidth = 18;
			miniButton.padding = new RectOffset(3, 3, 3, 3);

			listMarginStyle = new GUIStyle();
			listMarginStyle.margin = new RectOffset(5, 0, 0, 0);
		}
		#endregion

		#region Utility methods

		private static void GetChildRecursive(GameObject obj, List<GameObject> childrens)
		{
			if (obj == null)
			{
				Debug.LogError(TAG + "Error - GetChildRecursive : obj null");
				return;
			}

			foreach (Transform child in obj.transform)
			{
				if (child == null)
					continue;

				childrens.Add(child.gameObject);
				GetChildRecursive(child.gameObject, childrens);
			}
		}

		private static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] pix = new Color[width * height];
			for (int i = 0; i < pix.Length; ++i)
			{
				pix[i] = col;
			}
			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();
			return result;
		}

		private static void DrawLine(int height, Color color, int leftMargin = 0, int rightMargin = 0)
		{
			horizontalLineStyle.fixedHeight = height;
			horizontalLineStyle.margin = new RectOffset(leftMargin, rightMargin, 10, 10);

			var c = GUI.color;
			GUI.color = color;
			GUILayout.Box(GUIContent.none, horizontalLineStyle);
			GUI.color = c;
		}

		private void CheckAndDisplay(string msg, string defaultValue)
		{
			string[] msgs = msg.Split(':');
			string value = msgs[1].Substring(1);

			if (value != defaultValue)
				EditorGUILayout.LabelField(msg, normalTextStyle);
		}

		#endregion

		#region Debug display

		private static void Display()
		{
			Debug.Log(TAG + "--- nbMeshFilterFound : " + nbMeshFilterFound);
			Debug.Log(TAG + "--- nbRendererFound : " + nbRendererFound);

			Debug.Log(TAG + "--- globalVerticesCount : " + globalVertexCount);
			Debug.Log(TAG + "--- globaltrianglesCount : " + globalTrianglesCount);
			Debug.Log(TAG + "--- globalsubmeshCount : " + globalSubmeshCount);
			Debug.Log(TAG + "--- globalmaterialsCount : " + globalMaterialsCount);

			foreach (AssetInformations assetInfos in listAssetInformations)
			{
				string output = TAG + "--- " + assetInfos.assetName + " - ";

				output += "verticesCount : " + assetInfos.vertexCount + ", ";
				output += "trianglesCount : " + assetInfos.trianglesCount + ", ";
				output += "submeshCount : " + assetInfos.submeshCount + ", ";
				output += "materialsCount : " + assetInfos.materialsCount;

				if (assetInfos.materialsCount > 0)
					output += "\nList of materials : ";

				List<MaterialInformations> listMaterialsInfos = assetInfos.listMaterialInformations;
				for (int i = 0; i < listMaterialsInfos.Count; i++)
				{
					output += "\n   material[" + i + "] - ";
					output += "name : " + listMaterialsInfos[i].materialName + ", ";
					output += "shader : " + listMaterialsInfos[i].materialShader;

					List<AssetInformations> listAssetInformations = listMaterialsInfos[i].listOfUsers;
					output += ", nbOcurrences : " + listAssetInformations.Count;
					output += "\n   List of assets users : ";
					foreach (AssetInformations elem in listAssetInformations)
					{
						output += "\n      " + elem.assetName;
					}
				}

				Debug.Log(output, assetInfos.myAsset);
			}
		}

		private static void DisplayError()
		{
			// All is fine !
			if (listErrorResult.Count == 0)
			{
				string output = TAG + "--- The selected assets are correct";
				Debug.Log(output);
				return;
			}

			// Display each error asset & material for each asset
			foreach (AssetRulesResult assetResult in listErrorResult)
			{
				AssetInformations asset = assetResult.asset;
				if (asset == null) // Display Global result
				{
					string output = TAG + "--- Rules[" + assetResult.rulesName + "] - Global result : ";

					foreach (DataResult dataResult in assetResult.dataMsgs)
					{
						output += "\n   " + dataResult.errorMsg;
					}

					Debug.LogError(output);
				}
				else
				{
					string output = TAG + "--- Rules[" + assetResult.rulesName + "] - Asset[" + asset.assetName + "] : ";

					foreach (DataResult dataResult in assetResult.dataMsgs)
					{
						output += "\n   " + dataResult.errorMsg;
					}

					List<MaterialRulesResult> listMatResult = assetResult.listMaterialRulesResult;
					if (listMatResult.Count > 0)
					{
						output += "\n   Materials error : ";
						for (int i = 0; i < listMatResult.Count; i++)
						{
							Material mat = listMatResult[i].material;
							output += "\n      Material " + mat.name + " : ";
							foreach (DataResult dataResult in listMatResult[i].dataMsgs)
							{
								output += "\n         " + dataResult.errorMsg;
							}
						}
					}

					Debug.LogError(output);
				}
			}
		}

		#endregion
	}

}
