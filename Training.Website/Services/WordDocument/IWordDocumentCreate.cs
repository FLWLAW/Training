using Telerik.Windows.Documents.Flow.Model;

namespace Training.Website.Services.WordDocument
{
    public interface IWordDocumentCreate
    {
        Task<RadFlowDocument> Create();
    }
}
