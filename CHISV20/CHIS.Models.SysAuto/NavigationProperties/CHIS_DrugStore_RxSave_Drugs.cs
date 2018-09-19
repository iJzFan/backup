using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CHIS.Models
{
    public partial class CHIS_DrugStore_RxSave_Drugs
    {
        [ForeignKey("RxSaveId")]

        public CHIS_DrugStore_RxSave RxOrder { get; set; }
    }
}
