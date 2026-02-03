using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;

namespace BD.Standard.DD.BillChangePlugIns3s6.Inquiry
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("采购申请生成采购询价单列表插件")]
    public class PurInquiryListPlugIn : AbstractListPlugIn
    {

        DataSet ds;

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            base.BarItemClick(e);

            //当点击按钮
            if (e.BarItemKey.Equals("UJED_tbButton"))
            {
                //选择哪几行
                ListSelectedRowCollection selectRows = this.ListView.SelectedRowsInfo;

                //把获取的主键赋值给一个数组
                string[] ID = selectRows.GetPrimaryKeyValues();
                string[] entryids = selectRows.GetEntryPrimaryKeyValues();

                //没有选择单据,提示
                //如果空值的长度等于0
                if (entryids.Length == 0)
                {
                    this.View.ShowMessage("请选择单据", MessageBoxType.Notice);
                    return;
                }

                CreatePurInquiry(entryids);

            }
        }

        private void CreatePurInquiry(string[] entryids)
        {
            //下推采购询价单
            JObject json = new JObject()
                {
                    new JProperty("EntryIds",string.Join(",",entryids)),
                    new JProperty("RuleId","UJED_PurInquiry"),
                    new JProperty("IsEnableDefaultRule","false"),
                    new JProperty("IsDraftWhenSaveFail","true"),
                };
            string MessageReturned = JsonConvert.SerializeObject(WebApiServiceCall.Push(Context, "PUR_Requisition", json.ToString()));
            string fid = "";
            if (JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
            {
                fid = ((Newtonsoft.Json.Linq.JContainer)JObject.Parse(JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["SuccessEntitys"][0].ToString()).First).First.ToString();

            }
            else
            {
                this.View.ShowErrMessage("无法生成采购询价单，操作终止");

            }

            if (string.IsNullOrWhiteSpace(fid)) return;
            //根据生成的报价单id查询数据

            //执行sql
            ds = DBUtils.ExecuteDataSet(this.Context, $"exec seletePurInquiry {fid}");
            //把单据编号放到dt表里面
            //金蝶对Excel操作
            string fileName = string.Format("{0}_{1}.xlsx", "采购询价单", DateTime.Now.ToString("hhmmssffffff"));
            //获取路径
            string filePath = PathUtils.GetPhysicalPath(KeyConst.TEMPFILEPATH, fileName);
            //获取服务器Url地址,把文件传到服务器上面,然后下载
            string fileUrl = PathUtils.GetServerPath(KeyConst.TEMPFILEPATH, fileName);
            string ModelfileUrl = PathUtils.GetPhysicalPath("D:\\Program Files (x86)\\Kingdee\\K3Cloud\\WebSite\\modelfilePath", "采购询价单_model.xlsx");
            ExcelPackage.License.SetNonCommercialPersonal("My Name");
            var package = new ExcelPackage();
            // 创建工作表
            var worksheet = package.Workbook.Worksheets.Add("采购询价单#单据头(FBillHead)");
            var sourcePackage = new ExcelPackage(new FileInfo(ModelfileUrl));
            // 获取第一个工作表
            var sourceWorksheet = sourcePackage.Workbook.Worksheets[0];
            // 复制数据
            for (int row = 1; row <= 2; row++)
            {
                for (int col = 1; col <= sourceWorksheet.Dimension.Columns; col++)
                {
                    // 复制单元格数据
                    worksheet.Cells[row, col].Value = sourceWorksheet.Cells[row, col].Value;
                }
            }

            var startCell = worksheet.Cells[1, 1]; // 标题行
            var endCell = worksheet.Cells[2, sourceWorksheet.Dimension.Columns]; // 数据区域的最后一行
            worksheet.Cells[startCell.Start.Row, startCell.Start.Column, endCell.End.Row, endCell.End.Column].AutoFilter = true;
            worksheet.Cells["A3"].LoadFromDataTable(ds.Tables[0], false);
            worksheet.Cells[3, 1, ds.Tables[0].Rows.Count + 2, 20].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[3, 1, ds.Tables[0].Rows.Count + 2, 20].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(169, 208, 142));

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            // 保存文件
            FileInfo file = new FileInfo(filePath);
            package.SaveAs(file);

            //打开文件下载界面
            DynamicFormShowParameter showParameter = new DynamicFormShowParameter();
            showParameter.FormId = "BOS_FileDownload";
            showParameter.OpenStyle.ShowType = ShowType.Modal;
            showParameter.CustomComplexParams.Add("url", fileUrl);

            //显示
            this.View.ShowForm(showParameter);
        }
    }
}





