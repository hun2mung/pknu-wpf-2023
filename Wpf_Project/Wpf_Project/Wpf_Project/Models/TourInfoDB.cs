using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf_Project.Models
{
    public class TourInfoDB
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Area { get; set; }
        public string Phone { get; set; }
        public string Content { get; set; }
        public double Xposition { get; set; }
        public double Yposition { get; set; }
        public string Images { get; set; }
    }
}
