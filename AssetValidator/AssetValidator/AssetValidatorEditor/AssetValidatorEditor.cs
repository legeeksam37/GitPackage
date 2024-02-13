using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections;
using UnityEngine.Networking;

namespace AssetValidator.AssetValidatorEditor
{

	public partial class AssetsValidatorEditor : EditorWindow
	{
		private const string TAG		= "[AV] ";
		private const string NAME		= "Assets Validator...";
		private const string VERSION	= "0.0.9";
		public const string ruleFolderPath = "/ImmersiveNow/AssetValidator/Rules";

		private string[] toolbarStrings = { "Assets Inspector", "Rules" };
		int currentMenu = 0;

		private Vector2 windowScrollPos;

		private static List<AssetInformations> listAssetInformations;
		private static List<MaterialInformations> listMaterialInformations;
		private static List<TextureInformations> listTextureInformations;
		private static SceneInformations sceneInformations;

		private static int globalVertexCount;
		private static int globalTrianglesCount;
		private static int globalSubmeshCount;
		private static int globalMaterialsCount;

		private static int nbGameobjectCheck;
		private static int nbMeshFilterFound;
		private static int nbRendererFound;

		private AssetInformations currentAsset;

		private Material currentMaterial;
		private MaterialInformations currentMaterialInfos;

		private Texture currentTexture;
		private TextureInformations currentTextureInfos;

		private AssetRules currentAssetRules;

		private bool checkAssetDone;
		private static bool checkAssetLimitDone;
		private bool assetSelected;
		private bool materialSelected;
		private bool textureSelected;

		private string currentFileRules;

		private static AssetsValidator assetRulesManager;

		private static string currentScene;

		private GameObject[] selectedGameObjects;

		[MenuItem("Immersive Now/" + NAME)]
		private static void AssetsValidatorWindow()
		{
			EditorWindow wnd = GetWindow<AssetsValidatorEditor>(false, NAME, true);
			wnd.position = new Rect(10, 100, Screen.width, Screen.height);
			wnd.Show();

#if UNITY_EDITOR
			assetRulesManager = new AssetsValidator();
#else
			assetRulesManager = AssetsValidator.instance;
			if (assetRulesManager == null)
				assetRulesManager = new AssetsValidator();
#endif

			Scene scene = SceneManager.GetActiveScene();
			if (scene != null)
				currentScene = scene.name;

			InitializeTextures();
			InitializeStyle();
			InitializeFolderImage();
		}

		private void OnGUI()
		{
			DisplayHeader();

			windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);
			{
				if (currentMenu == 0)
					CheckAssetMenu();
				else
					RulesMenu();
			}
			EditorGUILayout.EndScrollView();
		}

		private void ChangedActiveScene(Scene current, Scene next)
		{
			InitializeTextures();
			InitializeStyle();
			InitializeFolderImage();
		}

		private void DisplayHeader()
		{
			// Toolbar
			EditorGUILayout.BeginHorizontal();
			{
				currentMenu = GUILayout.Toolbar(currentMenu, toolbarStrings, GUILayout.Width(position.width - 100));
				EditorGUILayout.LabelField("Version " + VERSION, versionTextStyle, GUILayout.Width(100));
			}
			EditorGUILayout.EndHorizontal();

			DrawLine(1, new Color(0.5f, 0.5f, 0.5f, 1), 5, 5);
		}

		#region Rule selection

		private void OnSuccessAuth()
		{
			EditorPrefs.SetBool(KEY_RULE_AUTH, true);
			authentificationDone = false;
		}

		private IEnumerator GetRulesRequest(string url)
		{
			if (string.IsNullOrEmpty(url))
				yield return null;

			DownloadHandlerBuffer df = new DownloadHandlerBuffer();
			UnityWebRequest uwr = UnityWebRequest.Get(url);
			uwr.downloadHandler = df;

			yield return uwr.SendWebRequest();

			if (uwr.result != UnityWebRequest.Result.Success)
			{
				Debug.Log(TAG + "UnityWebRequest Error : " + uwr.error);
			}
			else
			{
				Debug.Log(TAG + "Rules file find. Start loading.");
				string result = uwr.downloadHandler.text;
				assetRulesManager.LoadRules(result);

				currentFileRules = EditorPrefs.GetString(AssetValidatorParams.RULE_PROJECT_ID_KEY);
			}
		}

		private void OnFailedAuth()
		{
			string keyRemotePref = EditorPrefs.GetString(AssetValidatorParams.RULE_PROJECT_ID_KEY);
			Debug.Log(TAG + "Project ID " + keyRemotePref + " is not found");
		}

