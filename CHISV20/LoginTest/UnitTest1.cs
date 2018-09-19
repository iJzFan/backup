using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoginTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var c = new CHIS.Controllers.HomeController();            
           //var u= c.checkUserLogin("zkx", "1234");
           //var role= c.MainDbContext.CHIS_SYS_UserRole.FirstOrDefault(m => m.OpID == u.OpID);
        }

    }
}
