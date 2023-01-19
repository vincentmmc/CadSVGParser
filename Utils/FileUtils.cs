using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaveFileDialog = Autodesk.AutoCAD.Windows.SaveFileDialog;

namespace CADParser.Utils
{
    class FileUtils
    {
        public static void OpenDialogToSaveFile(string context)
        {
            SaveFileDialog dialog = new SaveFileDialog("Save As", "output.svg", "", "", SaveFileDialog.SaveFileDialogFlags.AllowAnyExtension);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SaveFile(dialog.Filename, context);
            }
        }

        public static void OpenFolderDialogToSaveFiles(Dictionary<string, string> blockSvgDict)
        {
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "Select Folder";
            if (dilog.ShowDialog() == DialogResult.OK)
            {
                foreach (string k in blockSvgDict.Keys)
                {
                    string context = blockSvgDict[k];
                    SaveFile(Path.Combine(dilog.SelectedPath, k + ".svg"), context);
                }
            }
        }

        public static void SaveFile(string filePath, string context)
        {
            string directoryName = ReplaceInvalidChars(Path.GetDirectoryName(filePath), Path.GetInvalidPathChars());
            string name = ReplaceInvalidChars(Path.GetFileName(filePath), Path.GetInvalidFileNameChars());
            if (!Directory.Exists(directoryName))
            {
                return;
            }
            string fileName = Path.Combine(directoryName, name);
            FileStream fs = new FileStream(fileName, FileMode.Create);
            byte[] data = new UTF8Encoding().GetBytes(context);
            fs.Write(data, 0, data.Length);
            fs.Close();
        }

        public static string GetSaveFolder()
        {
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "Select Folder";
            if (dilog.ShowDialog() == DialogResult.OK || dilog.ShowDialog() == DialogResult.Yes)
            {
                return dilog.SelectedPath;
            }
            return "";
        }

        private static string ReplaceInvalidChars(string input, char[] invalidChars)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (invalidChars.Contains(c))
                {
                    sb.Append("_invalidchar_");
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
