using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _TestProj1
{
    internal class InsertQuestion_Parameters
    {
        public required int Training_SESSION_ID { get; set; }
        public required string Question { get; set; }
        public required int AnswerFormat_ID { get; set; }
        public required int CreatedBy_ID { get; set; }
    }
}
