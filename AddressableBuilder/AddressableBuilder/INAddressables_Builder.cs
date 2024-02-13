using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using XRTools.IN_MiniJSON;
using XRTools;
using System.Diagnostics;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using XRWebService;

namespace AddressableBuilder
{
    public static class INAddressables_Builder
    {
        private const string IN_ADDRESSABLES_BASE_KEY = "addressableList";
        public const string IN_ADDRESSABLES_NAME = "INAddressables";
        public const string FORMAT = ".json";

        public static IEnumerator ExportINAddressable(string catalogUrl, string sceneName, string path) //  INAddressable Export
        {
            Dictionary<string, object> dicoINAddressable = BuildINAddressablesInfo(catalogUrl, sceneName);

            if (dicoINAddressable == null || dicoINAddressable.Count == 0)
                yield return null;

            string serializedJson = Json.Serialize(dicoINAddressable);
            string pathINAddressable = path + Path.DirectorySeparatorChar + IN_ADDRESSABLES_NAME + "_" + INAddressables_Params.PLATFORM_NAME + FORMAT;

            yield return INAddressableEditorCoroutine.StartCoroutine(IN_Utils.CreateFileCoroutine(pathINAddressable, serializedJson));

            yield return null;
        }

        private static Dictionary<string, object> BuildINAddressablesInfo(string catalogUrl, string sceneName)
        {
            INAddressableType jsonINAddressalbe = new INAddressableType();

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.Log("Addressable group not found");
                return null;
            }

            jsonINAddressalbe.Name          =   sceneName;
            jsonINAddressalbe.UrlCatalog    = catalogUrl;
            jsonINAddressalbe.Type          = "Scene";

            List<object> addressableList = new List<object>();

            Dictionary<string, object> data = new Dictionary<string, object>();

            data.Add(INAddressableType.NAME_KEY         , jsonINAddressalbe.Name);
            data.Add(INAddressableType.URL_CATALOG_KEY  , jsonINAddressalbe.UrlCatalog);
            data.Add(INAddressableType.TYPE_KEY         , jsonINAddressalbe.Type);

            addressableList.Add(data);

            Dictionary<string, object> dicoAddressableList = new Dictionary<string, object>();
            dicoAddressableList.Add(IN_ADDRESSABLES_BASE_KEY, addressableList);
            return dicoAddressableList;
        }

        public static void UploadBuilder(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                INAddressableEditorCoroutine.StartCoroutineOwnerless(OpenCatalogAndUpload(path));
            }
        }

        public static IEnumerator OpenCatalogAndUpload(string basePath)
        {
            string[] files = Directory.GetFiles(basePath);

            string catalogPath = string.Empty;
            foreach (string filePath in files)
            {
                if (filePath.Contains("catalog") && Path.GetExtension(filePath).Equals(".json"))
                {
                    catalogPath = filePath;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(catalogPath))
            {
                string filesString = "";
                string keyRemotePref = EditorPrefs.GetString(INAddressables_Params.PROJECT_ID_KEY);

                if (string.IsNullOrEmpty(keyRemotePref))
                    yield break;

                XRAddressablePartner currentPartner = INAddressables_Params.AddressableList.GetPartner(keyRemotePref);
                if (currentPartner == null)
                    yield break;

                foreach (string filePath in files)
                {
                    filesString += filePath + ";";
                }
                filesString = filesString.Substring(0, filesString.Length - 1);// remove the last useless ;
                
                INAddressables_Upload.UploadFiles(currentPartner , filesString);
                //Process.Start(Application.dataPath + @"/../ImmersiveNow/bin/Upload.exe", jsonString + " " + filesString + " " + currentPartner.m_DevBaseUri);
                //Process.Start(Application.dataPath + @"/ImmersiveNow/bin/Upload.exe", jsonString + " " + filesString + " " + "Dev/Addr/OSP/Partners/BIANCA/");
            }
            else
                Debug.Log("Built addressables not found, please create your addressables before download option");

            yield return null;
        }

        public static void ReplacePreviousBuild(string outputPath)
        {
            if (!Directory.Exists(outputPath))
                return;

            Regex reg = new Regex(@"(\.hash|\.json|\.bundle)$");

            string[] pathFiles = Directory.GetFiles(outputPath);
            List<string> namesFilesDir = new List<string>();

            if (pathFiles == null || pathFiles.Length <= 0)
                return;

            string backUpName = "Backup_" + INAddressables_Params.PLATFORM_NAME + "_" + DateTime.Now.ToString("yyyy'.'MM'.'dd'.'HH'.'mm'.'");

            string[] splitedName = outputPath.Split(Path.DirectorySeparatorChar);

            string newName = string.Empty;
            for (int i = 0; i < splitedName.Length - 2; i++)
            {
                if (i == 0)
                    newName += splitedName[i];
                else
                    newName += Path.DirectorySeparatorChar + splitedName[i];
            }

            string backUpBuildPath = Path.Combine(newName, "Backup" + Path.DirectorySeparatorChar + "Addressable" + Path.DirectorySeparatorChar + backUpName);

            if (!Directory.Exists(backUpBuildPath))
                Directory.CreateDirectory(backUpBuildPath);
            else
            {
                string[] files = Directory.GetFiles(backUpBuildPath);

                for (int i = 0; i < files.Length; i++)
                    File.Delete(files[i]);
            }

            foreach (string filePath in pathFiles)
            {
                if (reg.Match(filePath).Success)
                {
                    namesFilesDir.Add(new FileInfo(filePath).Name);

                    if (File.Exists(filePath))
                    {
                        string backUpFileName = backUpBuildPath + Path.DirectorySeparatorChar + Path.GetFileName(filePath);
                        File.Move(filePath, backUpFileName);
                    }
                }
            }

            BackupAddressableContent(backUpBuildPath);
        }

        private static void BackupAddressableContent(string backUpPath)
        {
            string basePath = Application.dataPath;

            string androidFile  = basePath + "/AddressableAssetsData/Android/addressables_content_state.bin";
            string iOSFile      = basePath + "/AddressableAssetsData/iOS/addressables_content_state.bin";

            if (File.Exists(androidFile))
                FileUtil.ReplaceFile(androidFile, backUpPath + "/Android_addressables_content_state.bin");

            if (File.Exists(iOSFile))
                FileUtil.ReplaceFile(iOSFile, backUpPath + "/iOS_addressables_content_state.bin");
        }

    }
}
