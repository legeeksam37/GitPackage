using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using XRTools.IN_MiniJSON;
using XRWebService;
using UnityEditor;
using UnityEngine.Networking;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace AddressableBuilder
{
    public static class INAddressables_Editor
    {
        private const string TAG = "[INAddressables] ";

        public const string CONFIG_DOOR_KEY                 = "ConfigDoors";
        public const string ADDRESSABLES_SCENE_NAME         = "addressableScene.json";
        public const string APP_MODE_SCENE_NAME             = "appMode.json";
        public const string SPAWN_BASE_KEY                  = "Spawn";
        public const string CONFIG_PLAYER_4DS_KEY           = "ConfigPlayer4DS";
        public const string CONFIG_PLAYER_2D_KEY            = "ConfigPlayer2D";
        public const string CONFIG_PLAYER_360_KEY           = "ConfigPlayer360";

        private const string CONFIG_LIVINGROOM_KEY          = "ConfigLivingRoom";
        private const string BASE_POSITION_ROOM_KEY         = "position";
        private const string BASE_VERSION_ROOM_KEY          = "minVersion";
        private const string BASE_VERSION_WARNING_ROOM_KEY  = "minVersionWarning";

		public const string ADDRESSABLE_BASE_DEV	        =  "https://addressable.dev.immersivenow.orange.com/";
		public const string ADDRESSABLE_BASE_PROD	        =  "https://addressable.prod.immersivenow.orange.com/";

        private const string BASE_DOOR_URL_IMG              =  "Data/Image/Doors/arche_texture_addbuilder.png";
        private const string BASE_POSITION_VALUE            = "A";
        private const string BASE_POSITION_DOOR_VALUE       = "A";
        private const string BASE_TITLE_VALUE               = "Addressables Builder";
        private const string BASE_VERSION_VALUE             = "2.0.4";
        private const string BASE_VERSION_WARNING_VALUE     = "Update the app to use the event";

        private static string CreateAddressableScene()
		{
            Dictionary<string, object> dicoAddressableScene = new Dictionary<string, object>();

            List< object> lstDoor         = GetDoorsLstInfo();
            if (lstDoor!=null && lstDoor.Count>0 )
                dicoAddressableScene[CONFIG_DOOR_KEY] = lstDoor;
            
            List< object>  lstPlayer4DS    = GetPlayer4DSLstInfo();
            if (lstPlayer4DS!=null && lstPlayer4DS.Count>0 )
                dicoAddressableScene[CONFIG_PLAYER_4DS_KEY] = lstPlayer4DS;

            List< object>  lstPlayer2D    = GetPlayer2DLstInfo();
            if (lstPlayer2D!=null && lstPlayer2D.Count>0 )
                dicoAddressableScene[CONFIG_PLAYER_2D_KEY] = lstPlayer2D;

            List< object>  lstPlayer360    = GetPlayer360LstInfo();
            if (lstPlayer360!=null && lstPlayer360.Count>0 )
                dicoAddressableScene[CONFIG_PLAYER_360_KEY] = lstPlayer360;

            string serializedSceneObjects = Json.Serialize(dicoAddressableScene);

            return serializedSceneObjects;
		}

        private static string CreateAppMode(string sceneName)
		{
            Dictionary<string, object> dicoAppMode = GenerateAppMode(sceneName);

            if (dicoAppMode == null || dicoAppMode.Count == 0)
            {
                Debug.Log(TAG + "AppMode not found");
                return null;
            }

            string serializedappMode = Json.Serialize(dicoAppMode);

            return  serializedappMode;
		}

        private static IEnumerator UploadSceneObjects(string filePath, string sceneName) //  Doors Export
        {
            if (string.IsNullOrEmpty(filePath))
                yield break;


            //yield return EditorCoroutineUtility.StartCoroutine(getRequest(), this);
            string filesString = string.Empty;

            Dictionary<string, object> dicoAppMode = GenerateAppMode(sceneName);

            if (dicoAppMode == null || dicoAppMode.Count == 0)
            {
                Debug.Log(TAG + "AppMode not found");
                yield break;
            }

            INAddressableObjectType.ReindexAll();


            // Create appMode
            string serializedappMode = CreateAppMode(sceneName);
            if (string.IsNullOrEmpty(serializedappMode))
            {
                Debug.Log(TAG + "AppMode not found");
                yield break;
            }
            string appModePath = filePath + APP_MODE_SCENE_NAME;
            yield return CreateFileCoroutine(appModePath, serializedappMode);

            // Create Addressable Scene
            string serializedSceneObjects = CreateAddressableScene();
            string addressableScenePath = filePath + ADDRESSABLES_SCENE_NAME;
            if (!string.IsNullOrEmpty(serializedSceneObjects))
                yield return CreateFileCoroutine(addressableScenePath, serializedSceneObjects);

            // Get project ID
            string keyRemotePref = EditorPrefs.GetString(INAddressables_Params.PROJECT_ID_KEY);
            if (string.IsNullOrEmpty(keyRemotePref))
                yield break;

            // Find Partner
            XRAddressablePartner currentPartner = INAddressables_Params.AddressableList.GetPartner(keyRemotePref);
            if (currentPartner == null)
                yield break;

            // get File
            filesString += addressableScenePath + ";";
            filesString += appModePath + ";";
            filesString = filesString.Substring(0, filesString.Length - 1);// remove the last useless ;

            /*
            string storage = currentPartner.m_StorageSA;
            string jsonString = storage;
            jsonString = UnityWebRequest.EscapeURL(jsonString);

            Process.Start(Application.dataPath + @"/../ImmersiveNow/bin/Upload.exe", jsonString + " " + filesString + " " + currentPartner.m_DevBaseUri);
            */

            // Upload Files
            INAddressables_Upload.UploadFiles(currentPartner , filesString);

            yield return null;
        }

        public static IEnumerator UploadEditor(string filePath, string sceneName)
        {
            yield return UploadSceneObjects(filePath, sceneName);
        }

        private static List<object> GetDoorsLstInfo()
        {
            INAddressablesDoor[] doors = GameObject.FindObjectsOfType<INAddressablesDoor>(true);

            if (doors == null || doors.Length <= 0)
                return null;

            List<object> configDoors = new List<object>();

            for (int i = 0; i < doors.Length; i++)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                data.Add(INAddressablesDoor.URL_IMAGE_KEY, doors[i].urlImage);
                data.Add(INAddressablesDoor.IS_ADDRESSABLE_KEY, doors[i].addressableBuilder);
                data.Add(INAddressablesDoor.CUSTOM_PREFAB_PATH, doors[i].customPrefabPath);

                if (!string.IsNullOrEmpty(doors[i].goToScene))
                    data.Add(INAddressablesDoor.ACTION_KEY, doors[i].goToScene);
                else
                    data.Add(INAddressablesDoor.ACTION_KEY, INAddressablesDoor.DEFAULT_ACTION_KEY);

                string positionDoor = doors[i].gameObject.name;
                data.Add(INAddressablesDoor.POSITION_DOOR_KEY, positionDoor);

                Dictionary<string, object> dictObj = new Dictionary<string, object>();
                dictObj.Add(Title.EN_KEY, doors[i].titles.EN);

                Dictionary<string, object> dicoTitleEN = new Dictionary<string, object>();
                Dictionary<string, object> dicoTitleFR = new Dictionary<string, object>();
                Dictionary<string, object> dicoTitleES = new Dictionary<string, object>();
                dicoTitleEN.Add(Title.EN_KEY, doors[i].titles.EN);
                dicoTitleFR.Add(Title.FR_KEY, doors[i].titles.FR);
                dicoTitleES.Add(Title.ES_KEY, doors[i].titles.ES);

                List<Dictionary<string, object>> lstTitles = new List<Dictionary<string, object>>();
                lstTitles.Add(dicoTitleEN);
                lstTitles.Add(dicoTitleFR);
                lstTitles.Add(dicoTitleES);

                data.Add(INAddressablesDoor.TITLE_KEY, lstTitles);
                if (!string.IsNullOrEmpty(doors[i].targetPlayerPosition))
                    data.Add(INAddressablesDoor.POSITION_PLAYER_KEY, doors[i].targetPlayerPosition);
                else
                    data.Add(INAddressablesDoor.POSITION_PLAYER_KEY, INAddressablesDoor.DEFAULT_POSITION_PLAYER_KEY);
                configDoors.Add(data);
            }

            return configDoors;
        }

        private static List<object> GetPlayer4DSLstInfo()
        {
            INAddressablesPlayer4DS[] players4DS = GameObject.FindObjectsOfType<INAddressablesPlayer4DS>(true);

            if (players4DS == null || players4DS.Length <= 0)
                return null;

            List<object> configPlayers = new List<object>();

            for (int i = 0; i < players4DS.Length; i++)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                data.Add(INAddressablesPlayer4DS.URL_VIDEO_KEY, players4DS[i].urlVideo);
                data.Add(INAddressablesPlayer4DS.POSITION_KEY, players4DS[i].name);

                configPlayers.Add(data);
            }

            return configPlayers;
        }
        
        private static List<object> GetPlayer360LstInfo()
        {
            INAddressablePlayer360[] players360 = GameObject.FindObjectsOfType<INAddressablePlayer360>(true);

            if (players360 == null || players360.Length <= 0)
                return null;

            List<object> configPlayers = new List<object>();

            for (int i = 0; i < players360.Length; i++)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                data.Add(INAddressablePlayer360.URL_VIDEO_KEY, players360[i].urlVideo);
                data.Add(INAddressablePlayer360.POSITION_KEY, players360[i].name);

                configPlayers.Add(data);
            }

            return configPlayers;
        }

        private static List<object> GetPlayer2DLstInfo()
        {
            INAddressablesPlayer2D[] players2D = GameObject.FindObjectsOfType<INAddressablesPlayer2D>(true);

            if (players2D == null || players2D.Length <= 0)
                return null;

            List<object> configPlayers = new List<object>();

            for (int i = 0; i < players2D.Length; i++)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                data.Add(INAddressablesPlayer2D.URL_VIDEO_KEY, players2D[i].urlVideo);
                data.Add(INAddressablesPlayer2D.POSITION_KEY, players2D[i].name);
                data.Add(INAddressablesPlayer2D.DISTANCE_TRIGGER.ToString(), players2D[i].isTrigger);

                Dictionary<string, object> dicoTitleEN = new Dictionary<string, object>();
                Dictionary<string, object> dicoTitleFR = new Dictionary<string, object>();
                Dictionary<string, object> dicoTitleES = new Dictionary<string, object>();
                dicoTitleEN.Add(Title.EN_KEY, players2D[i].titles.EN);
                dicoTitleFR.Add(Title.FR_KEY, players2D[i].titles.FR);
                dicoTitleES.Add(Title.ES_KEY, players2D[i].titles.ES);

                List<Dictionary<string, object>> lstTitles = new List<Dictionary<string, object>>();
                lstTitles.Add(dicoTitleEN);
                lstTitles.Add(dicoTitleFR);
                lstTitles.Add(dicoTitleES);

                data.Add(INAddressablesDoor.TITLE_KEY, lstTitles);

                configPlayers.Add(data);
            }

            return configPlayers;
        }

        private static Dictionary<string, object> GetPlayer2DInfo()
        {
            INAddressablesPlayer2D[] players2D = GameObject.FindObjectsOfType<INAddressablesPlayer2D>(true);

            if (players2D == null || players2D.Length <= 0)
                return null;

            List<object> configPlayers = new List<object>();

            for (int i = 0; i < players2D.Length; i++)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                data.Add(INAddressablesPlayer2D.URL_VIDEO_KEY, players2D[i].urlVideo);
                data.Add(INAddressablesPlayer2D.POSITION_KEY, players2D[i].name);

                Dictionary<string, object> dicoTitleEN = new Dictionary<string, object>();
                Dictionary<string, object> dicoTitleFR = new Dictionary<string, object>();
                Dictionary<string, object> dicoTitleES = new Dictionary<string, object>();
                dicoTitleEN.Add(Title.EN_KEY, players2D[i].titles.EN);
                dicoTitleFR.Add(Title.FR_KEY, players2D[i].titles.FR);
                dicoTitleES.Add(Title.ES_KEY, players2D[i].titles.ES);

                List<Dictionary<string, object>> lstTitles = new List<Dictionary<string, object>>();
                lstTitles.Add(dicoTitleEN);
                lstTitles.Add(dicoTitleFR);
                lstTitles.Add(dicoTitleES);

                data.Add(INAddressablesDoor.TITLE_KEY, lstTitles);

                configPlayers.Add(data);
            }

            Dictionary<string, object> dicoDoorList = new Dictionary<string, object>();
            dicoDoorList.Add(CONFIG_PLAYER_2D_KEY, configPlayers);

            return dicoDoorList;
        }

        private static string GetSpawnName()
        {
            INAddressablesSpawn[] spawn = GameObject.FindObjectsOfType<INAddressablesSpawn>(true);

            if (spawn != null && spawn.Length > 0)
                return spawn[0].gameObject.name;
            else
                return INAddressablesDoor.DEFAULT_POSITION_PLAYER_KEY;
        }


        private static Dictionary<string, object> GenerateAppMode(string sceneName)
        {
            List<object> configDoors = new List<object>();
            Dictionary<string, object> data = new Dictionary<string, object>();

            // Create one ConfigDoor
            if (INAddressables_Params.isDevEnvironment())
                data.Add(INAddressablesDoor.URL_IMAGE_KEY, ADDRESSABLE_BASE_DEV + BASE_DOOR_URL_IMG);
            else
                data.Add(INAddressablesDoor.URL_IMAGE_KEY, ADDRESSABLE_BASE_PROD + BASE_DOOR_URL_IMG);

            data.Add(INAddressablesDoor.CUSTOM_PREFAB_PATH, "");
            data.Add(INAddressablesDoor.IS_ADDRESSABLE_KEY, true);
            data.Add(INAddressablesDoor.ACTION_KEY, sceneName);
            data.Add(INAddressablesDoor.POSITION_DOOR_KEY, BASE_POSITION_DOOR_VALUE);
            data.Add(BASE_POSITION_ROOM_KEY, BASE_POSITION_VALUE);

            Dictionary<string, object> dictObj = new Dictionary<string, object>();
            dictObj.Add(Title.EN_KEY, BASE_TITLE_VALUE);

            Dictionary<string, object> dicTitleEN = new Dictionary<string, object>();
            Dictionary<string, object> dicTitleFR = new Dictionary<string, object>();
            Dictionary<string, object> dicTitleES = new Dictionary<string, object>();
            dicTitleEN.Add(Title.EN_KEY, BASE_TITLE_VALUE);
            dicTitleFR.Add(Title.FR_KEY, BASE_TITLE_VALUE);
            dicTitleES.Add(Title.ES_KEY, BASE_TITLE_VALUE);

            List<Dictionary<string, object>> lstTitles = new List<Dictionary<string, object>>();
            lstTitles.Add(dicTitleEN);
            lstTitles.Add(dicTitleFR);
            lstTitles.Add(dicTitleES);

            data.Add(INAddressablesDoor.TITLE_KEY, lstTitles);

            string spawnPosition = GetSpawnName();

            data.Add(INAddressablesDoor.POSITION_PLAYER_KEY, spawnPosition);
            data.Add(BASE_VERSION_ROOM_KEY, BASE_VERSION_VALUE);

            Dictionary<string, object> dicVersionWarningEN = new Dictionary<string, object>();
            Dictionary<string, object> dicVerionsWarningFR = new Dictionary<string, object>();
            Dictionary<string, object> dicVersionWanringES = new Dictionary<string, object>();
            dicVersionWarningEN.Add(Title.EN_KEY, BASE_VERSION_WARNING_VALUE);
            dicVerionsWarningFR.Add(Title.FR_KEY, BASE_VERSION_WARNING_VALUE);
            dicVersionWanringES.Add(Title.ES_KEY, BASE_VERSION_WARNING_VALUE);

            List<Dictionary<string, object>> lstWarnings = new List<Dictionary<string, object>>();
            lstWarnings.Add(dicVersionWarningEN);
            lstWarnings.Add(dicVerionsWarningFR);
            lstWarnings.Add(dicVersionWanringES);

            data.Add(BASE_VERSION_WARNING_ROOM_KEY, lstWarnings);

            // Add to ConfigDoors List

            configDoors.Add(data);

            Dictionary<string, object> dicoDoorList = new Dictionary<string, object>();
            dicoDoorList.Add(CONFIG_DOOR_KEY, configDoors);

            // Create to ConfigLivingRooms

            Dictionary<string, object> livingRoomObj = new Dictionary<string, object>();
            livingRoomObj.Add(CONFIG_LIVINGROOM_KEY, dicoDoorList);

            return livingRoomObj;
        }

        public static IEnumerator CreateFileCoroutine(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
            yield return new WaitForSeconds(1f);
        }

        private static Dictionary<string, object> MergeDictionaries(Dictionary<string, object> dict1, Dictionary<string, object> dict2)
        {
            Dictionary<string, object> mergedDict = new Dictionary<string, object>();

            foreach (var kvp in dict1)
            {
                mergedDict[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in dict2)
            {
                mergedDict[kvp.Key] = kvp.Value;
            }

            return mergedDict;
        }
    }
}
