#if UNITY_EDITOR || UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
using System.Collections.Generic;
//#if UNITY_IOS || UNITY_CLOUD_BUILD
//    using UnityEditor.iOS.Xcode;
//#endif
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor.iOS.Xcode;
#endif

public class iOSPostBuildProcessor : MonoBehaviour
{
    // public static iOSPostBuildProcessor Instance { get; private set; }
    /**
     * Runs when Post-Export method has been set to
     * 'PostBuildProcessor.OnPostprocessBuildiOS' in your Unity Cloud Build
     * target settings.
     */
#if UNITY_CLOUD_BUILD
        // This method is added in the Advanced Features Settings on UCB
        // PostBuildProcessor.OnPostprocessBuildiOS
        public static void OnPostprocessBuildiOS (string exportPath)
        {
            Debug.Log("[UCB Demos] OnPostprocessBuildiOS");
            ProcessPostBuild(BuildTarget.iOS,exportPath);
        }
#endif

    /**
     * Runs after successful build of an iOS-targetted Unity project
     * via the editor Build dialog.
     */
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
#if !UNITY_CLOUD_BUILD
        Debug.Log("[UNITY_CLOUD_BUILD] OnPostprocessBuild");
        ProcessPostBuild(buildTarget, path);
#endif
    }
    public static void TestProcessPostBuild(string path)
    {
        ProcessPostBuild(new BuildTarget(), path);
    }

    /**
     * This ProcessPostBuild method will run via Unity Cloud Build, as well as
     * locally when build target is iOS. Using the Xcode Manipulation API, it is
     * possible to modify build settings values and also perform other actions
     * such as adding custom frameworks. Link below is the reference documentation
     * for the Xcode Manipulation API:
     *
     * http://docs.unity3d.com/ScriptReference/iOS.Xcode.PBXProject.html
     */
    private static void ProcessPostBuild(BuildTarget buildTarget, string path)
    {
        Debug.Log("build path is " + path);
        // Only perform these steps for iOS builds
#if UNITY_IOS || UNITY_CLOUD_BUILD
     
            Debug.Log ("[UNITY_IOS] ProcessPostBuild - Adding Google Analytics frameworks.");
     
            // Go get pbxproj file
            string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
     
            // PBXProject class represents a project build settings file,
            // here is how to read that in.
            PBXProject proj = new PBXProject ();
            proj.ReadFromFile (projPath);
     
            // This is the Xcode target in the generated project
            // string target = proj.TargetGuidByName("Unity-iPhone");
            
            string target = proj.GetUnityMainTargetGuid();
     
            // List of frameworks that will be added to project
            List<string> frameworks = new List<string>() {
                "AdSupport.framework",
                "CoreData.framework",
                "SystemConfiguration.framework",
                "libz.dylib",
                "libsqlite3.dylib"
            };
     
            // Add each by name
            frameworks.ForEach((framework) => {
                proj.AddFrameworkToProject(target, framework, false);
                proj.AddFrameworksBuildPhase(target);
            });

        // If building with the non-bitcode version of the plugin, these lines should be uncommented.
        Debug.Log("[UNITY_IOS] ProcessPostBuild - Setting build property: ENABLE_BITCODE = NO");
        proj.AddBuildProperty(target, "ENABLE_BITCODE", "NO");
        proj.SetBuildProperty(target, "CODE_SIGN_IDENTITY[sdk=ipohneos*]", "iPhone Developer");


        // Write PBXProject object back to the file
        proj.WriteToFile (projPath);

        #region Jones: Info.plist editing after build.
        string infoPlistPath = path + "/Info.plist";
        PlistDocument plistDoc = new PlistDocument();
        plistDoc.ReadFromFile(infoPlistPath);

        if (plistDoc.root != null)
        {
            // plistDoc.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            // plistDoc.root.SetString("CFBundleDisplayName", "MY APP NAME");

            plistDoc.root.SetString("NSCameraUsageDescription", "$(PRODUCT_NAME) uses camera");
            plistDoc.root.SetString("NSLocationWhenInUseUsageDescription", "$(PRODUCT_NAME) uses location");
            plistDoc.root.SetString("NSPhotoLibraryUsageDescription", "$(PRODUCT_NAME) uses photos");
            plistDoc.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            
            // plistDoc.root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion.Replace(".", string.Empty));
            // plistDoc.root.SetString("CFBundleVersion", PlayerSettings.bundleVersion);


            plistDoc.root.SetString("CFBundleShortVersionString", Application.version.Replace(".", string.Empty));
            plistDoc.root.SetString("CFBundleVersion", Application.version);

            // plistDoc.root.SetString("CFBundleShortVersionString", "$(MARKETING_VERSION)");
            // plistDoc.root.SetString("CFBundleVersion", "$(CURRENT_PROJECT_VERSION)");

            // plistDoc.root.SetString("CFBundleShortVersionString", PlayerSettings.iOS.buildNumber.Replace(".", string.Empty));
            // plistDoc.root.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber);

        #region Jones: Facebook Related plist
            plistDoc.root.CreateArray("LSApplicationQueriesSchemes");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20130214");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20130410");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20130702");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20131010");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20131219");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20140410");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20140116");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20150313");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20150629");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbapi20160328");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbauth");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fb - messenger - share - api");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbauth2");
            plistDoc.root ["LSApplicationQueriesSchemes"].AsArray().AddString("fbshareextension");
            // plistDoc.root.SetString("LSApplicationQueriesSchemes", "");



        #endregion
        #region miscellaneous
            
            plistDoc.root.SetBoolean("UIPrerenderedIcon", true);
            
        #endregion

        #region 
        #endregion

            plistDoc.WriteToFile(infoPlistPath);
        }
        else
        {
            Debug.LogError("ERROR: Can't open " + infoPlistPath);
        }
        #endregion

        #region Jones: add flags to Compile Source files
        DisableArcOnFileByProjectPath(proj, "Libraries/Plugins/iOS/WebViewPlugin.mm");
        DisableArcOnFileByProjectPath(proj, "Libraries/Plugins/iOS/YSImageCrop.mm");
        #endregion

        CloseProject(projPath,proj);

        #region Jones: Edit text file of .pbxproj
        
