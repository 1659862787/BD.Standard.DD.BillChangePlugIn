using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.ComponentModel;

namespace BD.Standard.DD.BillChangePlugIns3s6
{
    [Description("项目生产订货变更单_插件")]
    [Kingdee.BOS.Util.HotUpdate]
    public class ProduceChanged : AbstractDynamicFormPlugIn
    {


        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            EntryEntity entry = this.View.BillBusinessInfo.GetEntryEntity("F_QEBI_Entity");

            DynamicObjectCollection dy = this.View.Model.GetEntityDataObject(entry) as DynamicObjectCollection;
            if (dy != null)
            {
                for (int row = 0; row < dy.Count; row++)
                {
                    this.View.GetFieldEditor("F_QEBI_material", row).Enabled = false;
                    this.View.GetFieldEditor("F_QEBI_Fqty", row).Enabled = false;
                    this.View.GetFieldEditor("F_QEBI_Datetime", row).Enabled = false;
                    this.View.GetFieldEditor("F_QEBI_FQTYED", row).Enabled = false;
                    this.View.GetFieldEditor("F_QEBI_Datetime", row).Enabled = false;
                }
            }


        }

        public override void AfterCreateNewData(EventArgs e)
        {
            base.AfterCreateNewData(e);
            //获取源单id
            string srcid = this.View.Model.GetValue("F_QEBI_SRCiD").ToString();
            string sql = "select fbillno from QEBI_t_Cust100003 where F_QEBI_SRCID='" + srcid + "'";
            DynamicObjectCollection dyc = DBUtils.ExecuteDynamicObject(this.Context, sql) as DynamicObjectCollection;
            //获取源单的所有变更单单据编号，处理
            string billno = string.Empty;
            if (dyc.Count > 0 && dyc != null)
            {
                int i = 0;
                foreach (var item in dyc)
                {
                    string fbillno = item["fbillno"].ToString();
                    string bill = fbillno.Substring(fbillno.LastIndexOf("_") + 2, 3);
                    billno = fbillno.Substring(0, fbillno.LastIndexOf("_"));
                    i = Convert.ToInt32(bill);
                }
                if ((++i).ToString().Length == 1)
                {
                    this.Model.DataObject["BillNo"] = billno + "_V00" + i;
                }
                if ((++i).ToString().Length == 2)
                {
                    this.Model.DataObject["BillNo"] = billno + "_V0" + i;
                }
                if ((++i).ToString().Length == 3)
                {
                    this.Model.DataObject["BillNo"] = billno + "_V" + i;
                }
            }
            else
            {
                var ss = this.View.Model.GetValue("F_QEBI_SRCBILLNO").ToString();
                this.Model.DataObject["BillNo"] = this.View.Model.GetValue("F_QEBI_SRCBILLNO").ToString() + "_V001";
            }
        }

