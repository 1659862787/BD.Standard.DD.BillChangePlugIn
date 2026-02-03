using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace BD.Standard.DD.BillChangePlugIns3s6.Inquiry
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("采购询价单保存操作插件")]
    public class PurInquiryOperationSave : AbstractOperationServicePlugIn
    {


        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            IOperationResult operationResult = new OperationResult();

            try
            {
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    string fid = entity[0].ToString();

                    if (entity["F_UJED_rule"].ToString().Equals("导入"))
                    {
                        //获取当前单据源单的所有下游数据
                        string srcBillno = entity["F_UJED_srcBillno"].ToString();
                        string srcid = entity["F_UJED_srcid"].ToString();



                        DynamicObjectCollection dyc = entity["FEntity"] as DynamicObjectCollection;
                        //string[] srcentryid ;
                        List<string> srcentryid=new List<string>();
                        foreach (DynamicObject item in dyc)
                        {
                            srcentryid.Add(item["F_UJED_srcentryid"].ToString());
                        }
                        string where=string.Join(",", srcentryid.ToArray());


                        string sql = $"select F_UJED_Supp,F_UJED_srcentryid,F_UJED_Amount1,F_UJED_Price1,F_UJED_KgQty,F_UJED_KgPriceTax,F_UJED_KgAmountTax from UJED_t_PurInquiryentry  where F_UJED_srcentryid in ({where}) ";

                        DynamicObjectCollection dy = DBUtils.ExecuteDynamicObject(this.Context, sql) as DynamicObjectCollection;

                        Dictionary<string, List<string>> prices1 = new Dictionary<string, List<string>>();
                        //数据处理
                        foreach (var item in dy)
                        {
                           
                            sql = $"select FSUPPLIERID from  T_BD_SUPPLIER_L  where FLOCALEID=2052 and  FNAME='{item["F_UJED_Supp"].ToString()}'";
                            long FSUPPLIERID = DBUtils.ExecuteScalar<long>(this.Context, sql, 0, null);

                            if (prices1.TryGetValue(item["F_UJED_srcentryid"].ToString(), out List<string> list))
                            {
                                if (Convert.ToDecimal(list[1]) > Convert.ToDecimal(item["F_UJED_Price1"]))
                                {


                                    prices1[item["F_UJED_srcentryid"].ToString()] = new List<string>()
                                    {
                                       FSUPPLIERID.ToString(),
                                       item["F_UJED_Price1"].ToString(),
                                       item["F_UJED_KgQty"].ToString(),
                                       item["F_UJED_KgPriceTax"].ToString(),
                                       item["F_UJED_KgAmountTax"].ToString(),
                                    };
                                }
                            }
                            else
                            {
                                prices1.Add(item["F_UJED_srcentryid"].ToString(), new List<string>()
                                    {
                                       FSUPPLIERID.ToString(),
                                       item["F_UJED_Price1"].ToString(),
                                       item["F_UJED_KgQty"].ToString(),
                                       item["F_UJED_KgPriceTax"].ToString(),
                                       item["F_UJED_KgAmountTax"].ToString()
                                    });
                            }
                        }

                        ////获取最大总额值供应商
                        //var maxEntry = suppiers.OrderByDescending(kv => kv.Value).First();
                        //// 获取最小总额值供应商
                        //var minEntry = suppiers.OrderBy(kv => kv.Value).First();


                        //sql = $"select FSUPPLIERID from  T_BD_SUPPLIER_L  where FLOCALEID=2052 and  FNAME='{minEntry.Key}'";
                        //long FSUPPLIERID = DBUtils.ExecuteScalar<long>(this.Context, sql, 0, null);
                        //if (FSUPPLIERID == 0)
                        //{
                        //    throw new KDException("", "总金额最小供应商名称:" + minEntry.Key + "，在供应商列表中不存在！");
                        //}
                        StringBuilder sqls = new StringBuilder();

                        foreach (var item in prices1)
                        {
                            sqls.AppendLine($"update T_PUR_ReqEntry set F_UJED_Price={item.Value[1]},F_QEBI_KgQty={item.Value[2]},F_QEBI_KgPriceTax={item.Value[3]},F_QEBI_KgAmountTax={item.Value[4]},FSuggestSupplierId={item.Value[0]} where fentryid='{item.Key}'");
                        }
                        int result = DBUtils.Execute(this.Context, sqls.ToString());
                        if (result == 0)
                        {
                            throw new KDException("", "采购申请单：" + srcBillno + "明细未找到匹配数据。确认是否存在或导入时是否修改了采购申请明细id字段");
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                throw new KDException("",ex.Message);
            }
            
        }


    
    }
}
