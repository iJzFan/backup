using AutoMapper;
using CHIS.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.Mapping
{
    public class DispensingProfile: Profile
    {
        public DispensingProfile()
        {
            CreateMap<DispensingItemViewModel, DispensingItem>()
                .ForMember(m=>m.CustomerGender,opt=>opt.MapFrom(a=>a.Gender))
                .ForMember(m=>m.CustomerBirthday,opt=>opt.MapFrom(a=>a.Birthday));
            
        }
    }
}
