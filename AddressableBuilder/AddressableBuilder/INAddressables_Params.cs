using UnityEditor;
using XRWebService;

namespace AddressableBuilder
{
    public static class INAddressables_Params
    {
        public static string ProjectID           = "Enter project ID";
        public static string PLATFORM_NAME       = string.Empty;
        public const string  PROJECT_ID_KEY      = "INProjectID";
        public const string  PLATFORM_PREF_KEY   = "INQuestBuild";

        public const string  ENVIRONMENT_KEY            = "Environment";
        public const string  ENVIRONMENT_VALUE_DEV      = "Dev";
        public const string  ENVIRONMENT_VALUE_PREPROD  = "Preprod";
        public const string  ENVIRONMENT_VALUE_PROD     = "Prod";
        public static string Environment                = string.Empty;

        public static string CurrentProjectID;

        public static XRAddressablePartnerList AddressableList;

        public static void SwitchToAndroid()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
        }

        public static void SwitchToIOS()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            }
        }

        public static bool isDevEnvironment()
		{
            if (string.IsNullOrEmpty(Environment))
                return true;

            if (Environment!=null && Environment.Equals(ENVIRONMENT_VALUE_DEV))
                return true;

            return false;
		}

        public static bool isPreprodEnvironment()
		{
            if (Environment==null)
                return false;

            if (Environment!=null && Environment.Equals(ENVIRONMENT_VALUE_PREPROD))
                return true;

            return false;
		}
        

        public static bool isProdEnvironment()
		{
            if (Environment==null)
                return false;

            if (Environment!=null && Environment.Equals(ENVIRONMENT_VALUE_PROD))
                return true;

            return false;
		}

    }
}
