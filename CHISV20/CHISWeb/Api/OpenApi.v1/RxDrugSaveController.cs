using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHIS.DbContext;
using CHIS.Models;
using CHIS.Models.DataModel;
using CHIS.Services;
using Microsoft.AspNetCore.Mvc;

namespace CHIS.Api.OpenApi
{
    public class RxDrugSaveController : OpenApiBaseController
    {
        private CustomerService _cusSvr;

        private RxService _rxSvr;

        public RxDrugSaveController(CHISEntitiesSqlServer db, CustomerService cusSvr,RxService rxSvr) : base(db)
        {
            _cusSvr = cusSvr;
            _rxSvr = rxSvr;
        }

        [HttpPost]
        public async Task<IActionResult> RxCusM(RxMobileInputModel model)
        {
            if (model.IsAgreement == false)
            {
                return View("RxCustomerInputMobile", model);
            }

            if (model.CustomerId == 0)
            {
                var cus = await _cusSvr.CreateCustomerAsync(
                    model.CustomerName,
                    model.CustomerMobile,
                    model.CustomerIdCode,
                    sysSources.处方药记录快录,
                    0, "");

                model.CustomerId = cus.CustomerID;
            }

            var rxModel = new CHIS_DrugStore_RxSave(
                model.Station.StationID,
                model.Doctor.DoctorId,
                model.CustomerId,
                model.CustomerName,
                model.CustomerIdCode,
                model.CustomerMobile,
                model.CustomerGenderStr,
                model.RxPicUrl1,
                model.RxPicUrl2,
                model.RxPicUrl3);

            var rxSaveId = _rxSvr.SaveCustomer(rxModel);

            try
            {
                var rlt = MyDynamicResult(true, "录入成功");
                rlt.RxSaveId = rxSaveId;
                return Ok(rlt);
            }catch(Exception e)
            {
                var rlt = MyDynamicResult(e);
                return BadRequest(rlt);
            }
        }
    }
}
