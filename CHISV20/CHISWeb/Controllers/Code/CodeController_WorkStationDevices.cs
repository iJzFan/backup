//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Ass;

//// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

//namespace CHIS.Controllers
//{
//    public partial class CodeController
//    {

//        //设备配置
//        public IActionResult DeviceConfig()
//        {
//            var u = GetCurrentUserInfo();
//            var station = MainDbContext.CHIS_Code_WorkStation.Where(m => m.StationID == u.StationID).FirstOrDefault();             
//            ViewBag.StationName = station?.StationName;
//            ViewBag.StationId = station?.StationID;
//            return View();
//        }
//        public IActionResult GetJqGridJson_DevicesOfStation(int stationId)
//        {
//            var u = base.GetCurrentUserInfo();
//            if (stationId <= 0) stationId = u.StationID;
//            int pageIndex = 1, pageSize = 0; string sort = "";
//            base.getJqGridInfo(out pageIndex, out pageSize, out sort, 1, u.TableRecordsPerPage);
//            if (string.IsNullOrWhiteSpace(sort)) sort = getForm("sort");
//            try
//            {
//                var find = from item in MainDbContext.vwCHIS_Code_WorkStationDevices
//                           where item.StationId == stationId                           
//                           select item;

//                int findTotal = find.Count();
//                int totalPage = (int) Math.Ceiling(findTotal * 1.0f / pageSize);


//                //排序获取当前页的数据  
//                var dataList = find.OrderBy(m=>m.Id).Skip(pageSize * (pageIndex - 1)).
//                               Take(pageSize);
//                return Json(new
//                {
//                    page = pageIndex,
//                    total = totalPage,
//                    records = findTotal,
//                    rows = dataList
//                });
//            }
//            catch (Exception ex)
//            {
//                Loger.WriteError("Code", "GetJqGridJson_DevicesOfStation", ex);
//                return View("ErrorBlank", ex);
//            }
//        }
//        public IActionResult GetJson_DevicesOfStation_Add(Models.CHIS_Code_WorkStationDevices model)
//        {
//            try
//            {
//                if (model.StationId == 0) throw new Exception("没有传入工作站Id");
//                if (model.DeviceTypeCode.IsEmpty()) throw new Exception("没有传入设备名");
//                if (model.DeviceCode.IsEmpty()) throw new Exception("没有传入设备号");
//                model.sysOpMan = GetCurrentUserInfo().OpManFullMsg;
//                model.sysOpId = GetCurrentUserInfo().OpID;
//                model.sysOpTime = DateTime.Now;
//                MainDbContext.CHIS_Code_WorkStationDevices.Add(model);
//                MainDbContext.SaveChanges();
//                return Json(new { rlt = true });
//            }
//            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
//        }
//        public IActionResult GetJson_DevicesOfStation_Mod(Models.CHIS_Code_WorkStationDevices model)
//        {
//            try
//            {
//                var u = GetCurrentUserInfo();
//                var find = MainDbContext.CHIS_Code_WorkStationDevices.AsNoTracking().FirstOrDefault(m => m.Id == model.Id);
//                if (find == null) throw new Exception("没有找到更新的数据");
//                if (model.StationId == 0) model.StationId = u.StationID;
//                model.sysOpMan = GetCurrentUserInfo().OpManFullMsg;
//                model.sysOpId = GetCurrentUserInfo().OpID;
//                model.sysOpTime = DateTime.Now;
//                MainDbContext.UpdateEntity(model);
//                return Json(new { rlt = true });
//            }
//            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
//        }
//        public async Task<IActionResult> GetJson_DevicesOfStation_Del(int id)
//        {
//            try
//            {
//                await MainDbContext.DeleteEntities<Models.CHIS_Code_WorkStationDevices>(m => m.Id == id);
//                return Json(new { rlt = true });
//            }
//            catch (Exception ex) { return Json(new { rlt = false, msg = ex.Message }); }
//        }

//    }
//}
