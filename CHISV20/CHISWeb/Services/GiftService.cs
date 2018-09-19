using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alipay.AopSdk.Core.Request;
using AutoMapper;
using CHIS.DbContext;
using CHIS.Models;
using CHIS.Models.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace CHIS.Services
{
    public class GiftService : BaseService
    {
        private IMapper _mapper;

        public GiftService(CHISEntitiesSqlServer db, IMapper mapper) : base(db)
        {
            _mapper = mapper;
        }

        public PaginatedItemsViewModel<GiftViewModel> GetGiftList(int index = 1, int pageSize = 10)
        {
            if (index < 0 || pageSize < 0)
            {
                index = 1;
                pageSize = 10;
            }

            var root = _db.CHIS_Code_Gift.Where(x => x.IsDeleted != true && (!x.ExpiryDate.HasValue || x.ExpiryDate.Value > DateTime.Today));

            var count = root.Count();

            var entities = root.OrderByDescending(x => x.NeedPoints).Skip(index - 1).Take(pageSize).ToList();

            var models = _mapper.Map<List<CHIS_Code_Gift>, List<GiftViewModel>>(entities);

            return new PaginatedItemsViewModel<GiftViewModel>(index, pageSize, count, models);
        }

        public GiftViewModel GetGift(int giftId)
        {
            var entity = _db.CHIS_Code_Gift.SingleOrDefault(x => x.GiftId == giftId && x.IsDeleted != true);

            var model = _mapper.Map<CHIS_Code_Gift, GiftViewModel>(entity);

            return model;
        }

        public int PublishGitf(GiftInputModel model)
        {
            var entity = _mapper.Map<GiftInputModel, CHIS_Code_Gift>(model);

            var entityInDb = _db.CHIS_Code_Gift.Add(entity);

            _db.SaveChanges();

            return entityInDb.Entity.GiftId;
        }

        public void UpdateGift(GiftViewModel model)
        {
            var entity = _mapper.Map<GiftViewModel, CHIS_Code_Gift>(model);

            var old = _db.CHIS_Code_Gift.AsNoTracking().SingleOrDefault(x => x.GiftId == model.GiftId);

            entity.UpdateCheck(old);

            _db.CHIS_Code_Gift.Update(entity);

            _db.SaveChanges();
        }

        public void DeleteGift(int giftId)
        {
            var entity = _db.CHIS_Code_Gift.SingleOrDefault(x => x.GiftId == giftId);

            if (entity == null)
            {
                throw new ApplicationException("礼品不存在");
            }

            entity.IsDeleted = true;//TODO:不应该对属性直接赋值

            _db.SaveChanges();
        }

        public PaginatedItemsViewModel<GiftViewModel> SearchGifts(string searchText, string giftStatus, int index, int pageSize)
        {
            if (index < 0 || pageSize < 0)
            {
                index = 1;
                pageSize = 10;
            }


            var root = _db.CHIS_Code_Gift.Where(x => x.IsDeleted != true);

            if (giftStatus == "OutOfStock")
            {
                root = root.Where(x => x.Stock == 0);
            }
            else if(giftStatus == "Expire")
            {
                root = root.Where(x => x.ExpiryDate.HasValue && x.ExpiryDate.Value.Date < DateTime.Today);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                root = root.Where(x => x.GiftName.Contains(searchText));
            }

            var count = root.Count();

            var entities = root.OrderByDescending(x => x.NeedPoints).Skip(index - 1).Take(pageSize).ToList();

            var models = _mapper.Map<List<CHIS_Code_Gift>, List<GiftViewModel>>(entities);

            return new PaginatedItemsViewModel<GiftViewModel>(index, pageSize, count, models);
        }
    }
}
