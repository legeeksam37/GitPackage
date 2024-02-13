using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using XRWebService;

namespace AddressableBuilder
{
    public static class INAddressables_Upload
    {
		private static string TAG  = "[INAddressables_Upload] ";

		public static void UploadFiles(XRAddressablePartner currentPartner , string filesString)
		{
            if (INAddressables_Params.isProdEnvironment() || INAddressables_Params.isPreprodEnvironment())
			{
				UnityEngine.Debug.Log(TAG+"Can't upload in production environment");
				return;
			}

            string storage = currentPartner.m_StorageSA;
            string jsonString = storage;
            jsonString = UnityWebRequest.EscapeURL(jsonString);
		
			Process.Start(Application.dataPath + @"/../ImmersiveNow/bin/Upload.exe", jsonString + " " + filesString + " " + currentPartner.m_DevBaseUri);
		}
    }
}
