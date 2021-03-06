﻿ 
 
 
   
//------------------------------------------------------------------------------   
// <auto-generated> 
//     此代码由T4模板自动生成    
//	   生成时间 2018-01-16 14:23:25 by Rex 
//     对此文件的更改可能会导致不正确的行为，并且如果  
//     重新生成代码，这些更改将会丢失。 
// </auto-generated>    
//------------------------------------------------------------------------------
 
using System; 
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Ass;
using ah.Models;
namespace ah.DbContext
{	
		 
				public partial class AHMSEntitiesSqlServer : Microsoft.EntityFrameworkCore.DbContext{

					private string _connectionString = null;
					public AHMSEntitiesSqlServer(DbContextOptions<AHMSEntitiesSqlServer> options) : base(options) { }
                    public AHMSEntitiesSqlServer(string connectionString) : base()
                    {
                            this._connectionString = connectionString;
                    }

					protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
					{
                        if (!string.IsNullOrEmpty(_connectionString))
                        {
                            string str = this._connectionString;
                            optionsBuilder.UseSqlServer(str);
                        }
					} 
					 


					public Ass.Data.IDbUtils DbUtils{
						get{return new Ass.Data.SqlServerDbUtils();}
					}

										public virtual DbSet<AH_Code_Config> AH_Code_Config { get; set; }
										public virtual DbSet<AHMS_Center_HealthDoc> AHMS_Center_HealthDoc { get; set; }
										public virtual DbSet<AHMS_Code_QAName> AHMS_Code_QAName { get; set; }
										public virtual DbSet<AHMS_Customer_AllergicHistory> AHMS_Customer_AllergicHistory { get; set; }
										public virtual DbSet<AHMS_Daily_Member> AHMS_Daily_Member { get; set; }
										public virtual DbSet<AHMS_HMGR_BreastMgr> AHMS_HMGR_BreastMgr { get; set; }
										public virtual DbSet<AHMS_HMGR_BreastMgr_Detail> AHMS_HMGR_BreastMgr_Detail { get; set; }
										public virtual DbSet<AHMS_QA_Item> AHMS_QA_Item { get; set; }
										public virtual DbSet<AHMS_QA_ItemMain> AHMS_QA_ItemMain { get; set; }
										public virtual DbSet<AHMS_QAFlow_Main> AHMS_QAFlow_Main { get; set; }
										public virtual DbSet<AHMS_QAFlow_Main_detail> AHMS_QAFlow_Main_detail { get; set; }
										public virtual DbSet<CHIS_Charge_Invoice> CHIS_Charge_Invoice { get; set; }
										public virtual DbSet<CHIS_Charge_InvoiceApply> CHIS_Charge_InvoiceApply { get; set; }
										public virtual DbSet<CHIS_Charge_Pay> CHIS_Charge_Pay { get; set; }
										public virtual DbSet<CHIS_Charge_Pay_Detail_ExtraFee> CHIS_Charge_Pay_Detail_ExtraFee { get; set; }
										public virtual DbSet<CHIS_Charge_Pay_Detail_Formed> CHIS_Charge_Pay_Detail_Formed { get; set; }
										public virtual DbSet<CHIS_Charge_Pay_Detail_Herb> CHIS_Charge_Pay_Detail_Herb { get; set; }
										public virtual DbSet<CHIS_Charge_PayPre> CHIS_Charge_PayPre { get; set; }
										public virtual DbSet<CHIS_Charge_PayPre_Detail_ExtraFee> CHIS_Charge_PayPre_Detail_ExtraFee { get; set; }
										public virtual DbSet<CHIS_Charge_PayPre_Detail_Formed> CHIS_Charge_PayPre_Detail_Formed { get; set; }
										public virtual DbSet<CHIS_Charge_PayPre_Detail_Herb> CHIS_Charge_PayPre_Detail_Herb { get; set; }
										public virtual DbSet<CHIS_Charge_Refund> CHIS_Charge_Refund { get; set; }
										public virtual DbSet<CHIS_Charge_Refund_Detail> CHIS_Charge_Refund_Detail { get; set; }
										public virtual DbSet<CHIS_Charge_RefundPre> CHIS_Charge_RefundPre { get; set; }
										public virtual DbSet<CHIS_Charge_RefundPre_Detail> CHIS_Charge_RefundPre_Detail { get; set; }
										public virtual DbSet<CHIS_Code_CalendarDetail> CHIS_Code_CalendarDetail { get; set; }
										public virtual DbSet<CHIS_Code_CalendarMaster> CHIS_Code_CalendarMaster { get; set; }
										public virtual DbSet<CHIS_Code_CheckItem> CHIS_Code_CheckItem { get; set; }
										public virtual DbSet<CHIS_Code_Class> CHIS_Code_Class { get; set; }
										public virtual DbSet<CHIS_Code_Customer> CHIS_Code_Customer { get; set; }
										public virtual DbSet<CHIS_Code_Customer_AddressInfos> CHIS_Code_Customer_AddressInfos { get; set; }
										public virtual DbSet<CHIS_Code_Customer_HealthInfo> CHIS_Code_Customer_HealthInfo { get; set; }
										public virtual DbSet<CHIS_Code_CustomerRelationship> CHIS_Code_CustomerRelationship { get; set; }
										public virtual DbSet<CHIS_Code_Department> CHIS_Code_Department { get; set; }
										public virtual DbSet<CHIS_Code_Diagnosis> CHIS_Code_Diagnosis { get; set; }
										public virtual DbSet<CHIS_Code_Dict_Detail> CHIS_Code_Dict_Detail { get; set; }
										public virtual DbSet<CHIS_Code_Dict_Main> CHIS_Code_Dict_Main { get; set; }
										public virtual DbSet<CHIS_Code_Doctor> CHIS_Code_Doctor { get; set; }
										public virtual DbSet<CHIS_Code_DoctorCertbook> CHIS_Code_DoctorCertbook { get; set; }
										public virtual DbSet<CHIS_Code_DoctorWorkInfo> CHIS_Code_DoctorWorkInfo { get; set; }
										public virtual DbSet<CHIS_Code_Drug_Main> CHIS_Code_Drug_Main { get; set; }
										public virtual DbSet<CHIS_Code_Drug_Main_Apply> CHIS_Code_Drug_Main_Apply { get; set; }
										public virtual DbSet<CHIS_Code_Drug_Outpatient> CHIS_Code_Drug_Outpatient { get; set; }
										public virtual DbSet<CHIS_Code_Drug_Storage> CHIS_Code_Drug_Storage { get; set; }
										public virtual DbSet<CHIS_Code_Rel_DoctorDeparts> CHIS_Code_Rel_DoctorDeparts { get; set; }
										public virtual DbSet<CHIS_Code_Storage> CHIS_Code_Storage { get; set; }
										public virtual DbSet<CHIS_Code_Supplier> CHIS_Code_Supplier { get; set; }
										public virtual DbSet<CHIS_Code_WorkStation> CHIS_Code_WorkStation { get; set; }
										public virtual DbSet<CHIS_Code_WorkStationDevices> CHIS_Code_WorkStationDevices { get; set; }
										public virtual DbSet<CHIS_Data_PsychPretreatQs> CHIS_Data_PsychPretreatQs { get; set; }
										public virtual DbSet<CHIS_DataInput_OneMachine> CHIS_DataInput_OneMachine { get; set; }
										public virtual DbSet<CHIS_DataTemp_SendMailVCode> CHIS_DataTemp_SendMailVCode { get; set; }
										public virtual DbSet<CHIS_DataTemp_SMS> CHIS_DataTemp_SMS { get; set; }
										public virtual DbSet<CHIS_Doctor_ExtraFee> CHIS_Doctor_ExtraFee { get; set; }
										public virtual DbSet<CHIS_Doctor_OnOffDutyData> CHIS_Doctor_OnOffDutyData { get; set; }
										public virtual DbSet<CHIS_Doctor_SickNote> CHIS_Doctor_SickNote { get; set; }
										public virtual DbSet<CHIS_Doctor_TreatExt> CHIS_Doctor_TreatExt { get; set; }
										public virtual DbSet<CHIS_DoctorAdvice_Formed> CHIS_DoctorAdvice_Formed { get; set; }
										public virtual DbSet<CHIS_DoctorAdvice_Formed_Detail> CHIS_DoctorAdvice_Formed_Detail { get; set; }
										public virtual DbSet<CHIS_DoctorAdvice_Herbs> CHIS_DoctorAdvice_Herbs { get; set; }
										public virtual DbSet<CHIS_DoctorAdvice_Herbs_Detail> CHIS_DoctorAdvice_Herbs_Detail { get; set; }
										public virtual DbSet<CHIS_DoctorTreat> CHIS_DoctorTreat { get; set; }
										public virtual DbSet<CHIS_DrugStock_Monitor> CHIS_DrugStock_Monitor { get; set; }
										public virtual DbSet<CHIS_DrugStock_Monitor_Log> CHIS_DrugStock_Monitor_Log { get; set; }
										public virtual DbSet<CHIS_DrugStock_Monitor_WebNet> CHIS_DrugStock_Monitor_WebNet { get; set; }
										public virtual DbSet<CHIS_DrugStock_Out> CHIS_DrugStock_Out { get; set; }
										public virtual DbSet<CHIS_DrugStore_RxSave> CHIS_DrugStore_RxSave { get; set; }
										public virtual DbSet<CHIS_DrugStore_RxSave_Drugs> CHIS_DrugStore_RxSave_Drugs { get; set; }
										public virtual DbSet<CHIS_DurgStock_Income> CHIS_DurgStock_Income { get; set; }
										public virtual DbSet<CHIS_Nurse_Addition> CHIS_Nurse_Addition { get; set; }
										public virtual DbSet<CHIS_Nurse_InfuseReg> CHIS_Nurse_InfuseReg { get; set; }
										public virtual DbSet<CHIS_Nurse_SkinTest> CHIS_Nurse_SkinTest { get; set; }
										public virtual DbSet<CHIS_Register> CHIS_Register { get; set; }
										public virtual DbSet<CHIS_Shipping_DispensingLog> CHIS_Shipping_DispensingLog { get; set; }
										public virtual DbSet<CHIS_Shipping_NetOrder> CHIS_Shipping_NetOrder { get; set; }
										public virtual DbSet<CHIS_Shipping_NetOrder_Formed_Detail> CHIS_Shipping_NetOrder_Formed_Detail { get; set; }
										public virtual DbSet<CHIS_Shipping_NetOrder_Herbs> CHIS_Shipping_NetOrder_Herbs { get; set; }
										public virtual DbSet<CHIS_Shipping_NetOrder_Herbs_Detail> CHIS_Shipping_NetOrder_Herbs_Detail { get; set; }
										public virtual DbSet<CHIS_Sys_FuncAccess> CHIS_Sys_FuncAccess { get; set; }
										public virtual DbSet<CHIS_Sys_FuncDetail> CHIS_Sys_FuncDetail { get; set; }
										public virtual DbSet<CHIS_SYS_Function> CHIS_SYS_Function { get; set; }
										public virtual DbSet<CHIS_Sys_Login> CHIS_Sys_Login { get; set; }
										public virtual DbSet<CHIS_Sys_LoginExt> CHIS_Sys_LoginExt { get; set; }
										public virtual DbSet<CHIS_Sys_LoginExt_Rel_LoginAllowRoles> CHIS_Sys_LoginExt_Rel_LoginAllowRoles { get; set; }
										public virtual DbSet<CHIS_Sys_LoginExt_Rel_RoleFunc> CHIS_Sys_LoginExt_Rel_RoleFunc { get; set; }
										public virtual DbSet<CHIS_Sys_LoginExt_Role> CHIS_Sys_LoginExt_Role { get; set; }
										public virtual DbSet<CHIS_Sys_MyConfig> CHIS_Sys_MyConfig { get; set; }
										public virtual DbSet<CHIS_Sys_Rel_DoctorStationRoles> CHIS_Sys_Rel_DoctorStationRoles { get; set; }
										public virtual DbSet<CHIS_Sys_Rel_DoctorStations> CHIS_Sys_Rel_DoctorStations { get; set; }
										public virtual DbSet<CHIS_Sys_Rel_RoleFuncDetails> CHIS_Sys_Rel_RoleFuncDetails { get; set; }
										public virtual DbSet<CHIS_Sys_Rel_RoleFunctions> CHIS_Sys_Rel_RoleFunctions { get; set; }
										public virtual DbSet<CHIS_Sys_Rel_WorkStationRoles> CHIS_Sys_Rel_WorkStationRoles { get; set; }
										public virtual DbSet<CHIS_SYS_Role> CHIS_SYS_Role { get; set; }
										public virtual DbSet<CHIS_Sys_UserMenu> CHIS_Sys_UserMenu { get; set; }
										public virtual DbSet<CHIS_SYS_UserStationRight> CHIS_SYS_UserStationRight { get; set; }
										public virtual DbSet<dfdrug> dfdrug { get; set; }
										public virtual DbSet<SYS_ChinaArea> SYS_ChinaArea { get; set; }
										public virtual DbSet<T_Sys_DrugDetail> T_Sys_DrugDetail { get; set; }
										public virtual DbSet<TEMP_Excel_JKImport> TEMP_Excel_JKImport { get; set; }
										public virtual DbSet<tmp_a> tmp_a { get; set; }
										public virtual DbSet<tmp_d> tmp_d { get; set; }
										public virtual DbSet<tmp_drug> tmp_drug { get; set; }
										public virtual DbSet<vwAHMS_Center_HealthDoc> vwAHMS_Center_HealthDoc { get; set; }
										public virtual DbSet<vwAHMS_Customer_AllergicHistory> vwAHMS_Customer_AllergicHistory { get; set; }
										public virtual DbSet<vwAHMS_Daily_Member> vwAHMS_Daily_Member { get; set; }
										public virtual DbSet<vwAHMS_HMGR_BreastMgr> vwAHMS_HMGR_BreastMgr { get; set; }
										public virtual DbSet<vwAHMS_HMGR_BreastMgr_Detail> vwAHMS_HMGR_BreastMgr_Detail { get; set; }
										public virtual DbSet<vwAHMS_QAFlow_Main> vwAHMS_QAFlow_Main { get; set; }
										public virtual DbSet<vwAHMS_QAFlow_Main_detail> vwAHMS_QAFlow_Main_detail { get; set; }
										public virtual DbSet<vwCHIS_Charge_DoctorTreated_NeedPay> vwCHIS_Charge_DoctorTreated_NeedPay { get; set; }
										public virtual DbSet<vwCHIS_Charge_Pay> vwCHIS_Charge_Pay { get; set; }
										public virtual DbSet<vwCHIS_Charge_Pay_Detail_ExtraFee> vwCHIS_Charge_Pay_Detail_ExtraFee { get; set; }
										public virtual DbSet<vwCHIS_Charge_Pay_Detail_Formed> vwCHIS_Charge_Pay_Detail_Formed { get; set; }
										public virtual DbSet<vwCHIS_Charge_Pay_Detail_Herb> vwCHIS_Charge_Pay_Detail_Herb { get; set; }
										public virtual DbSet<vwCHIS_Code_Customer> vwCHIS_Code_Customer { get; set; }
										public virtual DbSet<vwCHIS_Code_Customer_AddressInfos> vwCHIS_Code_Customer_AddressInfos { get; set; }
										public virtual DbSet<vwCHIS_Code_Department> vwCHIS_Code_Department { get; set; }
										public virtual DbSet<vwCHIS_Code_DictDetail> vwCHIS_Code_DictDetail { get; set; }
										public virtual DbSet<vwCHIS_Code_Doctor> vwCHIS_Code_Doctor { get; set; }
										public virtual DbSet<vwCHIS_Code_Doctor_Authenticate> vwCHIS_Code_Doctor_Authenticate { get; set; }
										public virtual DbSet<vwCHIS_Code_DoctorCertbook> vwCHIS_Code_DoctorCertbook { get; set; }
										public virtual DbSet<vwCHIS_Code_Drug_Main> vwCHIS_Code_Drug_Main { get; set; }
										public virtual DbSet<vwCHIS_Code_Drug_Main_Apply> vwCHIS_Code_Drug_Main_Apply { get; set; }
										public virtual DbSet<vwCHIS_Code_Rel_DoctorDeparts> vwCHIS_Code_Rel_DoctorDeparts { get; set; }
										public virtual DbSet<vwCHIS_Code_Storage> vwCHIS_Code_Storage { get; set; }
										public virtual DbSet<vwCHIS_Code_WorkStation> vwCHIS_Code_WorkStation { get; set; }
										public virtual DbSet<vwCHIS_Code_WorkStationDevices> vwCHIS_Code_WorkStationDevices { get; set; }
										public virtual DbSet<vwCHIS_Code_WorkStationEx> vwCHIS_Code_WorkStationEx { get; set; }
										public virtual DbSet<vwCHIS_Data_PsychPretreatQs> vwCHIS_Data_PsychPretreatQs { get; set; }
										public virtual DbSet<vwCHIS_DataInput_OneMachine> vwCHIS_DataInput_OneMachine { get; set; }
										public virtual DbSet<vwCHIS_Doctor_ExtraFee> vwCHIS_Doctor_ExtraFee { get; set; }
										public virtual DbSet<vwCHIS_Doctor_OnOffDutyData> vwCHIS_Doctor_OnOffDutyData { get; set; }
										public virtual DbSet<vwCHIS_Doctor_TestItem_Refund> vwCHIS_Doctor_TestItem_Refund { get; set; }
										public virtual DbSet<vwCHIS_DoctorAdvice_Formed_Detail> vwCHIS_DoctorAdvice_Formed_Detail { get; set; }
										public virtual DbSet<vwCHIS_DoctorAdvice_Herbs> vwCHIS_DoctorAdvice_Herbs { get; set; }
										public virtual DbSet<vwCHIS_DoctorAdvice_Herbs_Detail> vwCHIS_DoctorAdvice_Herbs_Detail { get; set; }
										public virtual DbSet<vwCHIS_DoctorTreat> vwCHIS_DoctorTreat { get; set; }
										public virtual DbSet<vwCHIS_DrugStock_Monitor> vwCHIS_DrugStock_Monitor { get; set; }
										public virtual DbSet<vwCHIS_DrugStock_Out> vwCHIS_DrugStock_Out { get; set; }
										public virtual DbSet<vwCHIS_DrugStock_Outpatient> vwCHIS_DrugStock_Outpatient { get; set; }
										public virtual DbSet<vwCHIS_DrugStock_Workstation> vwCHIS_DrugStock_Workstation { get; set; }
										public virtual DbSet<vwCHIS_DurgStock_Income> vwCHIS_DurgStock_Income { get; set; }
										public virtual DbSet<vwCHIS_Register> vwCHIS_Register { get; set; }
										public virtual DbSet<vwCHIS_RegisterInfos> vwCHIS_RegisterInfos { get; set; }
										public virtual DbSet<vwCHIS_Shipping_NetOrder> vwCHIS_Shipping_NetOrder { get; set; }
										public virtual DbSet<vwCHIS_Shipping_NetOrder_Formed_Detail> vwCHIS_Shipping_NetOrder_Formed_Detail { get; set; }
										public virtual DbSet<vwCHIS_Sys_Login> vwCHIS_Sys_Login { get; set; }
										public virtual DbSet<vwCHIS_Sys_Rel_DoctorStationRoles> vwCHIS_Sys_Rel_DoctorStationRoles { get; set; }
										public virtual DbSet<vwCHIS_Sys_Rel_RoleFuncDetails> vwCHIS_Sys_Rel_RoleFuncDetails { get; set; }
										public virtual DbSet<vwCHIS_Sys_Rel_WorkStationRoles> vwCHIS_Sys_Rel_WorkStationRoles { get; set; }
										public virtual DbSet<ypzdk2> ypzdk2 { get; set; }
									}
	}

