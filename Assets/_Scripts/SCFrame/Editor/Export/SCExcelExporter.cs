using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// SCFrame中的配表数据导出器
    /// </summary>
    public static class SCExcelExporter
    {
        public const string GAME_EXCEL_PATH = "Assets/Resources/RefData/Excels";
        public const string GAME_TXT_PATH = "Assets/Resources/RefData/ExportTxt";
        public const int TITLE_START_INDEX = 0;

        /// <summary>
        /// 导出所有的excel表 表在GAME_EXCEL_PATH里
        /// </summary>
        [MenuItem("Excel导出/导出全部的Excel")]
        public static void ExportAllExcels()
        {
            bool hasImported = false;
            DirectoryInfo direction = new DirectoryInfo(GAME_EXCEL_PATH);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                //查找excel的后缀
                if (Path.GetExtension(files[i].FullName) == ".xls" || Path.GetExtension(files[i].FullName) == ".xlsx")
                {
                    string excelName = Path.GetFileName(files[i].FullName);

                    if (excelName.StartsWith("~$"))
                    {
                        continue;
                    }

                    ExportExcel(excelName);

                    hasImported = true;
                }
            }

            if(!hasImported)
            {
                Debug.LogError("没有找到可以导出的Excel！！！");
            }
            else
            {
                Debug.Log("所有的Excel都导出成功！！！");
            }
        }

        /// <summary>
        /// 导出Excel表 
        /// </summary>
        /// <param name="_excelName"></param>
        public static void ExportExcel(string _excelName)
        {
            string excelPath = GAME_EXCEL_PATH + "/" + _excelName;

            IWorkbook workbook = CreatWrokbook(excelPath);
            ISheet sheet = null;
            IRow row = null;
            ICell cell = null;
            string cellValue = "";
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                using (FileStream fs = File.Open(GAME_TXT_PATH + "/" + workbook.GetSheetName(i) + ".txt", FileMode.Create, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                    {

                        sheet = workbook.GetSheetAt(i);
                        if (sheet == null)
                            continue;
                        List<int> memoIdxList = new List<int>();
                        for (int j = TITLE_START_INDEX; j <= sheet.LastRowNum; j++)
                        {

                            row = sheet.GetRow(j);
                            if (row == null)
                                continue;
                            for (int k = 0; k <= row.LastCellNum; k++)
                            {
                                if (memoIdxList.Contains(k))
                                {
                                    if(k == row.LastCellNum)
                                    {
                                        sw.Write("\n");
                                    }
                                    continue;
                                }
                                cell = row.GetCell(k);
                                if (cell == null)
                                    break;
                                if (string.IsNullOrEmpty(cell.ToString()))
                                    break;
                                //标题列中元素的带# 表示该列是备注列
                                if (j == TITLE_START_INDEX && cell.ToString().StartsWith("#"))
                                {
                                    memoIdxList.Add(k);
                                    continue;
                                }
                                cellValue = cell?.ToString() ?? "";
                                sw.Write(cellValue);
                                if (k < row.LastCellNum - 1 && !string.IsNullOrEmpty(row.GetCell(k + 1)?.ToString()))
                                    sw.Write("\t");
                            }
                            sw.Write("\n");
                        }
                    }
                }
            }

            Debug.Log("导出" + _excelName + "成功！！！");
        }


        /// <summary>
        /// 创建工作簿
        /// </summary>
        /// <param name="_excelPath"></param>
        /// <returns></returns>
        private static IWorkbook CreatWrokbook(string _excelPath)
        {
            using (FileStream stream = File.Open(_excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (Path.GetExtension(_excelPath) == ".xls")
                {
                    return new HSSFWorkbook(stream);
                }
                else
                {
                    return new XSSFWorkbook(stream);
                }
            }
        }

        /// <summary>
        /// 复制Txt到StreamingAssets下 因为unity导出运行时读不到resources下的txt
        /// </summary>
        [MenuItem("Excel导出/复制txt到StreamingAssets下")]
        private static void CopyTxtToStreamingAssets()
        {
            try
            {
                string sourcePath = GAME_TXT_PATH;
                string targetPath = Path.Combine(Application.streamingAssetsPath, "ExportTxt");

                // 确保目标目录存在
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                DirectoryInfo direction = new DirectoryInfo(sourcePath);
                FileInfo[] files = direction.GetFiles("*.txt", SearchOption.AllDirectories);
                int copiedCount = 0;

                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        string targetFile = Path.Combine(targetPath, files[i].Name);
                        File.Copy(files[i].FullName, targetFile, true);
                        copiedCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"复制文件 {files[i].Name} 时出错: {ex.Message}");
                    }
                }

                Debug.Log($"复制完成! 共复制 {copiedCount} 个txt文件");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"复制过程中发生错误: {ex.Message}");
            }
        }
    }
}
