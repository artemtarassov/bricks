using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Xml;



#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class Build
{
#if UNITY_IOS
    private const string IOSResolverTypeName = "Google.IOSResolver, Google.IOSResolver";
    private const string IOSResolverPodfileGenerationSetting = "Google.IOSResolver.PodfileGenerationEnabled";
#endif

    private static string[] Scenes = new string[]
    {
        //"Assets/Scenes/MainMatTest.unity"
        "Assets/Scenes/Main2.unity"
    };

    private static string GetFileName(string extension)
    {
        var version = PlayerSettings.bundleVersion.Replace(".", "_");
        return "solari_" + version + "-" + PlayerSettings.Android.bundleVersionCode + extension;
    }

#if UNITY_IOS
    private static void DisableIosResolverPodfileGeneration()
    {
        try
        {
            var resolverType = Type.GetType(IOSResolverTypeName);
            if (resolverType == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    resolverType = assembly.GetType("Google.IOSResolver");
                    if (resolverType != null) break;
                }
            }

            var property = resolverType?.GetProperty(
                "PodfileGenerationEnabled",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (property != null && property.CanWrite && property.PropertyType == typeof(bool))
            {
                property.SetValue(null, false, null);
                Debug.Log("Disabled EDM4U Podfile generation via Google.IOSResolver.");
            }
            else
            {
                Debug.LogWarning("Could not access Google.IOSResolver.PodfileGenerationEnabled via reflection.");
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Failed to disable EDM4U Podfile generation via reflection: " + exception.Message);
        }

        PersistProjectSetting(IOSResolverPodfileGenerationSetting, "False");
    }

    private static void PersistProjectSetting(string settingName, string value)
    {
        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
        {
            Debug.LogWarning("Could not determine the Unity project root to persist EDM4U settings.");
            return;
        }

        var settingsPath = Path.Combine(projectRoot, "ProjectSettings", "GvhProjectSettings.xml");
        var document = new XmlDocument();

        if (File.Exists(settingsPath))
        {
            document.Load(settingsPath);
        }
        else
        {
            var declaration = document.CreateXmlDeclaration("1.0", "utf-8", null);
            document.AppendChild(declaration);
            document.AppendChild(document.CreateElement("projectSettings"));
        }

        var root = document.DocumentElement;
        if (root == null)
        {
            root = document.CreateElement("projectSettings");
            document.AppendChild(root);
        }

        var existingSetting = root.SelectSingleNode(
            string.Format("projectSetting[@name=\"{0}\"]", settingName)) as XmlElement;

        if (existingSetting == null)
        {
            existingSetting = document.CreateElement("projectSetting");
            existingSetting.SetAttribute("name", settingName);
            root.AppendChild(existingSetting);
        }

        existingSetting.SetAttribute("value", value);
        document.Save(settingsPath);

        Debug.Log("Persisted EDM4U setting " + settingName + "=" + value + " to " + settingsPath);
    }

    private static void UpdateOtherFile()
    {
        var buildPath = "build/ios";

        Debug.Log("Updating iOS Xcode project at: " + buildPath);

        // --- Load Xcode project ---
        var projectPath = PBXProject.GetPBXProjectPath(buildPath);
        var proj = new PBXProject();
        proj.ReadFromFile(projectPath);

        var mainTarget = proj.GetUnityMainTargetGuid();
        var frameworkTarget = proj.GetUnityFrameworkTargetGuid();

        Debug.Log("Main target GUID: " + mainTarget);
        Debug.Log("Framework target GUID: " + frameworkTarget);

        // Do NOT clobber these. Only add inherited if missing.
        proj.AddBuildProperty(mainTarget, "OTHER_LDFLAGS", "$(inherited)");
        proj.AddBuildProperty(mainTarget, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
        proj.AddBuildProperty(frameworkTarget, "OTHER_LDFLAGS", "$(inherited)");
        proj.AddBuildProperty(frameworkTarget, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");

        // --- Entitlements file setup ---
        var entitlementsFileName = "grumpy.entitlements";
        var entitlementsFullPath = Path.Combine(buildPath, entitlementsFileName);

        // Bundle id for iOS
        var bundleId = "de.badmonkee.solari";

        // iCloud container id: iCloud.<bundleId> is the common convention
        // Make sure this container exists/allowed for your App ID in the Apple Developer portal.
        var iCloudContainerId = "iCloud." + bundleId;

        // Create / update entitlements plist
        var ent = new PlistDocument();
        if (File.Exists(entitlementsFullPath))
            ent.ReadFromFile(entitlementsFullPath);
        else
            ent.Create();

        var root = ent.root;

        // 1) KVS entitlement (you already have this, but we ensure it's correct)
        // Use $(TeamIdentifierPrefix) to avoid hardcoding Team ID
        root.SetString("com.apple.developer.ubiquity-kvstore-identifier",
            "$(TeamIdentifierPrefix)" + bundleId);

        // 2) If ANY code path calls ubiquity containers (your earlier error suggests it does),
        // you MUST have at least one container identifier.
        var containers = root.values.ContainsKey("com.apple.developer.icloud-container-identifiers")
            ? root["com.apple.developer.icloud-container-identifiers"].AsArray()
            : root.CreateArray("com.apple.developer.icloud-container-identifiers");

        // Ensure our container is present
        bool hasContainer = false;
        for (int i = 0; i < containers.values.Count; i++)
        {
            if (containers.values[i].AsString() == iCloudContainerId)
            {
                hasContainer = true;
                break;
            }
        }
        if (!hasContainer)
            containers.AddString(iCloudContainerId);

        // 3) iCloud services. If you only want KVS, this can be minimal,
        // but adding CloudDocuments prevents UbiquityContainerUnavailable if the plugin touches it.
        var services = root.values.ContainsKey("com.apple.developer.icloud-services")
            ? root["com.apple.developer.icloud-services"].AsArray()
            : root.CreateArray("com.apple.developer.icloud-services");

        bool hasCloudDocuments = false;
        for (int i = 0; i < services.values.Count; i++)
        {
            if (services.values[i].AsString() == "CloudDocuments")
            {
                hasCloudDocuments = true;
                break;
            }
        }
        if (!hasCloudDocuments)
            services.AddString("CloudDocuments");

        ent.WriteToFile(entitlementsFullPath);
        Debug.Log("Wrote entitlements: " + entitlementsFullPath);

        // Tell Xcode to use this entitlements file for codesign
        proj.SetBuildProperty(mainTarget, "CODE_SIGN_ENTITLEMENTS", entitlementsFileName);
        proj.SetBuildProperty(frameworkTarget, "CODE_SIGN_ENTITLEMENTS", entitlementsFileName);

        // --- Add capabilities (this updates pbxproj and may add frameworks) ---
        // Use the same entitlements file name (relative path in the Xcode project)
        proj.AddCapability(mainTarget, PBXCapabilityType.iCloud, entitlementsFileName, true);
        proj.AddCapability(mainTarget, PBXCapabilityType.GameCenter, entitlementsFileName, true);

        // Often harmless, sometimes necessary with Unity + plugins
        proj.AddCapability(frameworkTarget, PBXCapabilityType.iCloud, entitlementsFileName, false);
        proj.AddCapability(frameworkTarget, PBXCapabilityType.GameCenter, entitlementsFileName, false);

        proj.WriteToFile(projectPath);
        Debug.Log("Saved PBXProject modifications: " + projectPath);

        // --- Info.plist edits ---
        var plistPath = Path.Combine(buildPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        var plistRoot = plist.root;

        plistRoot.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        // Avoid recreating the array every build (which duplicates keys)
        PlistElementArray schemes;
        if (plistRoot.values.ContainsKey("LSApplicationQueriesSchemes"))
            schemes = plistRoot["LSApplicationQueriesSchemes"].AsArray();
        else
            schemes = plistRoot.CreateArray("LSApplicationQueriesSchemes");

        void AddScheme(string s)
        {
            foreach (var v in schemes.values)
                if (v.AsString() == s) return;
            schemes.AddString(s);
        }

        AddScheme("fbapi");
        AddScheme("fb-messenger-api");
        AddScheme("fbauth2");
        AddScheme("fbshareextension");

        plist.WriteToFile(plistPath);
        Debug.Log("Updated Info.plist: " + plistPath);

    }
    #endif
    public static void All()
    {
        //AndroidApk();
        AndroidAab();
        IOS();
    }

    public static void AndroidAll()
    {
        // AndroidApk();
        AndroidAab();
    }

    public static void AndroidApk()
    {
        PlayerSettings.Android.keystoreName = "user.keystore"; //build/user.keystore
        PlayerSettings.Android.keystorePass = "123dabei";
        PlayerSettings.Android.keyaliasName = "solari";
        PlayerSettings.Android.keyaliasPass = "123dabei";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = Scenes;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.CleanBuildCache | BuildOptions.StrictMode;

        { //build apk
            buildPlayerOptions.locationPathName = "build/android/" + GetFileName(".apk");
            EditorUserBuildSettings.buildAppBundle = false;

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("APK Build succeeded: " + summary.totalSize + " bytes");
            }

            if (summary.result == BuildResult.Failed)
            {
                Debug.Log("APK Build failed");
            }
        }
    }

    public static void AndroidAab()
    {
        PlayerSettings.Android.keystoreName = "user.keystore"; //build/user.keystore
        PlayerSettings.Android.keystorePass = "123dabei";
        PlayerSettings.Android.keyaliasName = "solari";
        PlayerSettings.Android.keyaliasPass = "123dabei";

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = Scenes;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.CleanBuildCache | BuildOptions.StrictMode;

        {
            //now the aab
            EditorUserBuildSettings.buildAppBundle = true;
            buildPlayerOptions.locationPathName = "build/android/" + GetFileName(".aab");
            ;
            BuildReport report1 = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary1 = report1.summary;

            if (summary1.result == BuildResult.Succeeded)
            {
                Debug.Log("AppBundle Build succeeded: " + summary1.totalSize + " bytes");
            }

            if (summary1.result == BuildResult.Failed)
            {
                Debug.Log("AppBundle Build failed");
            }
        }
    }

    public static void IOS()
    {
#if UNITY_IOS
        DisableIosResolverPodfileGeneration();
#endif

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = Scenes;
        buildPlayerOptions.locationPathName = "build/ios";
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.CleanBuildCache | BuildOptions.StrictMode;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;


        if (summary.result == BuildResult.Succeeded)
        {
            #if UNITY_IOS
            UpdateOtherFile();
            #endif
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
            //DeleteEmptyStringFiles("./build/ios/I2Localization/");

        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
    }

    private static void DeleteEmptyStringFiles(string folder)
    {
        //delete in all folder and subfolders
        var files = System.IO.Directory.GetFiles(folder, "*.*", System.IO.SearchOption.AllDirectories);
        Debug.Log("Checking for empty files in folder: " + folder + ". total " + files.Length + " files");
        foreach (var file in files)
        {
            Debug.Log("Checking file: " + file);
            var info = new System.IO.FileInfo(file);
            if (info.Length == 0)
            {
                System.IO.File.Delete(file);
                Debug.Log("Deleted empty file: " + file);
            }
        }
    }
}
