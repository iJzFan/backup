using Ass;
using CHIS.Models;
using CHIS.Models.DataModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.BllCaller
{
    public class DrugsBllCaller : BaseBllCaller
    {

        public IQueryable<vwCHIS_Code_Drug_Main> GetMyStockDrugInfos(string searchText, List<int> unitIds)
        {
            var db = CHISDbContext;
            searchText = Ass.P.PStr(searchText).ToLower();
            var finds = db.vwCHIS_Code_Drug_Main.AsNoTracking().Where(m => m.SourceFrom == (int)DrugSourceFrom.Local && m.IsEnable == true);

            long drugId = 0;
            if (long.TryParse(searchText, out drugId))
            {
                if (searchText.Length > 10) finds = finds.Where(m => m.BarCode == searchText);
                else finds = finds.Where(m => m.DrugId == drugId);
                if (searchText == "999") finds.Where(m => m.CodeDock.Contains(searchText));
            }
            else
            {
                finds = finds.Where(m => m.CodeDock.Contains(searchText));
            }
            if (unitIds != null && unitIds.Count > 0) finds = finds.Where(m => unitIds.Contains(m.UnitSmallId.Value));

            return finds;
        }

        public async Task<IEnumerable<MyDrugIncomePrice>> GetMyLastIncomePriceAsync(IEnumerable<vwCHIS_Code_Drug_Main> drugs, int stationId)
        {
            var drugids = drugs.Select(m => m.DrugId).ToList();
            if (drugids.Count == 0) return new List<MyDrugIncomePrice>();
            else
            {
                var dal = new DAL.DrugsDAL();
                var tb = await dal.GetMyLastIncomePrice(drugids, stationId);
                return dal.ConvertToList<MyDrugIncomePrice>(tb);
            }
            
        }
        public async Task<IEnumerable<MyDrugIncomePrice>> GetMyLastIncomePriceAsync(IList<int> drugids, int stationId)
        {     
            if (drugids.Count == 0) return new List<MyDrugIncomePrice>();
            else
            {
                var dal = new DAL.DrugsDAL();
                var tb = await dal.GetMyLastIncomePrice(drugids, stationId);
                return dal.ConvertToList<MyDrugIncomePrice>(tb);
            }

        }


    }
}
