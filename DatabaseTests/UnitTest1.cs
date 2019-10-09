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
            ElibContext context = new ElibContext(@"C:\Users\luka.budrak\Desktop\new_elib-test.db");
            Author first = context.Authors.Include("Books").FirstOrDefault();
            UserCollection collection = context.UserCollections.Include("Books").FirstOrDefault();
            Book mybook = context.Books
                .Include("Series")
                .Include("Authors")
                .Include("Files")
                .Include("Quotes")
                .FirstOrDefault();
        }
    }
}
