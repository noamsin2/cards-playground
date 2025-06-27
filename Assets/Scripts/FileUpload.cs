using UnityEngine;
using TMPro;
using System;
using System.Collections;
using SimpleFileBrowser;

public class FileUpload : MonoBehaviour
{
    public TMP_Text fileNameTMP;
    public static string selectedFilePath;

    public void ShowFileDialog(Action<string> onFileSelected)
    {
        StartCoroutine(ShowFileBrowserCoroutine(onFileSelected));
    }

    private IEnumerator ShowFileBrowserCoroutine(Action<string> onFileSelected)
    {
        yield return FileBrowser.WaitForLoadDialog(
            pickMode: FileBrowser.PickMode.Files,
            allowMultiSelection: false,
            initialPath: null,
            title: "Select a File",
            loadButtonText: "Select"
        );

        if (FileBrowser.Success)
        {
            selectedFilePath = FileBrowser.Result[0];
            Debug.Log("Selected: " + selectedFilePath);

            if (fileNameTMP != null)
                fileNameTMP.text = System.IO.Path.GetFileName(selectedFilePath);

            onFileSelected?.Invoke(selectedFilePath);
        }
        else
        {
            Debug.Log("User cancelled file selection.");
            onFileSelected?.Invoke(null);
        }
    }
}
