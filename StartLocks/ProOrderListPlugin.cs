using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.List.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.ComponentModel;

namespace BD.Standard.DD.BillChangePlugIns.Inquiry
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("生产订单列表开工判断是否锁库(未使用)")]
    public class ProOrderListPlugin : AbstractListPlugIn
    {

        public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
        {
            base.BeforeDoOperation(e);
            string operationName = e.Operation.FormOperation.OperationName.ToString();
            if (e.Operation.FormOperation.OperationName.ToString().Equals("执行至开工"))
            {
                try
                {
                    ListSelectedRowCollection listcoll = this.ListView.SelectedRowsInfo;

                    string[] FEntryids = listcoll.GetEntryPrimaryKeyValues();
                    foreach (var fentryid in FEntryids)
                    {
                        int F_UJED_CheckBox = DBUtils.ExecuteScalar<int>(this.Context, $"select F_UJED_CheckBox from T_PRD_MOENTRY where fentryid={fentryid}", 0, null);
                        if (F_UJED_CheckBox == 1) return;

                        StartLocks.LocksUtils.Locks(fentryid, this.Context);
                    }
                }
                catch(Exception ex)
                {
                    throw new KDException("ex", ex.ToString());
                }
                
            }
            if (e.Operation.FormOperation.OperationName.ToString().Equals("反执行至计划") || e.Operation.FormOperation.OperationName.ToString().Equals("反执行至计划确认"))
            {
                try
                {
                    ListSelectedRowCollection listcoll = this.ListView.SelectedRowsInfo;

                    string[] FEntryids = listcoll.GetEntryPrimaryKeyValues();
                    foreach (var fentryid in FEntryids)
                    {
                        int F_UJED_CheckBox = DBUtils.ExecuteScalar<int>(this.Context, $"select F_UJED_CheckBox,F_UJED_StockStatusId from T_PRD_MOENTRY where fentryid={fentryid}", 0, null);
                        if (F_UJED_CheckBox == 0) return;

                        DynamicObjectCollection dy = DBUtils.ExecuteDynamicObject(this.Context, $"select F_UJED_CheckBox,F_UJED_StockStatusId from T_PRD_MOENTRY where fentryid={fentryid}");
                        if (dy.Count>0)
                        {
                            if (Convert.ToInt32(dy[0]["F_UJED_CheckBox"]) == 0) return;

                            StartLocks.LocksUtils.UnLocks(fentryid, dy[0]["F_UJED_StockStatusId"].ToString(), this.Context);
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw new KDException("ex", ex.ToString());
                }
            }
        }



        public override void EntryBarItemClick(BarItemClickEventArgs e)
        {
            base.EntryBarItemClick(e);

        }
    }
}
