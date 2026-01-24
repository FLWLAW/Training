using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdHocBatchProcessingProgram01
{
    internal class InputFileClass
    {
        public required string? FullName { get; set; }
        public required string? FirstName { get; set; }
        public required string? LastName { get; set; }
        public required string? FullSupervisorName { get; set; }
        public required string? AnswerToQuestion5 { get; set; }
    }
}
