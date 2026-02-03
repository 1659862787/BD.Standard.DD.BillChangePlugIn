using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BD.Standard.DD.BillChangePlugIns3s6.StartLocks
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("生产出库判断是否锁库")]
    public  class ProStockAuditOperation:AbstractOperationServicePlugIn
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

        public override void BeginOperationTransaction(BeginOperationTransactionArgs e)
        {
            base.BeginOperationTransaction(e);
            string opera = this.FormOperation.Operation;
            try
            {
                foreach (var entity in e.DataEntitys)
                {
                    string fid = entity[0].ToString();

                    if (opera.Equals("Audit"))
                    {
                        Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
                        JArray jArray = new JArray();

                        //获取明细实体，拿到源单entryid,判断是否锁库。
                        //是：查询询用料清单，查看订单与入库数量比例，解库
                        DynamicObjectCollection dynamicObjectCollection = entity["Entity"] as DynamicObjectCollection;
                        foreach (var dynamicObject in dynamicObjectCollection)
                        {
                            //源单行内码
                            long MoEntryId = Convert.ToInt64(dynamicObject["MoEntryId"]);
                            if (MoEntryId == 0 || Convert.ToBoolean(dynamicObject["F_UJED_CheckBox"])) return;
                            //判断是否执行过锁库/反写数量否为零(扣减-解库)
                            string sql = $"select F_UJED_CheckBox,FQTY,FNOSTOCKINQTY from  T_PRD_MOENTRY m inner join T_PRD_MOENTRY_q mq on m.fentryid=mq.FENTRYID where m.FENTRYID={MoEntryId}";
                            DynamicObjectCollection dynamicObjects = DBUtils.ExecuteDynamicObject(this.Context, sql);
                            if (dynamicObjects.Count == 0) return;
                            //已进行开工锁库
                            if (Convert.ToInt32(dynamicObjects[0]["F_UJED_CheckBox"]) == 1)
                            {
                                //入库需求数量除以订单数量
                                decimal div = Decimal.Round(decimal.Divide(Convert.ToDecimal(dynamicObject["REALQTY"]), Convert.ToDecimal(dynamicObjects[0]["FQTY"])), 6);
                                //预计可入库需求数量减库需求数量
                                decimal inqty = Decimal.Round(decimal.Subtract(Convert.ToDecimal(dynamicObjects[0]["FNOSTOCKINQTY"]), Convert.ToDecimal(dynamicObject["REALQTY"])), 6);

                                DynamicObjectCollection dys = DBUtils.ExecuteDynamicObject(this.Context, $"select a.fentryid,FNOPICKEDQTY,FMustQty,F_UJED_InQty,M.FNUMBER from  T_PRD_PPBOMENTRY a  left join T_PRD_PPBOMENTRY_c b on b.fentryid=a.fentryid  left join T_PRD_PPBOMENTRY_Q c on c.fentryid=a.fentryid  left join T_BD_MATERIAL m on m.FMATERIALID=a.FMATERIALID  where FMOEntryID={MoEntryId} and FIssueType=4 ");
                                if (dys.Count == 0) return;
                                foreach (var dy in dys)
                                {
                                    decimal FConvertQty = 0;
                                    if (inqty == 0)
                                    {
                                        FConvertQty = Convert.ToDecimal(dy["FNOPICKEDQTY"]);
                                        dic.Add(dy["fentryid"].ToString(), Convert.ToDecimal(dy["FMustQty"]));
                                    }
                                    else
                                    {
                                        FConvertQty = Decimal.Round(decimal.Multiply(Convert.ToDecimal(dy["FMustQty"]), div), 6);
                                        dic.Add(dy["fentryid"].ToString(), FConvertQty + Convert.ToDecimal(dy["F_UJED_InQty"]));
                                    }

                                    JObject FBillEntry = new JObject()
                                    {
                                        new JProperty("FConvertType","A"),
                                        new JProperty("FMATERIALID",new JObject(){new JProperty("FNumber", dy["fnumber"]),}),
                                        new JProperty("FConvertQty",FConvertQty),
                                        new JProperty("FSTOCKID",new JObject(){new JProperty("FNumber", "CK014"),}),
                                        new JProperty("FStockStatus",new JObject(){new JProperty("FNumber", "KCZT03_SYS"),}),
                                    };
                                            JObject FBillEntry1 = new JObject()
                                    {
                                        new JProperty("FConvertType","B"),
                                        new JProperty("FMATERIALID",new JObject(){new JProperty("FNumber", dy["fnumber"]),}),
                                        new JProperty("FConvertQty",FConvertQty),
                                        new JProperty("FSTOCKID",new JObject(){new JProperty("FNumber", "CK014"),}),
                                        new JProperty("FStockStatus",new JObject(){new JProperty("FNumber", "KCZT01_SYS"),}),
                                    };
                                    jArray.Add(FBillEntry);
                                    jArray.Add(FBillEntry1);

                                }
                            }
                            #region 库存状态转换

                            JObject Model = new JObject()
                            {
                                new JProperty("FEntity",jArray),
                            };
                            JObject json = new JObject()
                            {
                                new JProperty("IsAutoAdjustField","true"),
                                new JProperty("IsDraftWhenSaveFail","true"),
                                new JProperty("IsAutoSubmitAndAudit","true"),
                                new JProperty("IsDeleteEntry","true"),
                                new JProperty("Model",Model),
                            };

                            Object MessageReturned = WebApiServiceCall.Save(this.Context, "STK_StockConvert", json.ToString());

                            JObject resultjson = JObject.FromObject(MessageReturned);
                            if (resultjson["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
                            {
                                string FID = resultjson["Result"]["ResponseStatus"]["SuccessEntitys"][0].First.First.ToString();
                                //开工后记录锁库状态
                                DBUtils.Execute(this.Context, $"update T_PRD_Instockentry set F_UJED_CheckBox=1,F_UJED_StockStatusId='{FID}' where fid={fid}");
                                //用料反写解库数量
                                StringBuilder sb = new StringBuilder();
                                foreach (var d in dic)
                                {
                                    sb.AppendLine($"update T_PRD_PPBOMENTRY set F_UJED_InQty={d.Value} where fentryid={d.Key}");
                                }
                                DBUtils.Execute(this.Context, sb.ToString());

                            }
                            else
                            {
                                throw new KDException("ex", resultjson.ToString());
                            }
                            #endregion 库存状态转换
                        }
                    }
                    else if (opera.Equals("UnAudit"))
                    {
                        Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
                        JArray jArray = new JArray();

                        //获取明细实体，拿到源单entryid,判断是否锁库。
                        //是：查询询用料清单，查看订单与入库数量比例，解库
                        DynamicObjectCollection dynamicObjectCollection = entity["Entity"] as DynamicObjectCollection;
                        foreach (var dynamicObject in dynamicObjectCollection)
                        {
                            //源单行内码
                            string fentryid = dynamicObject["id"].ToString();
                            long MoEntryId = Convert.ToInt64(dynamicObject["MoEntryId"]);
                            if (MoEntryId == 0 || !Convert.ToBoolean(dynamicObject["F_UJED_CheckBox"])) return;
                            string StockStatusId = Convert.ToString(dynamicObject["F_UJED_StockStatusId"]);
                            JObject json = new JObject()
                               {
                                new JProperty("Ids",StockStatusId),
                            };

                            string MessageReturned = JsonConvert.SerializeObject(WebApiServiceCall.UnAudit(this.Context, "STK_StockConvert", json.ToString()));
                            if (JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
                            {
                                MessageReturned = JsonConvert.SerializeObject(WebApiServiceCall.Delete(this.Context, "STK_StockConvert", json.ToString()));
                                if (JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
                                {
                                   
                                    
                                    decimal FQTY = DBUtils.ExecuteScalar<decimal>(this.Context, $"select FQTY from  T_PRD_MOENTRY  where FENTRYID={MoEntryId}", 0, null);
                                    decimal div = Decimal.Round(decimal.Divide(Convert.ToDecimal(dynamicObject["REALQTY"]), FQTY), 6);
                                    DynamicObjectCollection dynamicObjects = DBUtils.ExecuteDynamicObject(this.Context, $"select a.fentryid,FNOPICKEDQTY,FMustQty,F_UJED_InQty,M.FNUMBER from  T_PRD_PPBOMENTRY a  left join T_PRD_PPBOMENTRY_c b on b.fentryid=a.fentryid left join T_PRD_PPBOMENTRY_Q c on c.fentryid=a.fentryid  left join T_BD_MATERIAL m on m.FMATERIALID=a.FMATERIALID  where FMOEntryID={MoEntryId} and FIssueType=4 ");
                                    StringBuilder sb = new StringBuilder();
                                    foreach (var dy in dynamicObjects)
                                    {
                                        decimal FConvertQty = Decimal.Round(decimal.Multiply(Convert.ToDecimal(dy["FMustQty"]), div), 6);
                                        sb.AppendLine($" update T_PRD_PPBOMENTRY set F_UJED_InQty={Convert.ToDecimal(dy["F_UJED_InQty"]) - FConvertQty} where fentryid={dy["fentryid"]}");
                                    }

                                    sb.AppendLine($"update T_PRD_Instockentry set F_UJED_CheckBox=0,F_UJED_StockStatusId='' where fentryid={fentryid}");

                                    DBUtils.Execute(this.Context,sb.ToString());
                                }
                                else
                                {
                                    throw new KDException("ex", MessageReturned.ToString());
                                }
                            }
                            else
                            {
                                throw new KDException("ex", MessageReturned.ToString());
                            }






                           


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new KDException("ex", ex.ToString());
            }
            
        }
    }
}