#if !UNITY_CLOUD_BUILD
        
        TestversionConvert();
        
#endif

        #endregion


        
     
#endif


        // string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        // UnityEditor.iOS.Xcode.PBXProject proj = new UnityEditor.iOS.Xcode.PBXProject();
        //proj.ReadFromFile(projPath);

        //string infoPlistPath = path + "/Info.plist";
        //PlistDocument plistDoc = new PlistDocument();
        //plistDoc.ReadFromFile(infoPlistPath);

        //if (plistDoc.root != null)
        //{
        //    // plistDoc.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
        //    // plistDoc.root.SetString("CFBundleDisplayName", "MY APP NAME");

        //    plistDoc.root.SetString("NSCameraUsageDescription", "$(PRODUCT_NAME) uses camera");
        //    plistDoc.root.SetString("NSLocationWhenInUseUsageDescription", "$(PRODUCT_NAME) uses location");
        //    plistDoc.root.SetString("NSPhotoLibraryUsageDescription", "$(PRODUCT_NAME) uses photos");
        //    plistDoc.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        //    plistDoc.root.SetString("CFBundleShortVersionString", PlayerSettings.bundleVersion.Replace(".", string.Empty));
        //    plistDoc.root.SetString("CFBundleVersion", PlayerSettings.bundleVersion);

        //    plistDoc.root.SetString("CFBundleShortVersionString", PlayerSettings.iOS.buildNumber.Replace(".", string.Empty));
        //    plistDoc.root.SetString("CFBundleVersion", PlayerSettings.iOS.buildNumber);

        //    plistDoc.WriteToFile(infoPlistPath);
        //}
        //else
        //{
        //    Debug.LogError("ERROR: Can't open " + infoPlistPath);
        //}


        // proj.SetBuildProperty(target, "CODE_SIGN_IDENTITY[sdk=ipohneos*]", "iPhone Developer");
        // proj.SetBuildProperty(target, "")

        // proj.GetUnityFrameworkTargetGuid


        // string targetUnityFrameWorkGuid = proj.GetUnityFrameworkTargetGuid();
        // DisableArcOnFileByProjectPath(proj, "Libraries/Plugins/iOS/WebViewPlugin.mm");
        // DisableArcOnFileByProjectPath(proj, "Libraries/Plugins/iOS/YSImageCrop.mm");


    }
    private static void DisableArcOnFile(PBXProject project, string guid)
    {
        string target = project.GetUnityFrameworkTargetGuid();
        // project.RemoveFileFromBuild(target, guid);
        // project.AddFileToBuildWithFlags(target, guid, "-fno-objc-arc");

        List<string> testStrings = new List<string>();
        testStrings = project.GetCompileFlagsForFile(target, guid);
        
        testStrings.Add("-fno-objc-arc");

        project.SetCompileFlagsForFile(target, guid, testStrings);


        // string mainTarget = project.GetUnityMainTargetGuid();
        // project.RemoveFileFromBuild(mainTarget, guid);
        // project.AddFileToBuildWithFlags(mainTarget, guid, "-fno-objc-arc");


    }
    private static void DisableArcOnFileByProjectPath(PBXProject project, string filePath)
    {
        
        var guid = project.FindFileGuidByProjectPath(filePath);
        Debug.Log(guid);
        DisableArcOnFile(project, guid);
    }
    private static void DisableArcOnFileByRealPath(PBXProject project, string filePath)
    {
        var guid = project.FindFileGuidByRealPath(filePath);

        if (guid == null)
        {
            guid = project.AddFile(filePath, filePath);
        }

        DisableArcOnFile(project, guid);
    }
    private static void CloseProject(string _projectPath, PBXProject _project)
    {
        File.WriteAllText(_projectPath, _project.WriteToString());
    }

    [TestMethod]
    public static void TestversionConvert()
    {
        var path = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "Build/" + "Len89";
        string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        Debug.Log(projPath);
        // PBXProject class represents a project build settings file,
        // here is how to read that in.
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        // This is the Xcode target in the generated project
        // string target = proj.TargetGuidByName("Unity-iPhone");

        string target = proj.GetUnityMainTargetGuid();

        var version = proj.GetBuildPropertyForAnyConfig(target, "compatibilityVersion");
        var version2 = proj.GetBuildPropertyForConfig(target, "compatibilityVersion");
        Debug.Log("target guid is " + target
            + " version is " + version
            + " version2 is " + version2);


        //string line = string.Empty;
        //using (var fs = File.Open(projPath, FileMode.Open, FileAccess.ReadWrite))
        //{
        //    var destinationReader = new StreamReader(fs);
        //    var writer = new StreamWriter(fs);


        //    while( (line = destinationReader.ReadLine()) != null )
        //    {

        //        if (line.Contains("compatibilityVersion ="))
        //        {
        //            writer.WriteLine("compatibilityVersion = Xcode 12.0");
        //            continue;
        //        }
        //        writer.WriteLine(line);
        //    }

        //}

        // "compatibilityVersion = \"Xcode 12.0\""

        lineChanger("\t\t\tcompatibilityVersion = \"Xcode 12.0\";",
            projPath,
            LineSeek(projPath, "compatibilityVersion"));

        // compatibilityVersion = "Xcode 3.2";

    }
    private static void lineChanger(string newText, string filePath, int line_to_edit)
    {
        string[] arrLine = File.ReadAllLines(filePath);
        // arrLine[line_to_edit - 1] = newText;
        arrLine[line_to_edit] = newText;
        File.WriteAllLines(filePath, arrLine);
        
    }

    private static int LineSeek(string fileName, string textToFind)
    {
        string[] arrLine = File.ReadAllLines(fileName);
        // Array.FindIndex(arrLine, x => x.   )
        arrLine.Contains("compatibilityVersion = Xcode");
        var tempInt = arrLine.ToList().FindIndex(x => x.Contains("compatibilityVersion"));

        
        return tempInt;
    }
}
#endif
