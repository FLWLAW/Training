using Microsoft.Identity.Client;

namespace Training.Website.Models
{
    public class ResultsModel_Parameters
    {
        public required int? Session_ID { get; set; }
        public required int? OPS_User_ID { get; set; }
        public required int? CMS_User_ID { get; set; }
    }
}
