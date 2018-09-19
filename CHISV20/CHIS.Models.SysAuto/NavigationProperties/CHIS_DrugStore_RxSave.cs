using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHIS.Models
{
    public partial class CHIS_DrugStore_RxSave
    {
        public CHIS_DrugStore_RxSave() { }

        [ForeignKey("RxSaveId")]
        public IList<CHIS_DrugStore_RxSave_Drugs> DrugList { get; set; }

        public CHIS_DrugStore_RxSave(
            int stationId,
            int doctorId,
            int customerId,
            string customerName,
            string customerIdCode,
            string customerMobile,
            string customerGender,
            string rxPicUrl1,
            string rxPicUrl2,
            string rxPicUrl3)
        {
            this.StationId = stationId;
            this.DoctorId = doctorId;
            this.CustomerId = customerId;
            this.CustomerName = customerName;
            this.CustomerIdCode = customerIdCode;
            this.CustomerMobile = customerMobile;
            this.RxPicUrl1 = rxPicUrl1;
            this.RxPicUrl2 = rxPicUrl2;
            this.RxPicUrl3 = rxPicUrl3;
            this.CustomerGenderStr = customerGender;
            this.IsAgreement = true;
            this.IsCompleted = false;
            this.SendTime = DateTime.Now;
            this.sysCreateTime = DateTime.Now;
            this.CheckDrugMan = "";
            this.SendDrugMan = "";
            this.CheckTime = DateTime.Today;

        }

    }
}
