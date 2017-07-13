using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ForecourtSimulator_1
{
    public class Transaction
    {
        public int ID { get; set; }

        public int GradeID { get; set; }

        public double Amount { get; set; }

        public double Price { get; set;  }

        public double Volume { get; set; }

        public int PumpID { get; set; }

        public Transaction() { }

        public Transaction(int Id, int gradeID, double amount, double price, double volume, int pumpID) {
            this.ID = Id;
            this.GradeID = gradeID;
            this.Amount = amount;
            this.Price = price;
            this.Volume = volume;
            this.PumpID = pumpID; 
        }


    }
}