		/*private void OnSuccessAuth(string projectKey, string key, string result)
		{
			EditorPrefs.SetBool(KEY_RULE_AUTH, true);
			EditorPrefs.SetString(KEY_RULE_STORAGE, projectKey);
			EditorPrefs.SetString(RESULT_RULE_STORAGE, result);
			authentificationDone = false;

			Debug.Log(TAG + "Key authorize. Path file load : " + key);
		}

		private void LoadRules(string key, string result)
		{
			Debug.Log(TAG + "Rule content load for project ID : " + key);
			currentFileRules = key.ToUpper();
			assetRulesManager.LoadRules(result);
			currentAssetRules = null;
		}

		private void OnFailedAuth(string key, string result)
		{
			//EditorPrefs.SetBool(KEY_RULE_AUTH, false);
			//authentificationDone = false;
			Debug.Log(TAG + "Key try : " + key + " - Result get : " + result);
		}*/

		#endregion

		#region Button actions

		private void ScansAssets(bool isReload = false)
		{
			Scene scene = SceneManager.GetActiveScene();
			if (scene != null)
				currentScene = scene.name;

#if UNITY_EDITOR
			EditorSceneManager.activeSceneChangedInEditMode -= ChangedActiveScene;
			EditorSceneManager.activeSceneChangedInEditMode += ChangedActiveScene;
#else
			SceneManager.activeSceneChanged -= ChangedActiveScene;
			SceneManager.activeSceneChanged += ChangedActiveScene;
#endif

			if (actionChoose == Action.OpenScene)
				AssetsSceneButtonAction(isReload);
			else if (actionChoose == Action.SelectedWithChild)
				AssetsSelectionWithChildButtonAction(isReload);
			else if (actionChoose == Action.SelectedWithChild)
				AssetsSelectionWithoutChildButtonAction(isReload);
		}

		private void AssetsSceneButtonAction(bool isReload = false)
		{
			GameObject[] gameObjects = FindObjectsOfType<GameObject>(true);
			CheckAsset(gameObjects);
			if (listAssetInformations != null && listAssetInformations.Count > 0)
				listAssetInformations.Sort();

			listAssetValid = new List<AssetInformations>();
			listMaterialValid = new List<MaterialInformations>();
			listTextureValid = new List<TextureInformations>();

			listAssetError = new List<AssetInformations>();
			listMaterialError = new List<MaterialInformations>();
			listTextureError = new List<TextureInformations>();

			listAssetValid.AddRange(listAssetInformations);
			listMaterialValid.AddRange(listMaterialInformations);
			listTextureValid.AddRange(listTextureInformations);

			checkAssetDone = true;

			//if (isReload == false)
			//{
			assetSelected = false;
			materialSelected = false;
			textureSelected = false;
			//}
		}

		private void AssetsSelectionWithChildButtonAction(bool isReload = false)
		{
			GameObject[] gameObjects;
			if (isReload)
				gameObjects = selectedGameObjects;
			else
			{
				gameObjects = Selection.gameObjects;
				selectedGameObjects = gameObjects;
			}

			List<GameObject> allGameobject = new List<GameObject>();
			foreach (GameObject gameObject in gameObjects)
			{
				if (gameObject != null)
				{
					if (allGameobject.Contains(gameObject) == false)
					{
						List<GameObject> listOfChildren = new List<GameObject>();
						GetChildRecursive(gameObject, listOfChildren);

						allGameobject.Add(gameObject);

						foreach (GameObject child in listOfChildren)
						{
							if (allGameobject.Contains(child) == false)
								allGameobject.Add(child);
						}
					}
				}
			}

			CheckAsset(allGameobject.ToArray());
			if (listAssetInformations != null && listAssetInformations.Count > 0)
				listAssetInformations.Sort();

			listAssetValid = new List<AssetInformations>();
			listMaterialValid = new List<MaterialInformations>();
			listTextureValid = new List<TextureInformations>();

			listAssetError = new List<AssetInformations>();
			listMaterialError = new List<MaterialInformations>();
			listTextureError = new List<TextureInformations>();

			listAssetValid.AddRange(listAssetInformations);
			listMaterialValid.AddRange(listMaterialInformations);
			listTextureValid.AddRange(listTextureInformations);

			checkAssetDone = true;

			assetSelected = false;
			materialSelected = false;
			textureSelected = false;
		}

		private void AssetsSelectionWithoutChildButtonAction(bool isReload = false)
		{
			GameObject[] gameObjects = Selection.gameObjects;
			CheckAsset(gameObjects);
			if (listAssetInformations != null && listAssetInformations.Count > 0)
				listAssetInformations.Sort();

			listAssetValid = new List<AssetInformations>();
			listMaterialValid = new List<MaterialInformations>();
			listTextureValid = new List<TextureInformations>();

			listAssetError = new List<AssetInformations>();
			listMaterialError = new List<MaterialInformations>();
			listTextureError = new List<TextureInformations>();

			listAssetValid.AddRange(listAssetInformations);
			listMaterialValid.AddRange(listMaterialInformations);
			listTextureValid.AddRange(listTextureInformations);

			checkAssetDone = true;

			assetSelected = false;
			materialSelected = false;
			textureSelected = false;
		}

