using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BD.Standard.DD.BillChangePlugIns
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("生产项目审核操作服务插件")]
    public class AduitOperation : AbstractOperationServicePlugIn
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

                    #region 表头实体赋值
                    Expobj["F_QEBI_Place"] = entity["F_QEBI_Place"].ToString();
                    Expobj["F_QEBI_fax"] = entity["F_QEBI_fax"].ToString();
                    Expobj["F_QEBI_TEL"] = entity["F_QEBI_TEL"].ToString();
                    Expobj["F_QEBI_address"] = entity["F_QEBI_address"].ToString();
                    Expobj["F_QEBI_changeReason"]= entity["F_QEBI_changeReason"].ToString();
                    Expobj["F_QEBI_version"] = entity["billno"].ToString();
                    //Expobj["F_QEBI_Person"] = entity["F_QEBI_Person"].ToString();
                    //Expobj["F_QEBI_salesman"] = entity["F_QEBI_salesman"].ToString();

                    #endregion 表头实体赋值

                    DynamicObjectCollection dyc =entity["F_QEBI_Entity"] as DynamicObjectCollection;
                    foreach (DynamicObject item in dyc)
                    {
                        string combo=item["F_QEBI_Combo"].ToString();
                        switch (combo)
                        {
                            //新增明细
                            case "A":
                                if (item["F_QEBI_material"]!=null)
                                {
                                    DynamicObject srcdy = srcdyc.DynamicCollectionItemPropertyType.CreateInstance() as DynamicObject;
                                    srcdy["Seq"] = srcdyc.Count + 1;
                                    srcdy["F_QEBI_material"] = item["F_QEBI_material"];
                                    long materialId = Convert.ToInt64(item["F_QEBI_material_Id"]);
                                    srcdy["F_QEBI_material_Id"] = materialId;
                                    srcdy["F_QEBI_Fqty1"] = item["F_QEBI_Fqty1"];
                                    srcdy["F_QEBI_remark"] = item["F_QEBI_remark"];
                                    srcdy["F_QEBI_Datetime"] = item["F_QEBI_Datetime"];
                                    srcdy["F_QEBI_PipelineNo"] = item["F_QEBI_PipelineNo"];
                                    srcdy["F_QEBI_ProjectSequenceNo"] = item["F_QEBI_ProjectSequenceNo"];
                                    srcdyc.Add(srcdy);
                                }
                                break;
                            //修改明细
                            case "B":
                                long srcentryid = Convert.ToInt64(item["F_QEBI_srcentryid"]);
                                foreach (DynamicObject srcdy in srcdyc)
                                {
                                    if(Convert.ToInt64(srcdy["id"])== srcentryid)
                                    {
                                        srcdy["F_QEBI_Fqty1"] = item["F_QEBI_Fqty1"];
                                        srcdy["F_QEBI_remark"] = item["F_QEBI_remark"];
                                        srcdy["F_QEBI_PipelineNo"] = item["F_QEBI_PipelineNo"];
                                        srcdy["F_QEBI_ProjectSequenceNo"] = item["F_QEBI_ProjectSequenceNo"];
                                        break;
                                    }
                                }
                                break;
                            //删除明细
                            case "D":
                                long srcentryid1 = Convert.ToInt64(item["F_QEBI_srcentryid"]);
                                foreach (DynamicObject srcdy in srcdyc)
                                {
                                    if (Convert.ToInt64(srcdy["id"]) == srcentryid1)
                                    {
                                        srcdyc.Remove(srcdy);
                                        BusinessDataServiceHelper.Save(this.Context, Expobj);
                                        break;
                                    }
                                }
                                break;

                        }


                    }
                    BusinessDataServiceHelper.Save(this.Context, Expobj);

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
