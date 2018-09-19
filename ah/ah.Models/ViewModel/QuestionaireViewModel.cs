using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ah.Models.ViewModel
{
   public class QuestionaireViewModel
    {
        public ah.Models.AHMS_QAFlow_Main QAMain { get; set; }
        public IEnumerable<ah.Models.AHMS_QAFlow_Main_detail> QADetails { get; set; }

        [Required(ErrorMessage ="必须传入问卷总题数")]
        public int TotalQuesNum { get; set; }
    }
 
    public class vwQuestionaireModel
    {
        public ah.Models.vwAHMS_QAFlow_Main QAMain { get; set; }
        public IEnumerable<ah.Models.vwAHMS_QAFlow_Main_detail> QADetails { get; set; }
    }
}
