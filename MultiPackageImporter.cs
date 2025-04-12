using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MultiPackageImporter : EditorWindow
{
    private Vector2 scroll;

    [MenuItem("Tools/Myong/Multi Package Importer")]
    public static void ShowWindow()
    {
        GetWindow<MultiPackageImporter>("Multi Package Importer");
    }

    private void OnGUI()
    {
        GUILayout.Space(5);
        GUILayout.Label(MultiPackageImportManager.Tr("Language", "언어"));
        MultiPackageImportManager.Language = (MultiPackageImportManager.LanguageOption)
            EditorGUILayout.EnumPopup(MultiPackageImportManager.Language);

        GUILayout.Space(10);
        GUILayout.Label(MultiPackageImportManager.Tr("Add .unitypackage Files", ".unitypackage 파일 추가"), EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(MultiPackageImportManager.Tr("Select File", "파일 선택")))
        {
            string path = EditorUtility.OpenFilePanel("Select UnityPackage", "", "unitypackage");
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                MultiPackageImportManager.Enqueue(path);
        }

        if (GUILayout.Button(MultiPackageImportManager.Tr("From Folder", "폴더에서 불러오기")))
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder", "", "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                var searchOption = MultiPackageImportManager.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                string[] files = Directory.GetFiles(folderPath, "*.unitypackage", searchOption);
                foreach (string file in files)
                    MultiPackageImportManager.Enqueue(file);
            }
        }
        EditorGUILayout.EndHorizontal();

        MultiPackageImportManager.Interactive = EditorGUILayout.Toggle(MultiPackageImportManager.Tr("Manual Import", "수동 임포트"), MultiPackageImportManager.Interactive);
        MultiPackageImportManager.IncludeSubfolders = EditorGUILayout.Toggle(MultiPackageImportManager.Tr("Include Subfolders", "하위 폴더 포함"), MultiPackageImportManager.IncludeSubfolders);

        EditorGUILayout.Space();
        GUILayout.Label($"{MultiPackageImportManager.Tr("Total", "총")}: {MultiPackageImportManager.TotalCount}, {MultiPackageImportManager.Tr("Remaining", "남음")}: {MultiPackageImportManager.GetRemainingCount()}", EditorStyles.helpBox);

        GUILayout.Label(MultiPackageImportManager.Tr("Queue:", "큐:"), EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(180));

        var queueList = new List<string>(MultiPackageImportManager.GetQueue());
        for (int i = 0; i < queueList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Path.GetFileName(queueList[i]));
            if (GUILayout.Button(MultiPackageImportManager.Tr("Remove", "제거"), GUILayout.Width(60)))
            {
                MultiPackageImportManager.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(MultiPackageImportManager.Tr("Start Import", "시작"), GUILayout.Height(30)))
        {
            MultiPackageImportManager.SetPaused(false);
            MultiPackageImportManager.StartNextImport();
        }

        if (MultiPackageImportManager.IsPaused())
        {
            if (GUILayout.Button(MultiPackageImportManager.Tr("Resume", "계속"), GUILayout.Height(30)))
                MultiPackageImportManager.SetPaused(false);
        }
        else
        {
            if (GUILayout.Button(MultiPackageImportManager.Tr("Pause", "일시정지"), GUILayout.Height(30)))
                MultiPackageImportManager.SetPaused(true);
        }

        if (GUILayout.Button(MultiPackageImportManager.Tr("Clear", "초기화"), GUILayout.Height(30)))
        {
            MultiPackageImportManager.ClearQueue();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.Label(MultiPackageImportManager.Tr("Tip: Drag and drop .unitypackage files into this window.", "이 창에 .unitypackage 파일을 드래그하세요."), EditorStyles.helpBox);

        HandleDragAndDrop();
    }

    private void HandleDragAndDrop()
    {
        Event evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (string path in DragAndDrop.paths)
                {
                    if (path.EndsWith(".unitypackage") && File.Exists(path))
                    {
                        MultiPackageImportManager.Enqueue(path);
                    }
                }
            }

            evt.Use();
        }
    }
}
