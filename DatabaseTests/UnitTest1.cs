using System.Linq;
using DataLayer;
using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var context = new ElibContext(@"C:\Users\luka.budrak\Desktop\new_elib.db");
            Author first = context.Authors.Where(x => true).FirstOrDefault();
            Book mybook = context.Books.Include("Series").Where(x => true).FirstOrDefault();
        }
    }
}
