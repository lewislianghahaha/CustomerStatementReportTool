//SQL语句集合

namespace CustomerStatementReportTool.DB
{
    public class SqlList
    {
        //根据SQLID返回对应的SQL语句  
        private string _result;

        /// <summary>
        /// 获取客户列表信息
        /// </summary>
        /// <returns></returns>
        public string GetCustomerList()
        {
            _result = $@"
                            SELECT A.FNUMBER 客户编码,B.FNAME 客户名称 
                            FROM dbo.T_BD_CUSTOMER A
                            INNER JOIN dbo.T_BD_CUSTOMER_L B ON A.FCUSTID=B.FCUSTID AND B.FLOCALEID=2052
                            WHERE a.FDOCUMENTSTATUS='C'
                            AND A.FNUMBER NOT LIKE 'INT%'
                            ORDER BY a.FCUSTID
                         ";

            return _result;
        }

        /// <summary>
        /// 获取客户应收款明细表相关记录（对账单查询使用）
        /// </summary>
        /// <param name="edt">结束日期</param>
        /// <param name="customerlist">客户列表信息</param>
        /// <param name="sdt">开始日期</param>
        /// <returns></returns>
        public string GetFinialRecord(string sdt, string edt,string customerlist)
        {
            _result = $@"
                            BEGIN
	                            if OBJECT_ID('tmepdb..#temp_0')is not null
		                            drop table #temp_0

	                            --正常开始日期到结束日期范围内数据临时表
	                            create table #temp_0(FID int                          --单据内码
			                            ,FFORMID nvarchar(250)                        --单据类型
			                            ,FBILLNO nvarchar(250)                         --单据编号
			                            ,FDATE nvarchar(250)                           --单据日期
			                            ,FCONTACTUNITTYPE nvarchar(250)      --往来单位类型
			                            ,FCONTACTUNITID int                           --往来单位内码
			                            ,FCONTACTUNITNUMBER nvarchar(250) --往来单位编码
			                            ,FCONTACTUNITNAME nvarchar(250)     --往来单位名称
			                            ,FBUSINESSDEPTID int                          --销售部门内码
			                            ,FBUSINESSERID int                              --销售员内码
			                            ,FCURRENCYFORNAME nvarchar(250)     --本位币
			                            ,FCURRENCYNAME nvarchar(250)            --原币
			                            ,FBUSINESSDESP nvarchar(250)              --应收类型
			                            ,FAMOUNTFOR decimal(25,5)                  --本位币本期应收
			                            ,FHADIVAMOUNTFOR  decimal(25,5)        --本位币已开票金额
			                            ,FREALAMOUNTFOR decimal(25,5)           --本位币本期收款
			                            ,FOFFAMOUNTFOR decimal(25,5)             --本位币本期冲销额
			                            ,FLEFTAMOUNTFOR decimal(25,5)            --本位币期未余额
			                            ,FAMOUNT decimal(25,5)                         --原币本期应收
			                            ,FHADIVAMOUNT decimal(25,5)                --原币已开票金额
			                            ,FREALAMOUNT decimal(25,5)                  --原币本期收款
			                            ,FOFFAMOUNT decimal(25,5)                    --原币本期冲销额
			                            ,FLEFTAMOUNT decimal(25,5)                   --原币期未余额
			                            ,F_YTC_TEXT2 nvarchar(500))


	                            --创建#temp_0相关索引
	                            CREATE INDEX IDX_fbillno ON  #temp_0  (FBILLNO)
	                            CREATE INDEX IDX_FCONTACTUNITID ON  #temp_0  (FCONTACTUNITID)
	                            CREATE INDEX IDX_FBUSINESSDEPTID ON  #temp_0  (FBUSINESSDEPTID)
	                            CREATE INDEX IDX_FCONTACTUNITNUMBER ON #temp_0(FCONTACTUNITNUMBER)
	                            CREATE INDEX IDX_FFORMID ON #temp_0(FFORMID)

	                            INSERT into #temp_0 exec 应收款明细表按部门汇总_DIY '{sdt}','{edt}'

	                            IF OBJECT_ID('tempdb..#TEMP_X')is not null
	                                DROP table #TEMP_X

	                            SELECT * 
	                            INTO #TEMP_X
	                            FROM #temp_0 T1
                                WHERE t1.FCONTACTUNITNUMBER IN ({customerlist})   --@customer
	                            AND ISNULL(t1.FBUSINESSDESP,'') <> '冲销额'

