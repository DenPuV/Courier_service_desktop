using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courier_service.Models
{
    public class OrderInfo
    {
        public int OrderId { get; set; }
        public int ContactId { get; set; }
        public int PackageId { get; set; }
        public string FName { get; set; }
        public string SName { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
