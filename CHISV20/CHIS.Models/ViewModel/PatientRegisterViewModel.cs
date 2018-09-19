using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.ViewModels
{
    public class PatientRegisterViewModel
    {

        #region  患者挂号
        public Models.vwCHIS_Register vwCHIS_Register { get; set; }        //挂号信息
        public Models.CHIS_Code_Customer CHIS_Code_Customer { get; set; }  //患者信息
        #endregion
    }
}