	                            SELECT 
			                            t1.FFORMID 单据类型
			                            ,t1.FBILLNO 单据编号
			                            ,CONVERT(nvarchar(250),t1.FDATE,23) 单据日期
			                            ,t1.FCONTACTUNITNUMBER 往来单位编码
			                            ,t1.FCONTACTUNITNAME 往来单位名称
			                            ,t1.FCURRENCYNAME 原币
			                            ,t1.FBUSINESSDESP 应收类型
			                            ,SUM(isnull(t1.FAMOUNT,0)) 原币本期应收
			                            ,SUM(isnull(t1.FREALAMOUNT,0)) 原币本期收款
			                            ,SUM(isnull(t1.FOFFAMOUNT,0)) 原币本期冲销额
			                            ,SUM(isnull(t1.FLEFTAMOUNT,0)) 原币期未余额

			                            ,CASE when isnull(t21.F_YTC_TEXT2,'') <>'' and isnull(t1.FBUSINESSDESP,'') <> '冲销额' and isnull(t1.FBUSINESSDESP,'') <> '期初余额' then t21.F_YTC_TEXT2
				                                when isnull(t3.F_YTC_TEXT2,'')<>'' and isnull(t1.FBUSINESSDESP,'') <> '冲销额' and isnull(t1.FBUSINESSDESP,'') <> '期初余额' then t3.F_YTC_TEXT2
				                                when isnull(t4.F_YTC_REMARKS,'')<>'' and isnull(t1.FBUSINESSDESP,'') <> '冲销额' and isnull(t1.FBUSINESSDESP,'') <> '期初余额' then t4.F_YTC_REMARKS
				                                when isnull(t5.F_YTC_REMARKS,'')<>'' and isnull(t1.FBUSINESSDESP,'') <> '冲销额' and isnull(t1.FBUSINESSDESP,'') <> '期初余额' then t5.F_YTC_REMARKS
				                                when ISNULL(t1.FCONTACTUNITNAME,'') like '%本期合计' and isnull(t1.FBUSINESSDESP,'') <> '冲销额' and isnull(t1.FBUSINESSDESP,'') <> '期初余额' then '本期合计'
				                                when ISNULL(t1.FCONTACTUNITNUMBER,'') = '合计' then '累计合计'
				                                when isnull(t1.FBUSINESSDESP,'') = '期初余额' then '期初余额'
				                                when isnull(t1.FBUSINESSDESP,'') = '冲销额' then '冲销额' ELSE '0' END 摘要

	                            INTO #temp_1
	                            FROM #TEMP_X t1
                                LEFT join t_AR_receivable t21 on t1.FFORMID = 'AR_receivable' AND t1.FID = t21.FID     --t21.fbillno
	                            LEFT join T_AR_OtherRecAble t3 on t1.FFORMID = 'AR_OtherRecAble' AND t1.FID = t3.FID --t3.fbillno
	                            LEFT join T_AR_RECEIVEBILL t4 on t1.FFORMID = 'AR_RECEIVEBILL' AND t1.FID = t4.FID --t4.fbillno
	                            LEFT join T_AR_REFUNDBILL t5 on t1.FFORMID = 'AR_REFUNDBILL' AND t1.FID = t5.FID  --t5.fbillno

	                            group by t1.FFORMID,t1.FBILLNO,t1.FDATE,t1.FCONTACTUNITNUMBER,t1.FCONTACTUNITNAME
				                            ,t1.FCURRENCYNAME,t1.FBUSINESSDESP
				                            ,t21.F_YTC_TEXT2,t3.F_YTC_TEXT2,t4.F_YTC_REMARKS,t5.F_YTC_REMARKS
	                            --ORDER BY t1.FCONTACTUNITNAME,t1.FDATE

                                ----期末余额=期未余额+本期应收-本期收款-本期冲销额 

                                SELECT A.往来单位名称,A.单据日期,A.摘要
                                       ,A.原币期未余额 期末余额,A.原币本期应收 本期应收,A.原币本期收款 本期实收,A.原币本期冲销额
                                       ,A.单据编号,0.0 LastEndBalance,CONVERT(VARCHAR(10),DATEPART(MONTH,A.单据日期)) 月份
                                       ,0 FDtlId,A.往来单位编码
                                FROM #temp_1 A
                                WHERE (LEN(ISNULL(a.摘要,0))>0 AND A.摘要 <> '本期合计' )
                                --a.摘要 <> '本期合计' 
                                --AND A.原币本期收款=0
                                --a.往来单位名称='广州民福机电设备有限公司'
                                ORDER BY A.往来单位名称,A.单据日期

                            END
                        ";

            return _result;
        }

