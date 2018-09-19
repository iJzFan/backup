using System;

namespace CHIS.WebSocketService.Models
{
    public class PaySetModel
    {
        public int DoctorId { get; set; }
        public int StationId { get; set; }
        public string payOrderId { get; set; }

        /// <summary>
        /// ªÒ»°º”√‹¬Î
        /// </summary>
        /// <returns></returns>
        public string GetEncriptCode()
        { 
            return Ass.Data.Secret.Encript($"{StationId}|{DoctorId}|{payOrderId}", Global.SYS_ENCRIPT_PWD);
        }
        public static PaySetModel DecriptPayCode(string code)
        { 
            string[] a = Ass.Data.Secret.Encript(code, Global.SYS_ENCRIPT_PWD).Split('|');
            return new PaySetModel
            {
                StationId = Ass.P.PInt(a[0]),
                DoctorId = Ass.P.PInt(a[1]),
                payOrderId = a[2]
            };
        }
    }
}