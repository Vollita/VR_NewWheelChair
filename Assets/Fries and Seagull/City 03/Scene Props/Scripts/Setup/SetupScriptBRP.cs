﻿#if UNITY_EDITOR

using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Seagull.City_03.SceneProps.Setup {
    public static class SetupScriptBRP {
        
        private const string PostProcessPackage = "com.unity.postprocessing";
        private const string UniversalRpPackage = "com.unity.render-pipelines.universal";
        private const string MathematicsPackage = "com.unity.mathematics";

        // 当脚本重新编译完成后，自动执行以下方法
        private static void checkPackagesOnLoad() {
            // 同步方式列出已安装的 package
            ListRequest listRequest = Client.List(true);
            while (!listRequest.IsCompleted) {
                Thread.Sleep(100);
            }

            if (listRequest.Status == StatusCode.Success) {
                bool hasPostProcess = false;
                bool hasUniversalRP = false;
                bool hasMathematics = false;

                // 检查当前已安装的所有包
                foreach (var package in listRequest.Result) {
                    if (package.name == PostProcessPackage)
                        hasPostProcess = true;

                    if (package.name == UniversalRpPackage)
                        hasUniversalRP = true;

                    if (package.name == MathematicsPackage)
                        hasMathematics = true;
                }

                // 如果没有 postprocessing，就安装
                if (!hasPostProcess) {
                    Debug.Log("Postprocessing package not found. Installing...");
                    AddRequest addRequest = Client.Add(PostProcessPackage);
                    while (!addRequest.IsCompleted) {
                        Thread.Sleep(100);
                    }

                    if (addRequest.Status == StatusCode.Success)
                        Debug.Log("Postprocessing package installed successfully.");
                    else
                        Debug.LogError($"Failed to install {PostProcessPackage}. Error: {addRequest.Error.message}");
                }
                // 如果没有 数学包，就安装
                if (!hasMathematics) {
                    Debug.Log("Mathematics package not found. Installing...");
                    AddRequest addRequest = Client.Add(MathematicsPackage);
                    
                    while (!addRequest.IsCompleted) Thread.Sleep(100);

                    if (addRequest.Status == StatusCode.Success) Debug.Log("Mathematics package installed successfully.");
                    else Debug.LogError($"Failed to install {MathematicsPackage}. Error: {addRequest.Error.message}");
                }

                // 如果有 Universal RP，就卸载
                if (hasUniversalRP) {
                    Debug.Log("Universal Render Pipeline found. Removing...");
                    RemoveRequest removeRequest = Client.Remove(UniversalRpPackage);
                    while (!removeRequest.IsCompleted) {
                        Thread.Sleep(100);
                    }

                    if (removeRequest.Status == StatusCode.Success)
                        Debug.Log("Universal Render Pipeline removed successfully.");
                    else
                        Debug.LogError($"Failed to remove {UniversalRpPackage}. Error: {removeRequest.Error.message}");
                }
            }
            else {
                Debug.LogError($"Failed to list packages. Error: {listRequest.Error.message}");
            }
        }
        
        [MenuItem("Tools/Fries/City 03/Setup Built-in Rendering Pipeline", priority = 1)]
        public static void setup() {
            checkPackagesOnLoad();
            Debug.Log("Import of dependencies complete!");
            
            string packagePath = "Assets/Fries and Seagull/City 03/Pipelines/Built-in Pipeline.unitypackage";
            AssetDatabase.ImportPackage(packagePath, false);
            Debug.Log("Import of .unitypackage complete!");
            
            // 创建 Fries Props Manager
            GameObject friesManager = GameObject.Find("Fries Props Manager");
            if (friesManager == null) {
                friesManager = new GameObject("Fries Props Manager");
                friesManager.AddComponent<FriesManagerBRP>();
            }
            
            if (!friesManager.GetComponent<FriesManagerBRP>()) 
                Debug.LogError("You have an invalid Fries Props Manager in the scene. Please delete it and try again.");
            
            // 清除当前的选择
            Selection.activeGameObject = null;
            // 设置当前对象为选中状态
            Selection.activeGameObject = friesManager;
            // 也可以将视图聚焦到该对象上
            EditorGUIUtility.PingObject(friesManager);
        }

        // [MenuItem("Tools/Fries/City 03/Setup for BRP Lighting Example (Optional)", priority = 51)]
        // public static void light() {
        //     FriesManagerBRP.setupLight();
        // }
        
        // [MenuItem("Tools/Fries/City 03/Revert BRP Lighting Example Setting (Optional)", priority = 52)]
        // public static void delight() {
        //     FriesManagerBRP.unsetLight();
        // }
    }
}

#endif
