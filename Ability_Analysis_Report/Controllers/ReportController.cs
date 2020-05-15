using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebGrease.Css.Ast;

namespace Ability_Analysis_Report.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReportVer2_RDLC()
        {
            return View(GetExamineeAnswerStatuses());
        }

        public ActionResult ReportVer2_HTML()
        {
            return View(GetExamineeAnswerStatuses());
        }

        public ActionResult ReportVer1_RDLC()
        {
            var reportViewer = GetReport();

            //呼叫 ReportViewer.LoadReport 的 Render function，將資料轉成想要轉換的格式，並產生成Byte資料
            //轉成 Image ，格式為 image/Tif ( tif 是多張圖，瀏覽器不支援直接預覽，所以會變成下載 )
            byte[] tBytes = reportViewer.LocalReport.Render("Image");

            #region 拆解 tiff 並將圖片設為 png 格式，轉為 base64 後存起來
            MemoryStream ms = new MemoryStream(tBytes);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            Guid guid = (Guid)img.FrameDimensionsList.GetValue(0);
            FrameDimension dimension = new FrameDimension(guid);
            int totalPage = img.GetFrameCount(dimension);

            List<string> result = new List<string>();
            for (int i = 0; i < totalPage; i++)
            {
                // 選擇多張圖底下的圖，在進行匯出
                img.SelectActiveFrame(dimension, i);

                byte[] data = null;
                using (MemoryStream oMemoryStream = new MemoryStream())
                {
                    img.Save(oMemoryStream, ImageFormat.Png);
                    //設定資料流位置，在read之前務必設定為0
                    oMemoryStream.Position = 0;
                    //設定 buffer 長度
                    data = new byte[oMemoryStream.Length];
                    //將資料寫入 buffer
                    oMemoryStream.Read(data, 0, Convert.ToInt32(oMemoryStream.Length));
                    //將所有緩衝區的資料寫入資料流
                    //oMemoryStream.Flush();
                }

                var base64 = Convert.ToBase64String(data);
                var imgSrc = String.Format("data:image/png;base64,{0}", base64);
                result.Add(imgSrc);
            }
            #endregion            

            return View(result);
        }
        
        public FileResult ReportToPDF()
        {
            var reportViewer = GetReport();

            // 用這個不會跳出新 Tab，會直接下載 PDF
            return File(reportViewer.LocalReport.Render("PDF"), "application/pdf", "測試報告.pdf");
            // 用這個會跳出新 Tab
            //return File(tBytes, "application/pdf");
        }

        public ReportViewer GetReport()
        {
            ReportViewer reportViewer = new ReportViewer();
            reportViewer.ProcessingMode = ProcessingMode.Local;
            reportViewer.LocalReport.ReportPath = $"{Request.MapPath(Request.ApplicationPath)}Reports\\Report_Python.rdlc";
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Data_ExamineeEvaluationItemStatus", GetExamineeEvaluationItemStatuss()));
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Data_EvaluationItemComment", GetEvaluationItemComment()));
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Data_CoverScoreInfo", GetCoverScoreInfo()));

            // 如果想調整答題狀況的表格請特別注意，目前做法
            // 第 1 個資料集只給 1~17 題答案
            // 第 2 個資料集給 18~34 題答案
            // 第 3 個資料集給 35~50 題答案
            // 寬度變動會影響結果！！！
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Data_ExamineeAnswerStatus_1_17", GetExamineeAnswerStatuses().Take(17)));
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Data_ExamineeAnswerStatus_18_34", GetExamineeAnswerStatuses().Skip(17).Take(17)));
            reportViewer.LocalReport.DataSources.Add(new ReportDataSource("Data_ExamineeAnswerStatus_35_50", GetExamineeAnswerStatuses().Skip(34).Take(16)));


            ReportParameterCollection parameters = new ReportParameterCollection();

            // 允許外部圖片
            reportViewer.LocalReport.EnableExternalImages = true;
            // 加入圖片參數
            // 用路徑給，RDLC 那邊配置的運算式 => ="file:\\\" + Parameters!importImage.Value
            parameters.Add(new ReportParameter("ImageRadar", HostingEnvironment.MapPath("~\\RadarImages\\test.jpg")));
            // 報告中個人資訊參數
            parameters.Add(new ReportParameter("ExamDate", DateTime.Now.ToString("yyyy/MM/dd")));
            parameters.Add(new ReportParameter("ExamineeName", "韓羽伶"));
            parameters.Add(new ReportParameter("ExamineeSchool", "明新科大"));
            parameters.Add(new ReportParameter("ExamineeID", "A123456789"));
            reportViewer.LocalReport.SetParameters(parameters);

            return reportViewer;
        }

        /// <summary> 封面分數資料 ( 資料集只能接受清單型態，所以雖然只有一筆還是回傳清單型態 ) </summary> 
        public IEnumerable<ViewData_CoverScoreInfo> GetCoverScoreInfo()
        {
            var result = new List<ViewData_CoverScoreInfo>();

            var allItem = GetEvaluationItems();
            var examinessAnswers = GetExamineeAnswers();
            var basic_EvaluationQuestions = GetEvaluationQuestions();


            var temp = new ViewData_CoverScoreInfo();
            var totalQuestionCount = basic_EvaluationQuestions.Count();
            temp.TotalQuestionCount = totalQuestionCount;

            // 找出 User 答對的題目數量    
            var correctQuestionCount = basic_EvaluationQuestions.Where(x =>
            {
                var question = examinessAnswers.FirstOrDefault(exam => exam.QuestionNum == x.QuestionNum);
                if (question == null)
                    return false;

                if (question.Answer != x.StandardAnswer)
                    return false;

                return true;

            })
            .Count();

            temp.CorrectQuestionCount = correctQuestionCount;
            temp.WrongQuestionCount = totalQuestionCount - correctQuestionCount;
            var score = (100 / totalQuestionCount) * correctQuestionCount;
            temp.Score = score;
            temp.ExamResult = score > 70 ? "合格" : "不合格";

            result.Add(temp);


            return result;
        }

        /// <summary> 各能力面向表現 </summary> 
        public IEnumerable<ViewData_EvaluationItemComment> GetEvaluationItemComment()
        {
            var allItem = GetEvaluationItems().ToList();

            var result = new List<ViewData_EvaluationItemComment>();

            var commentArray = new List<string>() {
                "表現卓越，顯示您已具備該向度的堅強實力。",
                "表現不錯，有挑戰高分的實力。",
                "表現太差，回去多練練。"
            };

            var random = new Random(Guid.NewGuid().GetHashCode());

            for (int i = 1; i <= allItem.Count; i++)
            {
                var item = allItem[i - 1];
                result.Add(new ViewData_EvaluationItemComment()
                {
                    SeqNo = i,
                    Display = item.Display,
                    Comment = commentArray[random.Next(0, 3)],
                });
            }

            return result;
        }

        /// <summary> 評定項目 </summary>        
        public IEnumerable<ViewData_ExamineeEvaluationItemStatus> GetExamineeEvaluationItemStatuss()
        {
            var result = new List<ViewData_ExamineeEvaluationItemStatus>();

            var allItem = GetEvaluationItems();
            var examinessAnswers = GetExamineeAnswers();
            var basic_EvaluationQuestions = GetEvaluationQuestions();

            foreach (var item in allItem)
            {
                var temp = new ViewData_ExamineeEvaluationItemStatus();

                temp.ItemCode = item.Code;
                temp.ItemDisplay = item.Display;
                // 找出同 Code 的題目
                var sameCodeQuestions = basic_EvaluationQuestions.Where(x => x.ItemCode == item.Code);
                // 找出 User 答對的題目數量    
                var correctQuestionCount = sameCodeQuestions.Where(x =>
                {
                    var question = examinessAnswers.FirstOrDefault(exam => exam.QuestionNum == x.QuestionNum);
                    if (question == null)
                        return false;

                    if (question.Answer != x.StandardAnswer)
                        return false;

                    return true;

                })
                .Count();

                temp.CorrectQuestionCount = correctQuestionCount;
                temp.TotalQuestionCount = sameCodeQuestions.Count();

                result.Add(temp);
            }


            return result;
        }

        /// <summary> 考生答題狀況 </summary>        
        public IEnumerable<ViewData_ExamineeAnswerStatus> GetExamineeAnswerStatuses()
        {
            var examinessAnswers = GetExamineeAnswers();
            var basic_EvaluationQuestions = GetEvaluationQuestions();

            var result = new List<ViewData_ExamineeAnswerStatus>();

            foreach (var examinessAnswer in examinessAnswers)
            {
                var tempViewData = new ViewData_ExamineeAnswerStatus();
                tempViewData.QuestionNum = examinessAnswer.QuestionNum;
                tempViewData.ExamineeAnswer = examinessAnswer.Answer;
                tempViewData.StandardAnswer = basic_EvaluationQuestions.FirstOrDefault(x => x.QuestionNum == examinessAnswer.QuestionNum)?.StandardAnswer;

                result.Add(tempViewData);
            }

            return result;
        }

        /// <summary> 取得所有能力項目 </summary> 
        public IEnumerable<Basic_EvaluationItem> GetEvaluationItems()
        {
            return new List<Basic_EvaluationItem>()
            {
                new Basic_EvaluationItem(){ Code = "AA", Display="資料結構與演算法" },
                new Basic_EvaluationItem(){ Code = "BB", Display="程式設計" },
                new Basic_EvaluationItem(){ Code = "CC", Display="系統平台" },
                new Basic_EvaluationItem(){ Code = "DD", Display="資料表示、處理及分析" },
                new Basic_EvaluationItem(){ Code = "EE", Display="資訊科技應用"},
                new Basic_EvaluationItem(){ Code = "FF", Display="資訊科技與社會人類" },
            };
        }

        /// <summary> 取得所有題目 </summary> 
        public IEnumerable<Basic_EvaluationQuestion> GetEvaluationQuestions()
        {
            var result = new List<Basic_EvaluationQuestion>();

            var answerArray = new List<string>() { "A", "B", "C", "D" };
            var random = new Random(Guid.NewGuid().GetHashCode());

            // AA 10 題，剩下 8 題
            int questionNum = 1;

            #region ItemCode = AA
            for (int i = questionNum; i <= 10; i++)
            {
                result.Add(new Basic_EvaluationQuestion()
                {
                    QuestionNum = i,
                    StandardAnswer = answerArray[random.Next(0, 4)],
                    ItemCode = "AA"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = BB
            for (int i = questionNum; i <= 18; i++)
            {
                result.Add(new Basic_EvaluationQuestion()
                {
                    QuestionNum = i,
                    StandardAnswer = answerArray[random.Next(0, 4)],
                    ItemCode = "BB"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = CC
            for (int i = questionNum; i <= 26; i++)
            {
                result.Add(new Basic_EvaluationQuestion()
                {
                    QuestionNum = i,
                    StandardAnswer = answerArray[random.Next(0, 4)],
                    ItemCode = "CC"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = DD
            for (int i = questionNum; i <= 34; i++)
            {
                result.Add(new Basic_EvaluationQuestion()
                {
                    QuestionNum = i,
                    StandardAnswer = answerArray[random.Next(0, 4)],
                    ItemCode = "DD"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = EE
            for (int i = questionNum; i <= 42; i++)
            {
                result.Add(new Basic_EvaluationQuestion()
                {
                    QuestionNum = i,
                    StandardAnswer = answerArray[random.Next(0, 4)],
                    ItemCode = "EE"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = FF
            for (int i = questionNum; i <= 50; i++)
            {
                result.Add(new Basic_EvaluationQuestion()
                {
                    QuestionNum = i,
                    StandardAnswer = answerArray[random.Next(0, 4)],
                    ItemCode = "FF"
                });

                questionNum++;
            }
            #endregion

            return result;
        }

        /// <summary> 取得考生答案 </summary> 
        public IEnumerable<Data_ExamineeAnswer> GetExamineeAnswers()
        {
            var result = new List<Data_ExamineeAnswer>();

            var answerArray = new List<string>() { "A", "B", "C", "D", "*" };
            var random = new Random(new Guid().GetHashCode());

            // AA 10 題，剩下 8 題
            int questionNum = 1;

            #region ItemCode = AA
            for (int i = questionNum; i <= 10; i++)
            {
                result.Add(new Data_ExamineeAnswer()
                {
                    QuestionNum = i,
                    Answer = answerArray[random.Next(0, 5)],
                    PersonalID = "A123456789"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = BB
            for (int i = questionNum; i <= 18; i++)
            {
                result.Add(new Data_ExamineeAnswer()
                {
                    QuestionNum = i,
                    Answer = answerArray[random.Next(0, 5)],
                    PersonalID = "A123456789"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = CC
            for (int i = questionNum; i <= 26; i++)
            {
                result.Add(new Data_ExamineeAnswer()
                {
                    QuestionNum = i,
                    Answer = answerArray[random.Next(0, 5)],
                    PersonalID = "A123456789"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = DD
            for (int i = questionNum; i <= 34; i++)
            {
                result.Add(new Data_ExamineeAnswer()
                {
                    QuestionNum = i,
                    Answer = answerArray[random.Next(0, 5)],
                    PersonalID = "A123456789"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = EE
            for (int i = questionNum; i <= 42; i++)
            {
                result.Add(new Data_ExamineeAnswer()
                {
                    QuestionNum = i,
                    Answer = answerArray[random.Next(0, 5)],
                    PersonalID = "A123456789"
                });

                questionNum++;
            }
            #endregion

            #region ItemCode = FF
            for (int i = questionNum; i <= 50; i++)
            {
                result.Add(new Data_ExamineeAnswer()
                {
                    QuestionNum = i,
                    Answer = answerArray[random.Next(0, 5)],
                    PersonalID = "A123456789"
                });

                questionNum++;
            }
            #endregion

            return result;

        }

        public class ViewData_CoverScoreInfo
        {
            public int TotalQuestionCount { get; set; }

            public int CorrectQuestionCount { get; set; }

            public int WrongQuestionCount { get; set; }

            public double Score { get; set; }

            public string ExamResult { get; set; }

        }

        public class ViewData_EvaluationItemComment
        {
            public int SeqNo { get; set; }
            public string Display { get; set; }
            public string Comment { get; set; }
        }

        public class ViewData_ExamineeEvaluationItemStatus
        {
            public string ItemCode { get; set; }
            public string ItemDisplay { get; set; }
            public int CorrectQuestionCount { get; set; }
            public int TotalQuestionCount { get; set; }
        }

        public class ViewData_ExamineeAnswerStatus
        {
            /// <summary> 題號 </summary>
            public int QuestionNum { get; set; }

            /// <summary> 標準答案 </summary>
            public string StandardAnswer { get; set; }

            /// <summary> 考生答案 </summary>
            public string ExamineeAnswer { get; set; }
        }

        public class Data_ExamineeAnswer
        {
            public string PersonalID { get; set; }

            /// <summary> 題號 </summary>
            public int QuestionNum { get; set; }

            /// <summary> 答案 </summary>
            public string Answer { get; set; }
        }

        public class Basic_EvaluationItem
        {
            public string Code { get; set; }

            public string Display { get; set; }

            public IEnumerable<Basic_EvaluationQuestion> QuestionGroup { get; set; }
        }

        public class Basic_EvaluationQuestion
        {
            /// <summary> 題號 </summary>
            public int QuestionNum { get; set; }

            /// <summary> 標準答案 </summary>
            public string StandardAnswer { get; set; }

            /// <summary> 屬於哪個項目 </summary>
            public string ItemCode { get; set; }
        }
    }
}