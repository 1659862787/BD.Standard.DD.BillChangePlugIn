using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;


namespace BD.Standard.DD.BillChangePlugIns3s6
{
    [Description("简单生产领料单列表插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PickMtrlListPluginList : AbstractListPlugIn
    {
        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);

            //定义过滤条件的字符串
            string filterString = "";

            string listType = this.View.OpenParameter.GetCustomParameter("BillTypeID") as string;
            string listType2 = this.View.OpenParameter.GetCustomParameter("BillTypeID2") as string;
            //判断字符串 listType的值是否为空,或者为空格,与My,不区分大小写对比  66554696c8eff1    bendi:66447e0936cb9e
            //20240402测试环境
            /*if (!string.IsNullOrWhiteSpace(listType) && listType.Equals("66554696c8eff1", StringComparison.OrdinalIgnoreCase))
            {
                filterString = string.Format(" FBillType='{0}'", listType);
            }
            else if (!string.IsNullOrWhiteSpace(listType) && listType.Equals("9a1621221f4c47cfb018a11640b1b03f", StringComparison.OrdinalIgnoreCase))
            {
                filterString = string.Format(" (FBillType='{0}'", listType);
            }
            if (!string.IsNullOrWhiteSpace(listType2) && listType2.Equals("65fa3ea3b750cd", StringComparison.OrdinalIgnoreCase))
            {
                filterString += string.Format(" or FBillType='{0}')", listType2);
            }*/
            //正式环境
            if (!string.IsNullOrWhiteSpace(listType) && listType.Equals("6745655bfe6ea7", StringComparison.OrdinalIgnoreCase))
            {
                filterString = string.Format(" FBillType='{0}'", listType);
            }
            else if (!string.IsNullOrWhiteSpace(listType) && listType.Equals("9a1621221f4c47cfb018a11640b1b03f", StringComparison.OrdinalIgnoreCase))
            {
                filterString = string.Format(" FBillType <> '6745655bfe6ea7'");
            }
            //if (!string.IsNullOrWhiteSpace(listType2) && listType2.Equals("65fa3ea3b750cd", StringComparison.OrdinalIgnoreCase))
            //{
            //    filterString += string.Format(" or FBillType='{0}')", listType2);
            //}



            //原本的过滤条件e.FilterString
            if (!string.IsNullOrWhiteSpace(e.FilterString) && !string.IsNullOrWhiteSpace(filterString))
            {
                e.FilterString += " AND ";
            }
            e.FilterString += filterString;
        }

        public override void AfterDoOperation(AfterDoOperationEventArgs e)
        {
            string listType = this.View.OpenParameter.GetCustomParameter("BillTypeID") as string;

            if (e.Operation.OperationName.ToString().Equals("关闭")) return;
            if (listType.Equals("6745655bfe6ea7") && e.OperationResult != null && e.OperationResult.IsSuccess)
            {
                this.View.ShowMessage (e.Operation.OperationName.ToString() + "成功!");
            }
        }

    }
}
