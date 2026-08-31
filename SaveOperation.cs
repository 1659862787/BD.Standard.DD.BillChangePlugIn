using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace BD.Standard.DD.BillChangePlugIns3s6
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("项目生产订货变更单保存操作服务插件")]
    public class SaveOperation : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            List<Kingdee.BOS.Core.Metadata.FieldElement.Field> fields = this.BusinessInfo.GetFieldList();
            foreach (var item in fields)
            {
                e.FieldKeys.Add(item.Key);
            }
        }


        /// <summary>
        /// 审核按钮集合方法
        /// </summary>
        /// <param name="e"></param>
        /// //
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            try
            {
                IOperationResult operationResult = new OperationResult();
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    string fid = entity[0].ToString();
                    string srcid = entity["F_QEBI_SRCID"].ToString();
                    //获取源单数据
                    FormMetadata ExpMeta = MetaDataServiceHelper.Load(this.Context, "kd2503c87e15e4cf88ff9147dd356080f", true) as FormMetadata;
                    DynamicObject Expobj = BusinessDataServiceHelper.LoadSingle(this.Context, srcid, ExpMeta.BusinessInfo.GetDynamicObjectType());
                    DynamicObjectCollection srcdyc = Expobj["F_QEBI_Entity"] as DynamicObjectCollection;

                    DynamicObjectCollection dyc = entity["F_QEBI_Entity"] as DynamicObjectCollection;
               
                    foreach (DynamicObject item in dyc) 
                    {
                        DynamicObjectCollection link = item["F_QEBI_Entity_Link"] as DynamicObjectCollection;
                        //明细id
                        if (link == null || link.Count() == 0) continue;
                        string Sid = link[0]["Sid"].ToString();
                        string matreial = string.Empty;
                        string combo = item["F_QEBI_Combo"].ToString();
                        DynamicObject mater = (DynamicObject)item["F_QEBI_material"];
                        //修改数量
                        Decimal Fqty1 = Convert.ToDecimal(item["F_QEBI_Fqty1"]);
                        //已关联数量
                        Decimal qtyed = 0;
                        foreach (DynamicObject srcdy in srcdyc)
                        {

                            if (srcdy["id"].ToString().Equals(Sid))
                            {

                                qtyed = Convert.ToDecimal(srcdy["F_QEBI_POQty"]);
                                break;
                            }
                        }
                        //Logs.Log("提示", "D:log\\", "", ++i+"次，原明细id:"+ Sid+ " , aaa"+ aaa+ "   qtyed:"+ aaa1+ ",aaa2" + aaa2 + "   ,Fqty1"+ Fqty1);
                        if (mater != null)
                        {
                            matreial = mater["number"].ToString();
                        }
                        if (combo.Equals("A") && !string.IsNullOrWhiteSpace(Sid))
                        {
                            throw new Exception("明细物料编码:" + matreial + " 关联源单数据，变更状态不允许新增" + qtyed);
                        }
                        if (combo.Equals("D") && qtyed > 0)
                        {
                            throw new Exception("明细物料编码:" + matreial + " 源单数据已存在关联数量，变更状态不允许删除" + qtyed);
                        }
                        if (decimal.Subtract(Fqty1, qtyed) < 0)
                        {
                            throw new Exception("明细物料编码:" + matreial + " 的变更数量不允许小于已生产数量" + qtyed);
                        }
                        
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
