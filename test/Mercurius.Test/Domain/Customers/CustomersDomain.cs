using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Mercurius.Test.Model;

namespace Mercurius.Test.Domain.Customers
{
    public class CustomersDomain : MessageHandler
    {
        public override IEnumerable<Type> MessageTypes
        {
            get
            {
                yield return typeof(GetCustomers);
            }
        }

        public override async Task<T> TryGetAsync<T>(IQuery<T> query, IPrincipal principal)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            switch (query)
            {
                case GetCustomers querySubType:
                    return (T) await TryGetAsync().ConfigureAwait(false);
                default:
                    throw new Exception($"A case must be created to handle the query type {query.GetType()}");
            }
        }

        private static Task<IQueryable<Customer>> TryGetAsync()
        {
            return Task.FromResult(new List<Customer> { new Customer() }.AsQueryable());
        }
    }
}