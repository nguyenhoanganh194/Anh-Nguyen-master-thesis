using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class SimpleCSVFileWriter
{
    public string fullHeader = "ID,Body_part,Side,Movement,Object_name,Pick_up_time,Drop_time,Total_time,Eslapse_time,Reps,Result,Detail";

    public string FileName;
    public static readonly string FOLDER_STORAGE = Application.persistentDataPath + "/urst";
    private string pathFileTmp = string.Empty;
    private volatile bool isSaving = false;

    public SimpleCSVFileWriter(string fileName, params string[] headers)
    {
        fullHeader = string.Empty;
        fullHeader = string.Join(",", headers);
        FileName = fileName;
        Debug.LogError(fullHeader);
    }


    private string PathFile
    {
        get
        {

            var sBuilder = new StringBuilder();
            var pathFile = sBuilder.Append(FOLDER_STORAGE)
                    .Append("/")
                    .Append(FileName)
                    .Append(".csv").ToString();
            pathFileTmp = sBuilder.Replace(".csv", ".tmp").ToString();
            return pathFile;
        }
    }

    public void AddNewLine(string datas)
    {
     
        if (!Directory.Exists(FOLDER_STORAGE))
        {
            Directory.CreateDirectory(FOLDER_STORAGE);
        }
        if (isSaving) return;
        isSaving = true;
        var filePath = PathFile;
        var filePathTemp = pathFileTmp;
        Debug.LogError(datas);
        Task.Run(() =>
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError(filePath);
                    File.WriteAllText(filePath, fullHeader + Environment.NewLine);
                }
                File.AppendAllText(filePath, datas + Environment.NewLine);
            }
            catch (IOException e)
            {
                Debug.LogError(e.ToString());
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                isSaving = false;
            }
        });

    }

}