        /// <summary>
        /// 根据不同条件查询客户列表信息
        /// </summary>
        /// <param name="typeid">-1:全查找记录 0:按‘客户编码’查找 1:按'客户名称'查找</param>
        /// <param name="value">查询值</param>
        /// <returns></returns>
        public string SearchCustomerList(int typeid,string value)
        {
            //全查找记录
            if (typeid == -1)
            {
                _result = $@"
                            SELECT A.FNUMBER 客户编码,B.FNAME 客户名称 
                            FROM dbo.T_BD_CUSTOMER A
                            INNER JOIN dbo.T_BD_CUSTOMER_L B ON A.FCUSTID=B.FCUSTID AND B.FLOCALEID=2052
                            WHERE a.FDOCUMENTSTATUS='C'
                            AND A.FNUMBER NOT LIKE 'INT%'
                            ORDER BY a.FCUSTID
                         ";
            }
            //按‘客户编码’查找
            else if (typeid == 0)
            {
                _result = $@"
                            SELECT A.FNUMBER 客户编码, B.FNAME 客户名称
                            FROM dbo.T_BD_CUSTOMER A
                            INNER JOIN dbo.T_BD_CUSTOMER_L B ON A.FCUSTID = B.FCUSTID AND B.FLOCALEID = 2052
                            WHERE a.FDOCUMENTSTATUS = 'C'
                            AND A.FNUMBER NOT LIKE 'INT%'
                            AND A.FNUMBER LIKE '%{value}%'
                            ORDER BY a.FCUSTID
                        ";
            }
            //按'客户名称'查找
            else
            {
                _result = $@"
                            SELECT A.FNUMBER 客户编码, B.FNAME 客户名称
                            FROM dbo.T_BD_CUSTOMER A
                            INNER JOIN dbo.T_BD_CUSTOMER_L B ON A.FCUSTID = B.FCUSTID AND B.FLOCALEID = 2052
                            WHERE a.FDOCUMENTSTATUS = 'C'
                            AND A.FNUMBER NOT LIKE 'INT%'
                            AND B.FNAME LIKE '%{value}%'
                            ORDER BY a.FCUSTID
                        ";
            }

            return _result;
        }

        /// <summary>
        /// 根据客户记录查找相关应收单记录(销售单位为‘工业涂料事业部’)
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        /// <returns></returns>
        public string SearchProductCustomer(string sdt, string edt, string customerlist)
        {
            _result = $@"
                            SELECT  A.FCUSTOMERID
			                        ,A.FBILLNO 单据编号
			                        ,CONVERT(VARCHAR(10),A.FDATE,23) 业务日期
		                            ,D.FNAME 物料名称
			                        ,E.FDATAVALUE 品牌
			                        ,D.FSPECIFICATION 规格型号
			                        ,ROUND(B.F_YTC_DECIMAL8,4) 实发数量
			                        ,ROUND(B.FPRICEQTY,4) 计价数量
			                        ,ROUND(B.FTAXPRICE,4) 含税单价
			                        ,ROUND(B.FALLAMOUNTFOR,4) 价税合计
                                    ,G.FNAME 客户名称                                    

                        FROM dbo.T_AR_RECEIVABLE A
                        INNER JOIN dbo.T_AR_RECEIVABLEENTRY B ON A.FID=B.FID
                        INNER JOIN dbo.T_BD_MATERIAL C ON B.FMATERIALID=C.FMATERIALID
                        INNER JOIN dbo.T_BD_MATERIAL_L D ON C.FMATERIALID=D.FMATERIALID AND D.FLOCALEID=2052
                        INNER JOIN dbo.T_BAS_ASSISTANTDATAENTRY_l E ON C.F_YTC_ASSISTANT7=E.FENTRYID AND E.FLOCALEID=2052
						INNER JOIN dbo.T_BD_CUSTOMER F ON A.FCUSTOMERID=F.FCUSTID
                        INNER JOIN dbo.T_BD_CUSTOMER_L G ON G.FCUSTID=F.FCUSTID AND G.FLOCALEID=2052                           

                        WHERE A.FDOCUMENTSTATUS='C'
                        AND A.FSALEDEPTID='449713'   --工业涂料事业部
                        AND CONVERT(VARCHAR(10),A.FDATE,23)>='{sdt}'
                        AND CONVERT(VARCHAR(10),A.FDATE,23)<='{edt}'
                        AND F.FNUMBER IN ({customerlist})
                        ORDER BY A.FDATE,A.FCUSTOMERID
                        ";
            return _result;
        }

