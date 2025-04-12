using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[InitializeOnLoad]
public static class MultiPackageImportManager
{
    private static Queue<string> packageQueue = new Queue<string>();
    private static bool isImporting = false;
    private static bool isPaused = false;
    private static bool isWaitingForAutoNext = false;

    public static bool Interactive { get; set; } = false;
    public static bool IncludeSubfolders { get; set; } = false;

    public static int TotalCount { get; private set; } = 0;

    public enum LanguageOption { English, Korean }

    public static LanguageOption Language
    {
        get => (LanguageOption)EditorPrefs.GetInt("MultiPackageImport_Language", 0);
        set => EditorPrefs.SetInt("MultiPackageImport_Language", (int)value);
    }

    public static string Tr(string en, string ko)
    {
        return Language == LanguageOption.Korean ? ko : en;
    }

    static MultiPackageImportManager()
    {
        Reset();
    }

    public static void Reset()
    {
        packageQueue.Clear();
        isImporting = false;
        isPaused = false;
        isWaitingForAutoNext = false;
        TotalCount = 0;
    }

    public static void Enqueue(string path)
    {
        if (!packageQueue.Contains(path))
        {
            packageQueue.Enqueue(path);
            TotalCount++;
        }
    }

    public static void RemoveAt(int index)
    {
        var list = new List<string>(packageQueue);
        if (index >= 0 && index < list.Count)
        {
            list.RemoveAt(index);
            TotalCount--;
        }
        packageQueue = new Queue<string>(list);
    }

    public static IEnumerable<string> GetQueue() => packageQueue;
    public static int GetRemainingCount() => packageQueue.Count;
    public static void ClearQueue() { packageQueue.Clear(); TotalCount = 0; }

    public static void StartNextImport()
    {
        if (isImporting || isPaused) return;

        if (packageQueue.Count == 0)
        {
            Debug.Log("[MultiPackageImporter] All packages imported.");
            EditorUtility.DisplayDialog("Done", Tr("All packages imported.", "모든 패키지를 임포트했습니다."), "OK");
            Reset();
            return;
        }

        string nextPackage = packageQueue.Dequeue();
        Debug.Log($"[MultiPackageImporter] Importing: {Path.GetFileName(nextPackage)}");
        isImporting = true;

        if (Interactive)
        {
            AssetDatabase.ImportPackage(nextPackage, true);
        }
        else
        {
            AssetDatabase.ImportPackage(nextPackage, false);
            isWaitingForAutoNext = true;
            EditorApplication.update += AutoCompleteCheck;
        }
    }

    private static void AutoCompleteCheck()
    {
        if (isWaitingForAutoNext && !isImporting)
        {
            isWaitingForAutoNext = false;
            EditorApplication.update -= AutoCompleteCheck;
            EditorApplication.delayCall += StartNextImport;
        }
    }

    public static void NotifyImportCompleted()
    {
        isImporting = false;
        if (!isPaused && !isWaitingForAutoNext)
        {
            EditorApplication.delayCall += StartNextImport;
        }
    }

    public static bool IsImporting() => isImporting;
    public static void SetPaused(bool paused)
    {
        isPaused = paused;
        if (!paused && !isImporting)
        {
            EditorApplication.delayCall += StartNextImport;
        }
    }
    public static bool IsPaused() => isPaused;
}
