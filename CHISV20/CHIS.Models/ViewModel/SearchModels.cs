using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS.Models.ViewModel
{
   public class SickNoteCheckModel
    {
        public CHIS_Doctor_SickNote SickNote { get; set; }
        public vwCHIS_DoctorTreat Treat { get; set; }
    }

    public class PrescriptionCheckModel
    {
       public Guid PrescriptionNo { get; set; }
        public vwCHIS_DoctorTreat Treat { get; set; }
        public CHIS_DoctorAdvice_Formed formed { get; set; }
        public CHIS_DoctorAdvice_Herbs herb { get; set; }
    }
}
