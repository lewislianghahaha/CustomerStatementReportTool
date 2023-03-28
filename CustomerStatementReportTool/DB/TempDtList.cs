using System;
using System.Collections;
using System.Data;

namespace CustomerStatementReportTool.DB
{
    //临时表
    public class TempDtList
    {
        /// <summary>
        /// 保存最终运算结果-'对账单'使用
        /// </summary>
        /// <returns></returns>
        public DataTable MakeExportDtTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 16; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //开始日期
                        dc.ColumnName = "SDT";
                        dc.DataType = Type.GetType("System.String"); 
                        break;
                    case 1: //结束日期
                        dc.ColumnName = "EDT";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2: //往来单位名称
                        dc.ColumnName = "CustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3: //单据日期
                        dc.ColumnName = "FDATE";
                        dc.DataType = Type.GetType("System.String"); 
                        break;
                    case 4: //摘要
                        dc.ColumnName = "Remark";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 5: //本期应收
                        dc.ColumnName = "ReceiveQTY";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 6: //本期收款
                        dc.ColumnName = "ReceiveCurrentQty";
                        dc.DataType = Type.GetType("System.String"); 
                        break;
                    case 7: //期末余额
                        dc.ColumnName = "EndBalance";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 8: //记录结束日期备注
                        dc.ColumnName = "REMARK1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 9://单据编号
                        dc.ColumnName = "FBILLNO";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 10:
                        dc.ColumnName = "LastEndBalance";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 11:
                        dc.ColumnName = "Month";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //单据日期-用于显示
                    case 12:
                        dc.ColumnName = "FDATE1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //FDtlId 用于表体排序
                    case 13:
                        dc.ColumnName = "FDtlId";
                        dc.DataType = Type.GetType("System.Int32"); 
                        break;
                    //fcustomercode(客户编码) 用于表头分组
                    case 14:
                        dc.ColumnName = "fcustomercode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //记录‘总期末余额’（以千位符进行分隔,在STI报表显示）
                    case 15:
                        dc.ColumnName = "TLastEndBalance";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 获取从SQL查询语句运行的记录集
        /// </summary>
        /// <returns></returns>
        public DataTable GetSearchTempDt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 12; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0:
                        dc.ColumnName = "往来单位名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1:
                        dc.ColumnName = "单据日期";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2:
                        dc.ColumnName = "摘要";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3:
                        dc.ColumnName = "期末余额";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 4:
                        dc.ColumnName = "本期应收";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 5:
                        dc.ColumnName = "本期实收";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 6:
                        dc.ColumnName = "原币本期冲销额";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                      //单据编号
                    case 7:
                        dc.ColumnName = "FBILLNO";
                        dc.DataType = Type.GetType("System.String");
                        break;
                     //记录‘期末余额’
                    case 8:
                        dc.ColumnName = "LastEndBalance";
                        dc.DataType = Type.GetType("System.Decimal"); 
                        break;
                    //月份
                    case 9:
                        dc.ColumnName = "Month";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //用于STI报表排序(自定义批量导出功能使用)
                    case 10:
                        dc.ColumnName = "FDtlId";
                        dc.DataType = Type.GetType("System.Int32"); 
                        break;
                    //往来单位编码
                    case 11:
                        dc.ColumnName = "往来单位编码";
                        dc.DataType=Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 获取‘客户’记录集
        /// </summary>
        /// <returns></returns>
        public DataTable GetSearchCustomerListTempDt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 2; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //客户编码
                        dc.ColumnName = "客户编码";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1: //客户名称
                        dc.ColumnName = "客户名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 获取‘客户名称’临时表
        /// </summary>
        /// <returns></returns>
        public DataTable GetCustomerNameListTempdt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 2; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //客户名称
                        dc.ColumnName = "客户名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1://客户编码
                        dc.ColumnName = "客户编码";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 保存最终运算结果(工业对账单(横向)报表使用)
        /// </summary>
        /// <returns></returns>
        public DataTable MakeProductExportDtTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 13; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //单据编号
                        dc.ColumnName = "Orderno";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1: //业务日期
                        dc.ColumnName = "Orderdt";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2: //物料名称
                        dc.ColumnName = "MaterialName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3: //品牌
                        dc.ColumnName = "BrandCode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 4: //规格型号
                        dc.ColumnName = "KuiCode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 5: //实发数量
                        dc.ColumnName = "Sendqty";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 6: //计价数量
                        dc.ColumnName = "Jiqty";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 7: //含税单价
                        dc.ColumnName = "Priceqty";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 8: //价税合计 
                        dc.ColumnName = "Amountqty";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 9: //客户ID
                        dc.ColumnName = "FcustId";
                        dc.DataType = Type.GetType("System.Int32"); 
                        break;
                    case 10: //开始日期
                        dc.ColumnName = "SDT";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 11: //结束日期
                        dc.ColumnName = "EDT";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 12: //客户名称
                        dc.ColumnName = "CustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 销售发货清单临时表
        /// </summary>
        /// <returns></returns>
        public DataTable MakeSalesOutListDtTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 21; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //客户ID
                        dc.ColumnName = "FCUSTID";
                        dc.DataType = Type.GetType("System.Int32"); 
                        break;
                    case 1: //应收单ID
                        dc.ColumnName = "FID";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    case 2: //终端客户
                        dc.ColumnName = "FDATAVALUE";
                        dc.DataType = Type.GetType("System.String"); 
                        break;
                    case 3: //收货单位
                        dc.ColumnName = "ReceiveFNAME";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 4: //收货单位1
                        dc.ColumnName = "ReceiveFNAME1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 5: //二级客户
                        dc.ColumnName = "TwoCustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 6: //三级客户
                        dc.ColumnName = "ThreeCustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 7: //摘要
                        dc.ColumnName = "Remark";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 8: //销售订单号
                        dc.ColumnName = "SaleOrderNo";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 9: //日期
                        dc.ColumnName = "FDATE";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 10: //U订货单号
                        dc.ColumnName = "UOrderNo";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 11: //单据编号
                        dc.ColumnName = "FBILLNO";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 12: //托运货场地址
                        dc.ColumnName = "FAddress";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 13: //产品名称
                        dc.ColumnName = "FMaterialCode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 14: //规格
                        dc.ColumnName = "KuiName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 15: //实发罐数
                        dc.ColumnName = "FQty";
                        dc.DataType = Type.GetType("System.Int32"); 
                        break;
                    case 16: //单价
                        dc.ColumnName = "FPrice";
                        dc.DataType = Type.GetType("System.Decimal"); 
                        break;
                    case 17: //合同金额
                        dc.ColumnName = "FAmount";
                        dc.DataType = Type.GetType("System.Double"); 
                        break;
                    case 18: //备注
                        dc.ColumnName = "FNote";
                        dc.DataType = Type.GetType("System.String"); 
                        break;
                    case 19: //促销备注
                        dc.ColumnName = "FSalesNote";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 20: //开票人
                        dc.ColumnName = "FCeateName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        #region '自定义批量导出'部份
        /// <summary>
        /// 导入模板-自定义批量导出功能使用
        /// </summary>
        /// <returns></returns>
        public DataTable ImportBatchCustomerExcelDt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 2; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //客户编码
                        dc.ColumnName = "客户编码";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1: //客户名称
                        dc.ColumnName = "客户名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 记录‘自定义批量导出’返回结果
        /// </summary>
        /// <returns></returns>
        public DataTable BatchGenerateResultDt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 7; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //客户编码
                        dc.ColumnName = "客户编码";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1: //客户名称
                        dc.ColumnName = "客户名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2: //开始执行时间
                        dc.ColumnName = "开始执行时间";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3: //结束执行时间
                        dc.ColumnName = "结束执行时间";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 4: //执行结果
                        dc.ColumnName = "执行结果";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 5: //导出单据类型-(0)'对帐单' (1)‘销售出库清单’
                        dc.ColumnName = "导出单据类型";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 6: //是否成功标记(0:是 1:否)
                        dc.ColumnName = "remarkid";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 保存最终运算结果-自定义批量导出-'对账单'(二级客户对账单) 以及 ‘签收确定单’(中转)使用
        /// 注:1) STI报表排序方式=>Frowid与Fsortid结合在表头分组使用，而FDtlid在对明细行排序使用
        ///    2) Frowid记录需要打印多少份，从0开始
        /// </summary>
        /// <returns></returns>
        public DataTable BatchMakeExportDtTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 19; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //开始日期
                        dc.ColumnName = "SDT";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1: //结束日期
                        dc.ColumnName = "EDT";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2: //往来单位名称
                        dc.ColumnName = "CustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3: //单据日期
                        dc.ColumnName = "FDATE";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 4: //摘要
                        dc.ColumnName = "Remark";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 5: //本期应收
                        dc.ColumnName = "ReceiveQTY";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 6: //本期收款
                        dc.ColumnName = "ReceiveCurrentQty";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 7: //期末余额
                        dc.ColumnName = "EndBalance";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 8: //记录结束日期备注
                        dc.ColumnName = "REMARK1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 9://单据编号
                        dc.ColumnName = "FBILLNO";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 10://记录最后一行‘期末余额’
                        dc.ColumnName = "LastEndBalance";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 11://月份
                        dc.ColumnName = "Month";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //单据日期-用于显示
                    case 12:
                        dc.ColumnName = "FDATE1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //作用:对相同客户的区分显示(作用:针对相同客户打印多次时)
                    case 13:
                        dc.ColumnName = "FRowId";
                        dc.DataType = Type.GetType("System.String"); 
                        break;
                    //作用:批量打印时,作用:显示出来的打印排列顺序要与前面导入的DT一致 STI表头排序ID;注:以A开始,若ID值小于10 即加0,如:A01(用于代替‘客户名称’放到报表里进行分组排序)
                    case 14:
                        dc.ColumnName = "FSortId";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //用于STI报表明细行排序(自定义批量导出功能使用)
                    case 15:
                        dc.ColumnName = "FDtlId";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    //客户开票名称-二级客户对账单.核算项目名称使用
                    case 16:
                        dc.ColumnName = "InvoiceName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //客户编码
                    case 17:
                        dc.ColumnName = "customercode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //记录‘总期末余额’（以千位符进行分隔,在STI报表显示）
                    case 18:
                        dc.ColumnName = "TLastEndBalance";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 保存最终运算结果-自定义批量导出-'销售发货清单'使用
        /// 注:STI报表排序方式=>用FID,frowid分组
        /// </summary>
        /// <returns></returns>
        public DataTable BatchMakeSalesOutListDtTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 22; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //客户ID
                        dc.ColumnName = "FCUSTID";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    case 1: //应收单ID
                        dc.ColumnName = "FID";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    case 2: //终端客户
                        dc.ColumnName = "FDATAVALUE";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3: //收货单位
                        dc.ColumnName = "ReceiveFNAME";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 4: //收货单位1
                        dc.ColumnName = "ReceiveFNAME1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 5: //二级客户
                        dc.ColumnName = "TwoCustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 6: //三级客户
                        dc.ColumnName = "ThreeCustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 7: //摘要
                        dc.ColumnName = "Remark";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 8: //销售订单号
                        dc.ColumnName = "SaleOrderNo";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 9: //日期
                        dc.ColumnName = "FDATE";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 10: //U订货单号
                        dc.ColumnName = "UOrderNo";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 11: //单据编号
                        dc.ColumnName = "FBILLNO";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 12: //托运货场地址
                        dc.ColumnName = "FAddress";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 13: //产品名称
                        dc.ColumnName = "FMaterialCode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 14: //规格
                        dc.ColumnName = "KuiName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 15: //实发罐数
                        dc.ColumnName = "FQty";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    case 16: //单价
                        dc.ColumnName = "FPrice";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 17: //合同金额
                        dc.ColumnName = "FAmount";
                        dc.DataType = Type.GetType("System.Double");
                        break;
                    case 18: //备注
                        dc.ColumnName = "FNote";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 19: //促销备注
                        dc.ColumnName = "FSalesNote";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 20: //开票人
                        dc.ColumnName = "FCeateName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 21://作用:对相同客户的区分显示(作用:针对相同客户打印多次时)
                        dc.ColumnName = "FRowId";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 获取前端导入的客户列表-‘自定义批量导出’功能使用
        /// </summary>
        /// <returns></returns>
        public DataTable SearchBatchCustomerDt()
        {
            var dt = new DataTable();
            for (var i = 0; i < 5; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0:  //FCUSTID
                        dc.ColumnName = "FCUSTID";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    case 1: //客户编码
                        dc.ColumnName = "客户编码";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2: //客户名称
                        dc.ColumnName = "客户名称";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3: //客户开票名称
                        dc.ColumnName = "客户开票名称";
                        dc.DataType=Type.GetType("System.String");
                        break;
                    case 4: //收货天数-‘签收确认单’使用
                        dc.ColumnName = "收货天数";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        /// <summary>
        /// 保存最终运算结果-自定义批量导出-'签收确认单'使用
        /// </summary>
        /// <returns></returns>
        public DataTable BatchMakeConfirmDtTemp()
        {
            var dt = new DataTable();
            for (var i = 0; i < 20; i++)
            {
                var dc = new DataColumn();
                switch (i)
                {
                    case 0: //开始日期
                        dc.ColumnName = "SDT";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 1: //结束日期
                        dc.ColumnName = "EDT";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 2: //往来单位名称
                        dc.ColumnName = "CustomerName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 3: //单据日期
                        dc.ColumnName = "FDATE";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 4: //摘要
                        dc.ColumnName = "Remark";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 5: //本期应收
                        dc.ColumnName = "ReceiveQTY";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 6: //本期收款
                        dc.ColumnName = "ReceiveCurrentQty";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 7: //期末余额
                        dc.ColumnName = "EndBalance";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 8: //记录结束日期备注
                        dc.ColumnName = "REMARK1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 9://单据编号
                        dc.ColumnName = "FBILLNO";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    case 10://记录最后一行‘期末余额’
                        dc.ColumnName = "LastEndBalance";
                        dc.DataType = Type.GetType("System.Decimal");
                        break;
                    case 11://月份
                        dc.ColumnName = "Month";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //单据日期-用于显示
                    case 12:
                        dc.ColumnName = "FDATE1";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //作用:对相同客户的区分显示(作用:针对相同客户打印多次时)
                    case 13:
                        dc.ColumnName = "FRowId";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //作用:批量打印时,作用:显示出来的打印排列顺序要与前面导入的DT一致 STI表头排序ID;注:以A开始,若ID值小于10 即加0,如:A01(用于代替‘客户名称’放到报表里进行分组排序)
                    case 14:
                        dc.ColumnName = "FSortId";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //用于STI报表明细行排序(自定义批量导出功能使用)
                    case 15:
                        dc.ColumnName = "FDtlId";
                        dc.DataType = Type.GetType("System.Int32");
                        break;
                    //客户开票名称-二级客户对账单.核算项目名称使用
                    case 16:
                        dc.ColumnName = "InvoiceName";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //客户编码
                    case 17:
                        dc.ColumnName = "customercode";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //记录‘总期末余额’（以千位符进行分隔,在STI报表显示）
                    case 18:
                        dc.ColumnName = "TLastEndBalance";
                        dc.DataType = Type.GetType("System.String");
                        break;
                    //记录'签收日期'
                    case 19:
                        dc.ColumnName = "FConfirmDate";
                        dc.DataType = Type.GetType("System.String");
                        break;
                }
                dt.Columns.Add(dc);
            }
            return dt;
        }

        #endregion
    }
}
