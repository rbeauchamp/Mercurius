using System.Linq;
using Mercurius.Test.Model;

namespace Mercurius.Test.Domain.Customers
{
    public class GetCustomers : Query<IQueryable<Customer>>
    {
    }
}