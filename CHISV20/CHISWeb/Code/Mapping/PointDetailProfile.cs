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
    public class PointsDetailProfile:Profile
    {
        public PointsDetailProfile()
        {
            CreateMap<PointsDetailInputModel, CHIS_Customer_PointsDetail>()
                .ForMember(x=>x.Id,opt=>opt.Ignore())
                .ForMember(x=>x.CreatedTime, opt => opt.MapFrom(o=>DateTime.Now));

            CreateMap<CHIS_Customer_PointsDetail, PointsDetailViewModel>();
        }
    }
}
