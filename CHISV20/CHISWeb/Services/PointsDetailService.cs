using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CHIS.DbContext;
using CHIS.Models;
using CHIS.Models.InputModel;
using CHIS.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Services
{
    public class PointsDetailService : BaseService
    {
        private IMapper _mapper;

        public PointsDetailService(CHISEntitiesSqlServer db, IMapper mapper) : base(db)
        {
            _mapper = mapper;
        }

        public PaginatedItemsViewModel<PointsDetailViewModel> GetPointsDetailList(int customerId, int index = 1, int pageSize = 10)
        {
            if (index < 0 || pageSize < 0)
            {
                index = 1;
                pageSize = 10;
            }

            var root = _db.CHIS_Customer_PointsDetail.Where(x=>x.CustomerId == customerId);

            var count = root.Count();

            var entities = root.OrderByDescending(x => x.Id).Skip(index - 1).Take(pageSize).ToList();


            var models = _mapper.Map<List<CHIS_Customer_PointsDetail>, List<PointsDetailViewModel>>(entities);

            return new PaginatedItemsViewModel<PointsDetailViewModel>(index, pageSize, count, models);
        }

        public void ChangePoints(PointsDetailInputModel model)
        {
            var detail = _mapper.Map<PointsDetailInputModel, CHIS_Customer_PointsDetail>(model);

            var customer = _db.CHIS_Code_Customer.SingleOrDefault(x => x.CustomerID == model.CustomerId);

            customer.ChangePoints(detail);

            _db.SaveChanges();

        }

        /// <summary>
        /// 根据积分规则修改积分
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="pointsRuleId">积分规则Id</param>
        /// <param name="consumerMoney">消费金额</param>
        public void ChangePoints(int customerId,int pointsRuleId,decimal? consumerMoney)
        {
            var detail = new CHIS_Customer_PointsDetail
            {
                CustomerId = customerId,
                CreatedTime = DateTime.Now
            };

            var pointsRule = _db.CHIS_Customer_PointsDetail_Rule.SingleOrDefault(x=>x.PointsRuleId == pointsRuleId);

            //detail.PointsRule = pointsRule ?? throw new ApplicationException("积分规则错误！");

            detail.CreatePointSDetail(pointsRule, consumerMoney);

            _db.CHIS_Customer_PointsDetail.Add(detail);

            _db.SaveChanges();

        }

        public long CurrentPoints(int customerId)
        {
            var entity = _db.CHIS_Code_Customer.SingleOrDefault(x => x.CustomerID == customerId);

            if(entity == null || entity.Points == null)
            {
                return 0;
            }
            else
            {
                return entity.Points.Value;
            }
        }
    }
}
