using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CHIS.Models
{
    public partial class CHIS_Code_Gift
    {
        public void Publish(string cardId)
        {
            this.WeChatCardId = cardId;
        }

        public void UpdateCheck(CHIS_Code_Gift old)
        {
            if (this.ExpiryDate != old.ExpiryDate ||
                this.AvailableDays != old.AvailableDays ||
                this.DiscountPrice != old.DiscountPrice||
                this.OrderLimit != old.OrderLimit||
                this.DiscountRate!= old.DiscountRate||
                this.Type!= old.Type)
            {
                throw new ApplicationException("存在不能修改的基本信息,请选择发布新礼品而不是修改礼品");
            }
        }
    }
}
