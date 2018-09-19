using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.ViewModel
{
    public class InputControlsDataModel
    {
        public int DoctorId { get; set; }
        public int DoctorId_d { get; set; }
        public int DoctorId_r { get; set; }

        public bool IsMenu { get; set; }
        public bool IsMenu_d { get; set; }
        public bool IsMenu_r { get; set; }

        public bool IsSwitch2 { get; set; }
        public bool IsSwitch2_d { get; set; }
        public bool IsSwitch2_r { get; set; }


        public bool? Is3Status { get; set; }
        public bool? Is3Status_d { get; set; }
        public bool? Is3Status_r { get; set; }

        public bool? Is3Status1 { get; set; }
        public bool? Is3Status1_d { get; set; }
        public bool? Is3Status1_r { get; set; }



        public string Name { get; set; }
        public string Remark { get; set; }
        public int IntNumber { get; set; }

        public int FormTypeId { get; set; }
        public int FormTypeId_d { get; set; }
        public int FormTypeId_r { get; set; }

        public int FormTypeId2 { get; set; }
        public int FormTypeId2_d { get; set; }
        public int FormTypeId2_r { get; set; }

        public DateTime? StopDate { get; set; }
        public DateTime? StopDate_d { get; set; }
        public DateTime? StopDate_r { get; set; }

        public DateTime? StopDate2 { get; set; }
        public DateTime? StopDate2_d { get; set; }
        public DateTime? StopDate2_r { get; set; }

        public int AreaId { get; set; }
        public int AreaId_d { get; set; }
        public int AreaId_r { get; set; }


        public string KeyName { get; set; }
        public string KeyName_d { get; set; }
        public string KeyName_r { get; set; }
    }
}
