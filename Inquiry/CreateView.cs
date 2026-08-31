
using System;
using System.Collections.Generic;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using System.ComponentModel;
using Kingdee.BOS.Util;
using Kingdee.BOS.JSON;
using OfficeOpenXml;
using System.IO;

namespace BD.Standard.DD.BillChangePlugIns.Inquiry
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("询价单数据引入处理(未使用)")]
    public class CreateView : AbstractDynamicFormPlugIn
    {
        const string FileUploadServicesDir = "FileUploadServices/UploadFiles";
        string fid = "0";

        public override void BeforeBindData(EventArgs e)
        {
            base.AfterBindData(e);
            if (this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "UJED_PurInquiry")
            {
                this.View.Model.SetValue("FBuildType", "1");
                this.View.Model.SetValue("FChkAppend", true);
            }
        }


        public override void CustomEvents(CustomEventsArgs e)
        {
            base.CustomEvents(e);
            if (this.View.ParentFormView.BillBusinessInfo.GetForm().Id == "UJED_PurInquiry" && e.Key.EqualsIgnoreCase("FFileUpdate"))
            {
                string aa = e.EventName;
                Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel.Control control = this.View.GetControl("FFileUpdate");


                var postData = KDObjectConverter.DeserializeObject<JSONObject>(e.EventArgs);
                if (postData != null && postData.ToString().Contains("NewValue"))
                {
                    var uploadInfo = new JSONArray(postData["NewValue"].ToString());
                    if (uploadInfo.Count > 0)
                    {
                        // 取上传的文件名
                        var fileInfo = uploadInfo[0] as Dictionary<string, object>;
                        if (fileInfo != null)
                        {
                            var fileName = fileInfo["ServerFileName"].ToString();
                            var physicalPath = PathUtils.GetPhysicalPath(FileUploadServicesDir, fileName);

                            var sourcePackage = new ExcelPackage(new FileInfo(physicalPath));
                            // 获取第一个工作表
                            var sourceWorksheet = sourcePackage.Workbook.Worksheets[0];
                            // 复制数据
                           
                            for (int row = 4; row <= sourceWorksheet.Dimension.Rows; row++)
                            {
                                // 复制单元格数据
                                sourceWorksheet.Cells[row, 7].Value = int.TryParse(sourceWorksheet.Cells[row - 1, 7].Value.ToString(), out int value) ? value + 1 : 0;
                            }
                            fid=sourceWorksheet.Cells[3, 1].Value.ToString();
                            sourcePackage.Save();
                        }

                    }

                }

            }

        }





    }
}
