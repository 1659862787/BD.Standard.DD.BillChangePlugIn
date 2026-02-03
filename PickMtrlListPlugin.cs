using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BD.Standard.DD.BillChangePlugIns3s6
{
    [Description("简单生产领料单插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class PickMtrlListPlugin : AbstractListPlugIn
    {

        public override void AfterBindData(EventArgs e)
        {
            DynamicObject FBILLTYPE = (DynamicObject)this.View.Model.GetValue("FBILLTYPE");
            string billtype = FBILLTYPE["Number"].ToString();

            if (billtype.Equals("FLLLD01"))
            {
                LocaleValue title = new LocaleValue();
                title.Merger("辅料领料单");

                this.View.SetFormTitle(title);
                this.View.GetFieldEditor("FBILLTYPE", 0).Enabled = false;
            }

        }
        public override void AfterDoOperation(AfterDoOperationEventArgs e)
        {
            DynamicObject FBILLTYPE = (DynamicObject)this.View.Model.GetValue("FBILLTYPE");
            string billtype = FBILLTYPE["Number"].ToString();

            if (e.Operation.OperationName.ToString().Equals("关闭")) return;
            if (billtype.Equals("FLLLD01") && e.OperationResult!=null && e.OperationResult.IsSuccess)
            {
                if (e.Operation.OperationName.ToString().Equals("保存"))
                {
                    this.View.ShowMessage("辅料领料单，" + e.Operation.OperationName.ToString() + "成功!");
                }
                else
                {
                    this.View.ShowMessage("单据编号为“" + this.View.Model.GetValue("Fbillno").ToString() + "”的辅料领料单，" + e.Operation.OperationName.ToString() + "成功!");
                }
                
            }

        }
    }
    
}
