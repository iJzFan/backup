using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS.Models.DataModels
{
    public class DrugInfo
    {
        /// <summary>
        /// 产品编码
        /// </summary>
        public string productCode { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string productName { get; set; }
        /// <summary>
        /// 主产品编码
        /// </summary>
        public string mainProductCode { get; set; }
        public int productStatusType { get; set; }
        /// <summary>
        /// 市场价
        /// </summary>
        public int marketPrice { get; set; }
        public decimal marketPriceM
        {
            get
            {
                return ((marketPrice * 1.0m) / 100m);
            }
        }
        /// <summary>
        /// 健客价
        /// </summary>
        public int ourPrice { get; set; }

        /// <summary>
        /// Decimal格式圆角分0.00
        /// </summary>
        public decimal ourPriceM
        {
            get
            {
                return ((ourPrice * 1.0m) / 100m);
            }
        }
        public string manufacturer { get; set; }
        /// <summary>
        /// 处方类型 0:空 1:其它 2:红OTC 3:绿OTC 4:处方药 5:管制处方药 9:非药品
        /// </summary>
        public string prescriptionType { get; set; }
        /// <summary>
        /// 库存
        /// </summary>
        public int productInventory { get; set; }
        /// <summary>
        /// 产品属性 1:新药特药 2:进口药品 3:基本药品 4:麻黄碱类 5:电子监管 6:委托加工
            ///7:外用 8:药品 9:中药保护品种 10:特殊管理药品 11:运动员慎用药品
        /// </summary>
        public string productAttribute { get; set; }
        /// <summary>
        /// 药品图片
        /// </summary>
        public string thumbnailUrl { get; set; }
        /// <summary>
        /// 产品详情
        /// </summary>
        public string introduction { get; set; }
        /// <summary>
        /// 药品规格
        /// </summary>
        public string packing { get; set; }
    }
}