        /// <summary>
        /// 销售发货清单查询
        /// </summary>
        /// <param name="sdt"></param>
        /// <param name="edt"></param>
        /// <param name="customerlist"></param>
        /// <returns></returns>
        public string SearchSalesOutList(string sdt, string edt, string customerlist)
        {
            _result = $@"
                              SELECT A.FCUSTOMERID,A.FID
			                        ,'雅图高新材料股份有限公司' 交货单位
			                        ,X2.FDATAVALUE 终端客户
			                        ,F.FNAME 收货单位
			                        ,X3.F_YTC_TEXT1 收集单位1
			                        ,X0.FNAME 二级客户
			                        ,X1.FNAME 三级客户
			                        ,A.F_YTC_TEXT2 摘要
			                        ,A.F_YTC_TEXT3 销售订单号
			                        ,CONVERT(VARCHAR(10),A.FDATE,23) 日期
			                        ,A.F_YTC_TEXT13 U订货单号
			                        ,A.FBILLNO 单据编号
			                        ,A.F_YTC_TEXT1 托运货场地址
			                        ,D.FNAME 产品名称,D.FSPECIFICATION 规格
			                        ,B.F_YTC_DECIMAL8 实发罐数
			                        ,B.FTAXPRICE 单价
			                        ,B.FALLAMOUNTFOR 合同金额
			                        ,B.FCOMMENT 备注
			                        ,B.F_YTC_ASSISTANT10 促销备注
			                        ,X5.FNAME 开票人

                        FROM dbo.T_AR_RECEIVABLE A
                        INNER JOIN dbo.T_AR_RECEIVABLEENTRY B ON A.FID=B.FID

                        INNER JOIN dbo.T_BD_MATERIAL C ON B.FMATERIALID=C.FMATERIALID
                        INNER JOIN dbo.T_BD_MATERIAL_L D ON C.FMATERIALID=D.FMATERIALID AND D.FLOCALEID=2052
                        INNER JOIN dbo.T_BD_CUSTOMER E ON A.FCUSTOMERID=E.FCUSTID
                        INNER JOIN dbo.T_BD_CUSTOMER_L F ON E.FCUSTID=F.FCUSTID AND F.FLOCALEID=2052

                        LEFT JOIN dbo.T_BD_CUSTOMER_L X0 ON A.F_YTC_BASE=X0.FCUSTID AND X0.FLOCALEID=2052
                        LEFT JOIN dbo.T_BD_CUSTOMER_L X1 ON A.F_YTC_BASE1=X1.FCUSTID AND X1.FLOCALEID=2052

                        LEFT JOIN dbo.T_BAS_ASSISTANTDATAENTRY_L X2 ON A.F_YTC_ASSISTANT9=X2.FENTRYID
                        LEFT JOIN dbo.ytc_t_Cust100018 X3 ON A.F_YTC_BASE10=X3.FID

                        LEFT JOIN T_SEC_USER X5 ON A.FCREATORID=X5.FUSERID

                        WHERE A.FDOCUMENTSTATUS='C'
                        AND CONVERT(VARCHAR(10),A.FDATE,23)>='{sdt}'
                        AND CONVERT(VARCHAR(10),A.FDATE,23)<='{edt}'
                        AND E.FNUMBER IN ({customerlist})
                        --AND A.FBILLNO='AR00175220'--'AR00130430'--'AR00175243'--'AR00175282'
                        ORDER BY A.FCUSTOMERID,A.FID 
                        ";
            return _result;
        }

        /// <summary>
        /// 获取客户列表信息-'自定义批量导出'功能使用
        /// </summary>
        /// <returns></returns>
        public string GetSearchCustomerList(string customer)
        {
            _result = $@"
                            SELECT A.FCUSTID,A.FNUMBER 客户编码,B.FNAME 客户名称,A.F_YTC_TEXT41 客户开票名称
                                  ,CASE LEN(F_YTC_TEXT50) WHEN 0 THEN 'A' ELSE A.F_YTC_TEXT50 END 收货天数 
                            FROM dbo.T_BD_CUSTOMER A
                            INNER JOIN dbo.T_BD_CUSTOMER_L B ON A.FCUSTID=B.FCUSTID AND B.FLOCALEID=2052
                            WHERE a.FDOCUMENTSTATUS='C'
                            --AND A.FNUMBER NOT LIKE 'INT%'
                            AND A.FNUMBER ='{customer}'
                            ORDER BY a.FCUSTID
                         ";

            return _result;
        }

        /// <summary>
        /// 根据获取的数值,转换为带'千位符'进行显示(并保留两位小数进行输出)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ChangeMoneyValue(decimal value)
        {
            _result = $@"select convert(varchar,cast({value} AS MONEY),1) value";
            return _result;
        }
    }
}
