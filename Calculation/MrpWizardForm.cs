using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.Standard.DD.BillChangePlugIns2606X01.Calculation
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("计划运算向导插件-获取BOM最高版本")]
    public class MrpWizardForm : AbstractDynamicFormPlugIn
    {
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            if (e.Key.EqualsIgnoreCase("F_HighVersionBom"))
            {
                Entity entity = this.Model.BusinessInfo.GetEntity("FEntity");
                Kingdee.BOS.Orm.DataEntity.DynamicObjectCollection dynamicObjectCollection = this.View.Model.GetEntityDataObject(entity);
                if (dynamicObjectCollection.Count == 0) return;
                foreach (var item in dynamicObjectCollection)
                {
                    string MaterialId_id = item["MaterialId_id"].ToString();
                    string UseOrgId_Id = ((DynamicObject)item["MaterialId"])["UseOrgId_Id"].ToString();
                    List<DynamicObject> value = HighVersionBomDatas.HighVersionBomData(this.Context,Convert.ToInt64(MaterialId_id), Convert.ToInt64(UseOrgId_Id), 0);

                    if (value.Count > 0)
                    {
                        item["BomId"] =value[0];
                        item["BomId_id"] =value[0]["id"].ToString();
                    }
                }
                this.View.UpdateView("FEntity");
            }
        }



    }
}
