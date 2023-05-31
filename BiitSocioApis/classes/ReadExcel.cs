using BiitSocioApis.Models;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;

namespace BiitSocioApis.classes
{
    public class ReadExcel
    {
        private static List<Timetable> timeTables =new List<Timetable>();
        private static string filePath;
        public static List<Timetable> readExcel(string filePath,string toUpload) {

           if (toUpload == "dateSheet") {
                uploadDateSheet(filePath);
                return timeTables;

            }
            else
            {
                string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=YES;\"";

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        DataTable schemaTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                        List<object> objects = new List<object>();
                        foreach (DataRow item in schemaTable.Rows)
                        {
                            string sheetName = item["TABLE_NAME"].ToString().Replace("$", "");
                            DataTable dataTable = new DataTable();


                            string n = string.Concat("SELECT * FROM [", sheetName, "$]");
                            n = n.Replace("'", "");

                            OleDbDataAdapter dataAdapter = new OleDbDataAdapter(n, connection);

                            dataAdapter.Fill(dataTable);

                            String data = "";
                            if (toUpload == "timeTable")
                            {
                                uploadTimeTable(dataTable);
                            }




                        }
                        connection.Close();
                        return timeTables;
                    }
                    catch (Exception ex)
                    {
                        connection.Close();
                        return timeTables;
                    }




                }
            }
        }
      
        public static DataTable ReadExcels(string filePath)
        {
            // Create an empty DataTable to store the data
            DataTable dataTable = new DataTable();

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        // Read each sheet
                        DataTable sheetTable = new DataTable();
                        
                        // Read the header row to get the column names
                        reader.Read();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            sheetTable.Columns.Add(reader.GetString(i));
                        }

                        // Read the data rows
                        while (reader.Read())
                        {
                            DataRow row = sheetTable.NewRow();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                // Check if the cell contains a concatenated value
                                if (reader.GetFieldType(i) == typeof(string) && reader.GetString(i).Contains(","))
                                {
                                    string[] values = reader.GetString(i).Split(',');
                                    foreach (string value in values)
                                    {
                                        row[i] = value.Trim();
                                        sheetTable.Rows.Add(row.ItemArray);
                                    }
                                }
                                else
                                {
                                    row[i] = reader.GetValue(i);
                                }
                            }
                            sheetTable.Rows.Add(row);
                        }

                        // Merge the sheet data into the main DataTable
                        dataTable.Merge(sheetTable);
                    }
                    while (reader.NextResult()); // Move to the next sheet if available
                }
            }

            return dataTable;
        }
       
        private static void uploadTimeTable(DataTable dataTable) {
            try {

              
              
                string section = "";
                foreach (DataRow row in dataTable.Rows)
                        {

                    if (row[0].ToString().Contains("Time"))
                    {
                        try
                        {
                            section = row[0].ToString().Split(':')[1];
                        }
                        catch (Exception ex)
                        {
                            section = "";
                        }
                    }
                    else {
                       
                            Timetable timetable = new Timetable()
                            {
                                slot = row[0].ToString().Trim(),
                                monday = row[1].ToString().Trim(),
                                tuesday = row[2].ToString().Trim(),
                                wednesday = row[3].ToString().Trim(),
                                thursday = row[4].ToString().Trim(),
                                friday = row[5].ToString().Trim(),
                                teacherName ="",
                                courseName = "",
                                section = section
                            };if(timetable.slot!=null&& timetable.slot != "")
                            timeTables.Add(timetable);
                        
                    }
                  
                }


            }
            catch (Exception ex) { }
        }
      
        public static string ExtractTeacherName(string inputString)
        {
            string teacherName = "";
            try
            {
                if (inputString == "" || inputString == null)
                { return ""; }
                    teacherName = inputString.Split('(')[1].Trim().Split(')')[0].Trim();
                

                    
                    if (teacherName.Contains("BCS-") || teacherName.Contains("BAI-") || teacherName.Contains("BSSE-"))
                    {
                    String[] d = inputString.Split('(');
                       teacherName = ExtractTeacherName("("+d[d.Length-1]);
                    }
                    else
                    {
                    return teacherName;
                    }
                
            }
            catch (Exception ex) { }
            return teacherName;
        }
        private static void uploadDateSheet(string filePath) {
            try
            {
                DataTable dataTable = new DataTable();

                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            // Read each sheet
                            DataTable sheetTable = new DataTable();

                            // Read the header row to get the column names
                            reader.Read();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                sheetTable.Columns.Add(reader.GetString(i));
                            }

                            // Read the data rows
                            while (reader.Read())
                            {
                                DataRow row = sheetTable.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    // Check if the cell contains a concatenated value
                                    
                                        row[i] = reader.GetValue(i);
                                    
                                }
                                sheetTable.Rows.Add(row);
                            }

                            // Merge the sheet data into the main DataTable
                            dataTable.Merge(sheetTable);
                        }
                        while (reader.NextResult()); // Move to the next sheet if available
                    }
                }


                string time = "";
                string exam = "";
                string venue = "";
                List<DateeSheet> dateeSheets = new List<DateeSheet>();
                List<String> days = new List<string>();
                foreach (DataRow item in dataTable.Rows)
                {
                    //if(item.)
                    var v = item.ItemArray[0].ToString();
                    List<string> data = item.ItemArray.Select(s => s.ToString()).ToList();
                  
                    if (data.Any(s => s.ToString().ToLower().Contains("Time:".ToLower()) && s.ToString().ToLower().Contains("-".ToLower())))
                    {
                        time = data.Where(s => s.ToString().ToLower().Contains("Time:".ToLower()) && s.ToString().ToLower().Contains("-".ToLower())).Select(s => s.ToString()).FirstOrDefault();
                        time += "~"+DateTime.Now.ToShortDateString();
                    }
                    else if (data.Any(s => s.ToLower().Contains("Exam".ToLower()) && s.ToLower().Contains("Sheet".ToLower())))
                    {
                        exam = data.Where(s => s.ToString().ToLower().Contains("Exam".ToLower()) && s.ToString().ToLower().Contains("Sheet".ToLower())).Select(s => s.ToString()).FirstOrDefault();

                    }
                    else if (item.ItemArray.Any(s => s.ToString().ToLower().Contains("Date".ToLower()) && s.ToString().ToLower().Contains("Day".ToLower())))
                    {
                        days = item.ItemArray.Select(s => s.ToString()).ToList();
                    }
                    else if (item.ItemArray.Any(s => s.ToString().ToLower().Contains("venue:".ToLower())))
                    {
                        venue = item.ItemArray.Where(s => s.ToString().ToLower().Contains("venue:".ToLower())).Select(s => s.ToString()).FirstOrDefault();
                    }
                    else {
                        

                        for (int i = 1; i < item.ItemArray.Length; i++)
                        {
                            DateeSheet d = new DateeSheet();
                            d.paper = item.ItemArray[i].ToString();
                            d.venue = venue;
                            d.section = days[i].Trim();
                            d.day = item.ItemArray[0].ToString().Trim();
                            d.Time = time;
                            d.examType = exam.Trim();
                            dateeSheets.Add(d);
                        }
                     
                        }
                    Console.Write(item);
                }
                
                BIITSOCIOEntities db = new BIITSOCIOEntities();
                if (dateeSheets.Count > 0)
                {
                    var data=db.DateeSheets.AsEnumerable().Where(s => s.examType.Trim() == dateeSheets[0].examType.Trim()).ToList();
                    db.DateeSheets.RemoveRange(data);
                    
                    db.DateeSheets.AddRange(dateeSheets);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                
            }
        }
        private static void uploadCallender(string filePath)
        {
            try
            {
                DataTable dataTable= ReadExcels(filePath);
                foreach (var item in dataTable.Rows)
                {
                    Console.Write(item);
                }
            }
                
            catch (Exception ex)
            {

                
            }
        }
        public static string givePath(String toget)
        {
            string Path = Environment.CurrentDirectory;

            string[] paths = Path.Split('\\');
            Path = "";
            foreach (string path in paths)
            {
                if (path == "bin")
                {
                    return Path + "" + toget + "\\";
                }
                Path += path + "\\";
            }

            return "";
        }
    }
}