		private void AssetButtonAction(AssetInformations asset)
		{
			if (asset == null)
				return;

			currentAsset = asset;
			assetSelected = true;
			Selection.activeObject = currentAsset.myAsset;

			materialSelected = false;
			currentMaterialInfos = null;
			currentMaterial = null;

			textureSelected = false;
			currentTexture = null;
			currentTextureInfos = null;
		}

		private void MaterialButtonAction(MaterialInformations matInfos)
		{
			materialSelected = true;
			currentMaterial = matInfos.myMaterial;
			currentMaterialInfos = matInfos;

			textureSelected = false;
			currentTexture = null;
			currentTextureInfos = null;
		}

		private void TextureButtonAction(TextureInformations textureInfos)
		{
			textureSelected = true;
			currentTexture = textureInfos.myTexture;
			currentTextureInfos = textureInfos;
		}

		private void AssetUserButtonAction(AssetInformations asset)
		{
			Selection.activeObject = asset.myAsset;
		}

		private void MaterialUserButtonAction(MaterialInformations matInfos)
		{
			Selection.activeObject = matInfos.myMaterial;
		}

		private void OpenAssetInFolder(string path)
		{
			string[] pathTab = path.Split('/');
			string pathResource = "/";
			if (pathTab != null && pathTab.Length > 0)
			{
				for (int i = 1; i < pathTab.Length; i++)
					pathResource += pathTab[i] + "/";

				pathResource = pathResource.Substring(0, pathResource.Length - 1);
			}

			EditorUtility.RevealInFinder(Application.dataPath + pathResource);
		}

		private void AssetRulesButtonAction(AssetRules assetRules)
		{
			currentAssetRules = assetRules;
		}

		#endregion

		#region Check assets

		private void CheckAsset(GameObject[] gameObjects)
		{
			// Clear list error
			listErrorResult = null;

			// get informations
			CheckAssetsScene(gameObjects);

			// debug display informations
			if (debug)
				Display();

			checkAssetLimitDone = false;
		}

		private void CheckAssetsScene(GameObject[] gameObjects)
		{
			if (debug)
				Debug.Log(TAG + "--- Nb gameObjects : " + gameObjects.Length);

			globalVertexCount = 0;
			globalTrianglesCount = 0;
			globalSubmeshCount = 0;
			globalMaterialsCount = 0;

			nbGameobjectCheck = gameObjects.Length;
			nbMeshFilterFound = 0;
			nbRendererFound = 0;

			listAssetInformations = new List<AssetInformations>();
			foreach (GameObject obj in gameObjects)
			{
				if (obj.TryGetComponent(out MeshFilter mf))
				{
					Mesh mesh = mf.sharedMesh;
					if (mesh != null)
					{
						AssetInformations assetInfos = new AssetInformations(obj, mesh);
						assetInfos.CheckAsset();
						listAssetInformations.Add(assetInfos);

						globalVertexCount += assetInfos.vertexCount;
						globalTrianglesCount += assetInfos.trianglesCount;
						globalSubmeshCount += assetInfos.submeshCount;

						if (assetInfos.meshFound)
							nbMeshFilterFound += 1;

						if (assetInfos.rendererFound)
							nbRendererFound += 1;
					}
				}
			}

			listMaterialInformations = new List<MaterialInformations>();
			foreach (AssetInformations assetInfos in listAssetInformations)
			{
				foreach (Material mat in assetInfos.listMaterials)
				{
					bool matAlreadyExist = false;
					foreach (MaterialInformations matInfos in listMaterialInformations)
					{
						if (matInfos.AlreadyExist(mat, assetInfos))
						{
							matAlreadyExist = true;
							assetInfos.AddMaterialInformations(matInfos);
							break;
						}
					}

					if (matAlreadyExist == false)
					{
						MaterialInformations materialInfos = new MaterialInformations(mat, assetInfos);
						materialInfos.CheckMaterial();
						listMaterialInformations.Add(materialInfos);

						assetInfos.AddMaterialInformations(materialInfos);

						globalMaterialsCount += 1;
					}
				}
			}

			listTextureInformations = new List<TextureInformations>();
			foreach (MaterialInformations matInfos in listMaterialInformations)
			{
				for (int i = 0; i < matInfos.listTextures.Count; i++)
				{
					Texture texture = matInfos.listTextures[i];

					bool textureAlreadyExist = false;
					foreach (TextureInformations textureInfos in listTextureInformations)
					{
						if (textureInfos.AlreadyExist(texture, matInfos))
						{
							textureAlreadyExist = true;
							matInfos.AddTextureInformations(textureInfos);
							break;
						}
					}

					if (textureAlreadyExist == false)
					{
						TextureInformations textureInfos = new TextureInformations(texture, matInfos, matInfos.listTypesOfTextures[i]);
						textureInfos.CheckTexture();
						listTextureInformations.Add(textureInfos);

						matInfos.AddTextureInformations(textureInfos);
					}
				}
			}

			sceneInformations = new SceneInformations(globalMaterialsCount);
			sceneInformations.CheckScene();
		}

		#endregion

	}

}
