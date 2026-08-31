using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace BD.Standard.DD.BillChangePlugIns.StartLocks
{
    public class LocksUtils
    {
        public static void Locks(string fentryid, Context context )
        {
            string sql = string.Format("select a.FMATERIALID,m.FNUMBER,FBASEQTY,FBASEMUSTQTY,FMustQty" +
                "   from  T_PRD_PPBOMENTRY a " +
                "left join T_PRD_PPBOMENTRY_c b on b.fentryid=a.fentryid " +
                "left join T_BD_MATERIAL m on m.FMATERIALID=a.FMATERIALID  " +
                "left join (select sum(FBASEQTY) FBASEQTY,FMATERIALID from  T_STK_INVENTORY where FSTOCKID={1} and FSTOCKSTATUSID=10000 group by FMATERIALID) I on a.FMATERIALID=I.FMATERIALID " +
                "where FMOEntryID={0} and FIssueType=4 ", fentryid, 100022);//100022,,,136627

            DynamicObjectCollection dys = DBUtils.ExecuteDynamicObject(context, sql);
            bool flag = false;
            StringBuilder sb = new StringBuilder();
            JArray jArray = new JArray();
            if (dys.Count == 0) return;
            foreach (var dy in dys)
            {
                if (Convert.ToDecimal(dy["FBASEQTY"]) - Convert.ToDecimal(dy["FBASEMUSTQTY"]) < 0)
                {
                    sb.AppendLine("物料编码：" + dy["FNUMBER"] + ",库存基本单位数量:" + dy["FBASEQTY"] + ",用料基本单位数量:" + dy["FBASEMUSTQTY"] + "，线边仓库存不足！");
                    flag = true;
                }

                JObject FBillEntry = new JObject()
                        {
                            new JProperty("FConvertType","A"),
                            new JProperty("FMATERIALID",new JObject(){new JProperty("FNumber", dy["fnumber"]),}),
                            new JProperty("FConvertQty",dy["FMustQty"]),
                            new JProperty("FSTOCKID",new JObject(){new JProperty("FNumber", "CK014"),}),
                            new JProperty("FStockStatus",new JObject(){new JProperty("FNumber", "KCZT01_SYS"),}),
                        };
                JObject FBillEntry1 = new JObject()
                        {
                            new JProperty("FConvertType","B"),
                            new JProperty("FMATERIALID",new JObject(){new JProperty("FNumber", dy["fnumber"]),}),
                            new JProperty("FConvertQty",dy["FMustQty"]),
                            new JProperty("FSTOCKID",new JObject(){new JProperty("FNumber", "CK014"),}),
                            new JProperty("FStockStatus",new JObject(){new JProperty("FNumber", "KCZT03_SYS"),}),
                        };
                jArray.Add(FBillEntry);
                jArray.Add(FBillEntry1);

            }
            if (flag)
            {
                throw new KDException("ex", sb.ToString());
            
            }
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

            Object MessageReturned = WebApiServiceCall.Save(context, "STK_StockConvert", json.ToString());

            JObject resultjson = JObject.FromObject(MessageReturned);
            if (resultjson["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
            {
                string FID = resultjson["Result"]["ResponseStatus"]["SuccessEntitys"][0].First.First.ToString();
                //开工后记录锁库状态
                DBUtils.Execute(context, $"update T_PRD_MOENTRY set F_UJED_CheckBox=1,F_UJED_StockStatusId='{FID}' where fentryid={fentryid}");

            }
            else
            {
                throw new KDException("ex", resultjson.ToString());
            
            }
        }

        public static void UnLocks(string fentryid,string StockStatusId, Context context)
        {
            JObject json = new JObject()
            {
                new JProperty("Ids",StockStatusId),
            };

            string MessageReturned = JsonConvert.SerializeObject(WebApiServiceCall.UnAudit(context, "STK_StockConvert", json.ToString()));
            if (JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
            {
                MessageReturned = JsonConvert.SerializeObject(WebApiServiceCall.Delete(context, "STK_StockConvert", json.ToString()));
                if (JObject.Parse(MessageReturned)["Result"]["ResponseStatus"]["IsSuccess"].ToString().Equals("True"))
                {
                    //开工后记录锁库状态
                    DBUtils.Execute(context, $"update T_PRD_MOENTRY set F_UJED_CheckBox=0,F_UJED_StockStatusId='' where fentryid={fentryid}");
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
