using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace BD.Standard.DD.BillChangePlugIns2606X01.Calculation
{
    [Description("收货确认单判断到货日期插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class DeliveryConfirmBillPlugIn : AbstractDynamicFormPlugIn
    {


        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.FieldName.EqualsIgnoreCase("F_QEBI_Date2"))
            {
                try
                {
                    DateTime F_QEBI_Date2 = Convert.ToDateTime(e.NewValue);
                    DateTime F_QEBI_GYDATE = Convert.ToDateTime(this.View.Model.GetValue("F_QEBI_GYDATE"));
                    if (F_QEBI_Date2 <= F_QEBI_GYDATE)
                    {
                        string message = "收货确认单到货验收日期需大于发货日期";
                        this.View.ShowErrMessage(message);
                        
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
        public override void BeforeDoOperation(BeforeDoOperationEventArgs e)
        {
            base.BeforeDoOperation(e);
            try
            {
                //
                if (e.Operation.FormOperation.Operation.EqualsIgnoreCase("Save"))
                {
                    DateTime F_QEBI_Date2 = Convert.ToDateTime(this.View.Model.GetValue("F_QEBI_Date2"));
                    DateTime F_QEBI_GYDATE = Convert.ToDateTime(this.View.Model.GetValue("F_QEBI_GYDATE"));
                    if (F_QEBI_Date2 <= F_QEBI_GYDATE)
                    {
                        string message = "收货确认单到货验收日期需大于发货日期";
                        this.View.ShowErrMessage(message);
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
       
    }
}
