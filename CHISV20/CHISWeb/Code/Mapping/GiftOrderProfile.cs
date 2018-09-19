using AutoMapper;
using CHIS.Models;
using CHIS.Models.InputModel;
using CHIS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.Mapping
{
    public class GiftOrderProfile:Profile
    {
        public GiftOrderProfile()
        {
            CreateMap<GiftOrderInputModel, CHIS_Gift_Order>();
                //.ForMember(x => x.CreatedTime, opt => opt.MapFrom(o => DateTime.Now));

            CreateMap<CHIS_Gift_Order, GiftOrderViewModel>()
                .ForMember(x => x.GiftName, opt => opt.MapFrom(o => o.Gift.GiftName))
                .ForMember(x => x.CoverImg, opt => opt.MapFrom(o => o.Gift.CoverImg))
                .ForMember(x => x.Instruction, opt => opt.MapFrom(o => o.Gift.Instruction))
                .AfterMap((p,x) => x.CheckOrderState());
        }
    }
}
