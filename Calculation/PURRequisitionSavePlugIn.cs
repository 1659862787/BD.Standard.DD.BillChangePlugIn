using Kingdee.BOS.App.Core;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.WebApi.FormService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.Standard.DD.BillChangePlugIns2606X01.Calculation
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("采购申请单操作插件")]
    public class PurRequisitionOperationPlugIn : AbstractOperationServicePlugIn
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
            IOperationResult operationResult = new OperationResult();
            try
            {
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    string fid = entity[0].ToString();
                    string Billno = entity["Billno"].ToString();
                    if (opera.Equals("Submit"))
                    {
                        DynamicObjectCollection dynamicObjectCollection1 = entity["ReqEntry"] as DynamicObjectCollection;
                        foreach (DynamicObject item in dynamicObjectCollection1)
                        {
                            DynamicObject FMaterialId = (DynamicObject)item["MaterialId"];

                            string Number = FMaterialId["Number"].ToString();
                            string Name = FMaterialId["Name"].ToString();
                            bool IsPurImage = Convert.ToBoolean(FMaterialId["F_ISPurImage"]);
                            if (IsPurImage)
                            {
                                object Image = FMaterialId["Image"];
                                if (Image == null)
                                {
                                    throw new Exception($"物料{Number}的图片附件为空，上传图片附件后重新提交");
                                    
                                    //operationResult.OperateResult.Add(new OperateResult()
                                    //{
                                    //    SuccessStatus = false,
                                    //    Name = "图片附件获取失败",
                                    //    Message = string.Format($"物料{Number}的图片附件为空，上传图片附件后重新提交"),
                                    //    MessageType = MessageType.Normal,
                                    //    PKValue = 0,
                                    //});
                                    //e.CancelOperation = true;
                                }
                                else
                                {
                                    Byte[] FIMAGE = (Byte[])Image;
                                    string SendByte = Convert.ToBase64String(FIMAGE);
                                    var imageName = Number + Name;
                                    JObject ImageObjData = new JObject();
                                    ImageObjData.Add("FileName", imageName + ".jpg");
                                    ImageObjData.Add("FormId", "PUR_Requisition");
                                    ImageObjData.Add("IsLast", "true");
                                    ImageObjData.Add("InterId", fid);
                                    ImageObjData.Add("BillNO", Billno);
                                    ImageObjData.Add("AliasFileName", imageName);
                                    ImageObjData.Add("SendByte", SendByte);
                                    var ImageDataJson = JsonConvert.SerializeObject(ImageObjData);
                                    var Upload = WebApiServiceCall.AttachmentUpload(this.Context, ImageDataJson);
                                    var json = JsonUtil.Serialize(Upload);
                                }
                            }
                        }
                    }
                    else if (opera.Equals("CancelAssign") || opera.Equals("UnAudit"))
                    {
                        DBUtils.Execute(Context, $"delete from T_BAS_ATTACHMENT where FINTERID={fid} and FBILLTYPE='PUR_Requisition'");
                    }
                }
                //this.OperationResult.MergeResult(operationResult);
               
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
