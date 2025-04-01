using UnityEngine;
using UnityEngine.UI;
using TMPro; // Optional for TextMeshPro
using System.IO;

public class FileUpload : MonoBehaviour
{
    
    public TMP_Text fileNameTMP; // TextMeshPro
    public static string selectedFilePath;

    
    public void OpenFileDialog()
    {
        // Open a file dialog (this works in the Editor and Standalone builds)
        selectedFilePath = UnityEditor.EditorUtility.OpenFilePanel("Select a File", "", "*");

        if (!string.IsNullOrEmpty(selectedFilePath))
        {
            Debug.Log("File selected: " + selectedFilePath);

            // Display the selected file name in the UI
            if (fileNameTMP != null)
                fileNameTMP.text = Path.GetFileName(selectedFilePath);
            
        }
        else
        {
            Debug.Log("No file selected.");
        }
    }

    
}
