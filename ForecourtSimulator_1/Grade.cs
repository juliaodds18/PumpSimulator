using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForecourtSimulator_1
{
    class Grade
    {
        public int ID { get; set; }

        public string name { get; set; }

        public double price { get; set; }

        public Grade(int ID, string name, double price) {
            this.ID = ID;
            this.name = name;
            this.price = price;
        }
    }
}
