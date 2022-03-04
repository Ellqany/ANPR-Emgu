using System;
using System.Collections.Generic;
using System.Linq;
using Tesseract;

namespace ANPRCV.AppServices.Service.Concreate
{
    public class OCRService : IOCRService
    {
        /// <summary>
        /// Convert base64string image to byte array then get its violations
        /// </summary>
        /// <param name="HeatMap">base64string image</param>
        /// <returns>the vioations list that made by the vehicles</returns>
        public string ReadHeatMap(string HeatMap)
        {
            var heatMapbytes = Convert.FromBase64String(HeatMap);
            return GetViolation(heatMapbytes);
        }

        #region Private Methods
        /// <summary>
        /// read text from image using tesseract OCR
        /// </summary>
        /// <param name="heatMapbytes">cropped image in bytes formate</param>
        /// <returns></returns>
        static string GetViolation(byte[] heatMapbytes)
        {
            if (heatMapbytes == null)
            {
                return "";
            }

            List<string> Violation = new List<string>();
            try
            {
                using var engine = new TesseractEngine(@"./wwwroot/tessdata", "eng", EngineMode.Default);
                using var img = Pix.LoadFromMemory(heatMapbytes);
                using var page = engine.Process(img);
                var text = page.GetText();
                var Alldata = text.Split('|', StringSplitOptions.RemoveEmptyEntries).Where(x =>
                        x.ToLower().Contains("Calling State:".ToLower()) ||
                        // x.ToLower().Contains("Illegal Type:".ToLower()) ||
                        x.ToLower().Contains("SafeBelt State:".ToLower()))
                    .ToList();
                foreach (var item in Alldata)
                {
                    Violation.AddRange(GetViolation(item));
                }

                return string.Join("-", Violation);
            }
            catch (Exception)
            {
                return string.Join("-", Violation);
            }
        }

        /// <summary>
        /// get list of violations in the line
        /// </summary>
        /// <param name="line">the line that contains violations</param>
        /// <returns></returns>
        static List<string> GetViolation(string line)
        {
            List<string> Violations = new List<string>();
            var allData = line.Split(':');
            var violationData = allData[1].Split(')', '(').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            for (int i = 0; i < violationData.Count; i += 2)
            {
                string violation = violationData[i + 1].Replace(" ", string.Empty);

                if ("Calling State".ToLower() == allData[0].ToLower())
                {
                    if (violation.ToLower() == "yes")
                    {
                        Violations.Add("استخدام التليفون يدويا أثناء القيادة");
                    }
                }
                else if ("SafeBelt State".ToLower() == allData[0].ToLower())
                {
                    if (i == 0)
                    {
                        if (violation.ToLower() == "no")
                        {
                            Violations.Add("قيادة السيارة بدون حزام امان");
                        }
                    }
                    else
                    {
                        if (violation.ToLower() == "no")
                        {
                            Violations.Add("عدم ارتداء حزام الامان للمجاور للقائد اثناء السير");
                        }
                    }
                }
            }

            return Violations;
        }
        #endregion
    }
}