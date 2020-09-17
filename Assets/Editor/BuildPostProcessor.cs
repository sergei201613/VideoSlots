using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class BuildPostProcessor
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target == BuildTarget.iOS)
        {
            // Read.
            //string projectPath = PBXProject.GetPBXProjectPath(path);
            //PBXProject project = new PBXProject();
            //project.ReadFromString(File.ReadAllText(projectPath));
            //string targetName = PBXProject.GetUnityTargetName();
            //string targetGUID = project.TargetGuidByName(targetName);

            //AddFrameworks(project, targetGUID);
            // Write.
            //File.WriteAllText(projectPath, project.WriteToString());

            SetInfoList(path);
        }
    }

    static void SetInfoList(string path)
    {
        string plistPath = Path.Combine(path, "Info.plist");

        //PlistDocument plist = new PlistDocument();
        //plist.ReadFromString(File.ReadAllText(plistPath));

        //PlistElementDict rootDict = plist.root;

        ////picture
        //rootDict.SetString("NSPhotoLibraryUsageDescription", "Save media to Photos");
        //rootDict.SetString("NSPhotoLibraryAddUsageDescription", "Save media to Photos");
        //rootDict.SetString("NSCameraUsageDescription", "Take Photos");

        ////location
        //rootDict.SetString("NSLocationAlwaysUsageDescription", "This app requires access to the location library.");
        //rootDict.SetString("NSLocationWhenInUseUsageDescription", "This app requires access to the location library.");

        //admob nếu cần
        //rootDict.SetString("gad_preferred_webview", "wkwebview");
        //rootDict.SetString("GADApplicationIdentifier", "ca-app-pub-3940256099942544~1458002511");


        //File.WriteAllText(plistPath, plist.WriteToString());
    }

    /*
    static void AddFrameworks(PBXProject project, string targetGUID)
    {
        // Frameworks (eppz! Photos, Google Analytics).
        project.AddFrameworkToProject(targetGUID, "MobileCoreServices.framework", false);
        project.AddFrameworkToProject(targetGUID, "SystemConfiguration.framework", false);
        project.AddFrameworkToProject(targetGUID, "UserNotifications.framework", false);
        project.AddFrameworkToProject(targetGUID, "CoreData.framework", false);
        project.AddFrameworkToProject(targetGUID, "AdSupport.framework", false);
        project.AddFrameworkToProject(targetGUID, "CoreTelephony.framework", false);
        project.AddFrameworkToProject(targetGUID, "EventKit.framework", false);
        project.AddFrameworkToProject(targetGUID, "EventKitUI.framework", false);
        project.AddFrameworkToProject(targetGUID, "AudioToolbox.framework", false);
        project.AddFrameworkToProject(targetGUID, "AVFoundation.framework", false);
        project.AddFrameworkToProject(targetGUID, "MediaPlayer.framework", false);
        project.AddFrameworkToProject(targetGUID, "MessageUI.framework", false);
        project.AddFrameworkToProject(targetGUID, "StoreKit.framework", false);
        project.AddFrameworkToProject(targetGUID, "CoreGraphics.framework", false);
        project.AddFrameworkToProject(targetGUID, "CoreMedia.framework", false);

        //Admob
        project.AddFrameworkToProject(targetGUID, "GLKit.framework", false);

        //Photo
        //project.AddFrameworkToProject(targetGUID, "AssetsLibrary.framework", false);

        //AppLovin
        //project.AddFrameworkToProject(targetGUID, "UIKit.framework", false);
        //project.AddFrameworkToProject(targetGUID, "WebKit.framework", false);
        //project.AddFrameworkToProject(targetGUID, "libz.dylib", false);

        //Game analytics
        //project.AddFrameworkToProject(targetGUID, "libsqlite3.tbd", false);

        // Add `-ObjC` to "Other Linker Flags", Disbale Bitcode
        project.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-ObjC");
        project.SetBuildProperty(targetGUID, "ENABLE_BITCODE", "NO");
        project.SetBuildProperty(targetGUID, "CLANG_ENABLE_MODULES", "YES");


    }
    */
}