        /// <summary>
        /// 新增行数据
        /// </summary>
        /// <param name="e"></param>
        public override void AfterCreateNewEntryRow(CreateNewEntryEventArgs e)
        {
            base.AfterCreateNewEntryRow(e);
            this.View.Model.SetValue("F_QEBI_Combo", "A", e.Row);
            this.View.GetFieldEditor("F_QEBI_Combo", e.Row).Enabled = false;
            this.View.GetFieldEditor("F_QEBI_Fqty", e.Row).Enabled = false;
            this.View.GetFieldEditor("F_QEBI_FQTYED", e.Row).Enabled = false;
            // this.View.Refresh();
        }



        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            int row = e.Row;
            if (e.Field.FieldName.EqualsIgnoreCase("F_QEBI_Combo"))
            {
                string aaa = e.NewValue.ToString();

                decimal qtyed = Convert.ToDecimal(this.View.Model.GetValue("F_QEBI_Fqtyed", row).ToString());
                decimal Fqty = Convert.ToDecimal(this.View.Model.GetValue("F_QEBI_Fqty", row).ToString());
                if ((aaa.Equals("D") && qtyed > 0) || (aaa.Equals("A") && Fqty > 0))
                {
                    DynamicObjectCollection dyc = this.Model.DataObject["F_QEBI_Entity"] as DynamicObjectCollection;
                    foreach (var item in dyc)
                    {
                        if (Convert.ToInt32(item["seq"]) - 1 == row)
                        {
                            item["F_QEBI_Combo"] = 'B';
                        }
                    }
                    this.View.UpdateView("F_QEBI_Combo");

                    string message = aaa.Equals("D") ? "源单明细已存在下游单据，不允许删除！" : "源单明细行数据的变更状态不允许为创建状态！";
                    this.View.ShowErrMessage(message);
                }
                if (aaa.Equals("A") && Fqty > 0)
                {
                    var field = (ComboField)this.View.BillBusinessInfo.GetField("F_QEBI_Combo");
                    DynamicObjectCollection dyc = this.Model.DataObject["F_QEBI_Entity"] as DynamicObjectCollection;
                    foreach (var item in dyc)
                    {
                        if (Convert.ToInt32(item["seq"]) - 1 == row)
                        {
                            item["F_QEBI_Combo"] = 'B';
                        }
                    }
                    this.View.UpdateView("F_QEBI_Combo");
                    this.View.UpdateView();
                    this.View.ShowErrMessage("源单明细数据，不允许新增！");
                }


            }
            if (e.Field.FieldName.EqualsIgnoreCase("F_QEBI_Fqty1"))
            {
                Decimal Oldqty = Convert.ToDecimal(e.OldValue);
                Decimal qty = Convert.ToDecimal(e.NewValue);
                Decimal qtyed = Convert.ToDecimal(this.View.Model.GetValue("F_QEBI_FQTYED", row));
                if (Decimal.Subtract(qty, qtyed) < 0)
                {
                    //this.View.Model.SetValue("F_QEBI_FQTY1", Oldqty, row);


                    DynamicObjectCollection dyc = this.Model.DataObject["F_QEBI_Entity"] as DynamicObjectCollection;
                    foreach (var item in dyc)
                    {
                        if (Convert.ToInt32(item["seq"]) - 1 == row)
                        {
                            item["F_QEBI_FQTY1"] = Oldqty;
                        }
                    }

                    this.View.UpdateView("F_QEBI_FQTY1");
                    this.View.ShowErrMessage("数量不允许小于源单明细下推关联数量！");
                }
            }

        }

        /// <summary>
        /// 删除明细行前
        /// </summary>
        /// <param name="e"></param>
        public override void BeforeDeleteRow(BeforeDeleteRowEventArgs e)
        {
            base.BeforeDeleteRow(e);
            if (e.EntityKey.EqualsIgnoreCase("F_QEBI_Entity"))
            {
                //获取删除明细行的已用数量字段
                int qty = Convert.ToInt32(this.View.Model.GetValue("F_QEBI_FQTYED", e.Row));
                if (qty > 0)
                {
                    this.View.ShowErrMessage("源单明细已有下游单据，禁止删除分录行！");
                    return;
                }

            }
        }

        /// <summary>
        /// 删除明细行后数据反写表头删除明细id字段
        /// </summary>
        /// <param name="e"></param>
        public override void AfterDeleteRow(AfterDeleteRowEventArgs e)
        {
            base.AfterDeleteRow(e);
            if (e.EntityKey.EqualsIgnoreCase("F_QEBI_Entity"))
            {
                //获取源分录id
                string srcentryid = e.DataEntity["F_QEBI_srcentryid"].ToString();
                //获取表头删除明细id
                string delid = this.View.Model.GetValue("F_QEBI_delid").ToString();
                //为空，直接添加，否则拼接
                if (string.IsNullOrWhiteSpace(delid))
                {
                    this.View.Model.SetValue("F_QEBI_delid", srcentryid + ";");
                }
                else
                {
                    this.View.Model.SetValue("F_QEBI_delid", delid + srcentryid + ";");
                }

            }
        }

        /// <summary>
        /// 修改明细
        /// </summary>
        /// <param name="e"></param>
        public override void BeforeUpdateValue(BeforeUpdateValueEventArgs e)
        {
            base.BeforeUpdateValue(e);
            if (e.Key.EqualsIgnoreCase("F_QEBI_Fqty"))
            {
                int qty = Convert.ToInt32(e.Value);
                int QTYED = Convert.ToInt32(this.View.Model.GetValue("F_QEBI_FQTYED", e.Row));
                if (qty < QTYED)
                {
                    this.View.ShowErrMessage("当前用户录入数量不能小于已用数量！");
                    return;
                }
            }
        }

    }
}
