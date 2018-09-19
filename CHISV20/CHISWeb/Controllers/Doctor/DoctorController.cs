using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Ass.Models;
using Ass;
using System.Collections.Generic;
using CHIS.Models.ViewModels;
using CHIS.Services;
using Microsoft.Extensions.Caching.Memory;
using AutoMapper;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{
    public partial class DoctorController : BaseController
    {
        DispensingService _dispensingSvr;
        DrugService _drugSrv;
        IMemoryCache _memoryCache;
        Services.JKWebNetService _jkSvr;
        AccessService _accSvr;
        DictService _dictSvr;
        TreatService _treatSvr;
        private IMapper _mapper;
        public DoctorController(IMemoryCache memoryCache
            , DispensingService dispensingSvr
            , DrugService drugSrv
            , Services.JKWebNetService jkSvr
            , AccessService accSvr
            , DictService dictSvr
            ,TreatService treatSvr
            , IMapper mapper
            , DbContext.CHISEntitiesSqlServer db) : base(db)
        {
            _dispensingSvr = dispensingSvr;
            _drugSrv = drugSrv;
            _memoryCache = memoryCache;
            _jkSvr = jkSvr;
            _accSvr = accSvr;
            _dictSvr = dictSvr;
            _treatSvr = treatSvr;
            _mapper = mapper;
        }




    }
}