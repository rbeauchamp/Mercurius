using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Mercurius.Test.Domain.Orders;
using Mercurius.Validations;

namespace Mercurius.Test.Domain.Customers
{
    public class UpdateCustomer : Command
    {

        /// <summary>
        /// Categories to update
        /// </summary>
        [Required]
        [ValidateObject]
        public ICollection<CreateOrder> Categories { get; } = new List<CreateOrder>();
    }
}