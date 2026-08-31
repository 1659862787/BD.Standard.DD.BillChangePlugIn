using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace BD.Standard.DD.BillChangePlugIns
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("调拨单保存与审核操作服务插件")]
    public class SaveX : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            List<Field> field = this.BusinessInfo.GetFieldList();
            foreach (Field item in field)
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
            string opera=this.FormOperation.Operation;
            try
            {
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    string fid = entity[0].ToString();
                    //string username = this.Context.UserName;
                    string userNumber = string.Empty;
                    //员工新增单选框，控制反审核权限
                    DynamicObjectCollection dycn = DBUtils.ExecuteDynamicObject(this.Context, "select F_ZOCM_xiangmudiaobodan from T_SEC_USER a inner join T_HR_EMPINFO b on a.flinkobject=b.fpersonid where a.fuserid='" + this.Context.UserId.ToString() + "'");
                    if (dycn.Count > 0) userNumber = dycn[0][0].ToString();

                    DataSet ds =DBUtils.ExecuteDataSet(this.Context, "exec  STK_STKTRANSFERIN_save '" + fid+"'");
                    int xm = Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[0].ToString());
                    int fxm = Convert.ToInt32(ds.Tables[1].Rows[0].ItemArray[0].ToString());
                    if (xm > 0 && fxm > 0 && opera.Equals("Save"))
                    {
                        throw new Exception("【操作插件控制】明细调入仓不允许同时存在项目仓与非项目仓数据，请修改后重新保存!");
                    }
                    else if( fxm > 0 && !userNumber.Equals("1") && opera.Equals("UnAudit"))
                    {
                        throw new Exception("【操作插件控制】调入仓为项目仓的数据已推送，不允许反审核!");
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
