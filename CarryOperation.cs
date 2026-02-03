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
    [Description("携带bom版本操作服务插件")]
    public class CarryOperation : AbstractOperationServicePlugIn
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
                    bool formid = Convert.ToBoolean(entity["F_QEBI_Checkbox"].ToString());
                    string sql = "";
                    if (!formid)
                    {
                        sql = "exec selectBomId '" + fid + "'";
                    }
                    else
                    {
                        sql = "exec selectBomIdX '" + fid + "'";
                    }
                    string billno=DBUtils.ExecuteScalar<string>(this.Context, sql,"",null);
                    if (!string.IsNullOrWhiteSpace(billno))
                    {
                        operationResult.OperateResult.Add(new OperateResult()
                        {
                            SuccessStatus = false,
                            Name = "携带bom版本失败",
                            Message = string.Format("文件编号:" + billno ),
                            MessageType = MessageType.Normal,
                            PKValue = 0,
                        });
                    }
                   // Logs.Log("提示", "D:log\\", "","fid:"+ fid + "\r\n formid:" + formid + "  \r\n sql:" + sql + ",\r\nbillno" + billno );

                }
                this.OperationResult.MergeResult(operationResult);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
