using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Authentication;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.Standard.DD.BillChangePlugIns3s6.Inquiry
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("生产订单表单开工判断是否锁库")]
    public class ProOrderBillPlugin : AbstractBillPlugIn
    {

        public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
        {
            base.BeforeDoOperation(e);
            if (e.Operation.FormOperation.OperationName.ToString().Equals("执行至开工"))
            {
                try
                {
                    // 获取分录当前选中行明细id
                    var entryRowIndex = this.Model.GetEntryCurrentRowIndex("FTreeEntity");
                    Entity entity = this.Model.BusinessInfo.GetEntity("FTreeEntity");
                    DynamicObject dynamics = this.Model.GetEntityDataObject(entity, entryRowIndex);
                    if (Convert.ToBoolean(dynamics["F_UJED_CheckBox"])) return;
                    StartLocks.LocksUtils.Locks(dynamics["id"].ToString(), this.Context);
                }
                catch (Exception ex)
                {
                    throw new KDException("ex", ex.ToString());
                }
            }
            if (e.Operation.FormOperation.OperationName.ToString().Equals("反执行至计划")|| e.Operation.FormOperation.OperationName.ToString().Equals("反执行至计划确认"))
            {
                try
                {
                    // 获取分录当前选中行明细id
                    var entryRowIndex = this.Model.GetEntryCurrentRowIndex("FTreeEntity");
                    Entity entity = this.Model.BusinessInfo.GetEntity("FTreeEntity");
                    DynamicObject dynamics = this.Model.GetEntityDataObject(entity, entryRowIndex);
                    if (!Convert.ToBoolean(dynamics["F_UJED_CheckBox"])) return;

                    StartLocks.LocksUtils.UnLocks(dynamics["id"].ToString(), dynamics["F_UJED_StockStatusId"].ToString(), this.Context);
                }
                catch (Exception ex)
                {
                    throw new KDException("ex", ex.ToString());
                }
            }
        }
    }
}
