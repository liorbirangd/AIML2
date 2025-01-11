using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Soccer.Scripts
{
    public class MetricsLogger
    {
        private readonly string filePath;
        private int runCounter = 0;
        private bool fileExists;
        
        public MetricsLogger(string fileName = "training_metrics.csv")
        {
            string directory = Path.Combine(Application.dataPath, "../metrics");
            Directory.CreateDirectory(directory);
            filePath = Path.Combine(directory, fileName);
            fileExists = File.Exists(filePath);
            
            if (!fileExists)
            {
                string[] headers = {
                    "Run",
                    "Timestamp",
                    "TotalSteps",
                    "BlueGoals",
                    "PurpleGoals",
                    "BlueRewards",
                    "PurpleRewards"
                };
                WriteRow(string.Join(",", headers));
            }
        }

        public void LogMetrics(int steps, int blueGoals, int purpleGoals, float blueRewards, float purpleRewards)
        {

            Debug.Log($"LogMetrics called with: steps={steps}, blueGoals={blueGoals}, purpleGoals={purpleGoals}, blueRewards={blueRewards}, purpleRewards={purpleRewards}");
            runCounter++;
            string[] rowData = {
                $"Run {runCounter}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                steps.ToString(),
                blueGoals.ToString(),
                purpleGoals.ToString(),
                blueRewards.ToString("F2"),
                purpleRewards.ToString("F2")
            };
            
            //WriteRow(string.Join(",", rowData));
            WriteRow(string.Join(",","hello"));
        }
        
        private void WriteRow(string row)
        {

            Debug.Log($"WriteRow called with row: {row}");

            try
            {
                using (StreamWriter sw = new StreamWriter(filePath, true, Encoding.UTF8))
                {
                    sw.WriteLine(row);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write metrics to file: {e.Message}");
            }
        }
    }
}