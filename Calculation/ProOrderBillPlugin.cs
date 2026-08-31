using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BD.Standard.DD.BillChangePlugIns.Calculation
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("生产订单表单更新最新bom版本")]
    public class ProOrderBillPlugin : AbstractDynamicFormPlugIn
    {

        public override void EntryBarItemClick(BarItemClickEventArgs e)
        {
            base.EntryBarItemClick(e);
            if (e.BarItemKey.Equals("F_BOMChange"))
            {
                Entity entity = this.Model.BusinessInfo.GetEntity("FTreeEntity");
                DynamicObjectCollection dynamicObjectCollection = this.View.Model.GetEntityDataObject(entity);
                if (dynamicObjectCollection.Count == 0) return;
                foreach (var item in dynamicObjectCollection)
                {
                    string MaterialId_id = item["MaterialId_id"].ToString();
                    string UseOrgId_Id = ((DynamicObject)item["MaterialId"])["UseOrgId_Id"].ToString();
                    List<DynamicObject> value = HighVersionBomDatas.HighVersionBomData(this.Context, Convert.ToInt64(MaterialId_id), Convert.ToInt64(UseOrgId_Id), 0);
                    if (value.Count > 0)
                    {
                        item["BomId"] = value[0];
                        item["BomId_id"] = value[0]["id"].ToString();
                    }
                }
                this.View.UpdateView("FTreeEntity");
            }
        }

    }
}
