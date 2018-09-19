using AutoMapper;
using CHIS.Models;
using CHIS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.Mapping
{
    public class GiftProfile : Profile
    {
        public GiftProfile()
        {
            // Add as many of these lines as you need to map your objects

            CreateMap<GiftInputModel, CHIS_Code_Gift>();
            CreateMap<CHIS_Code_Gift, GiftViewModel>();
            CreateMap<GiftViewModel, CHIS_Code_Gift>();
        }
    }
}
