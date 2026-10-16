using MetaCompta.Application.DTOs;

namespace MetaCompta.Application.Interfaces;

public interface IDomiciliationService
{
    Task<ApiResponse<List<DomiciliationDashboardRowDto>>> GetDashboardAsync(int cabinetId);
    Task<ApiResponse<DomiciliationClientWorkspaceDto>> GetClientWorkspaceAsync(int clientId, int cabinetId);
    Task<ApiResponse<List<DomiciliationTimelineDto>>> GetTimelineAsync(int clientId, int cabinetId, int take = 50);

    Task<ApiResponse<CourrierDomiciliationDto>> ReceiveCourrierAsync(CourrierReceiveDto dto, int cabinetId, string userId, string userName);
    Task<ApiResponse<CourrierDomiciliationDto>> GetCourrierAsync(int id, int cabinetId);
    Task<ApiResponse<CourrierDomiciliationDto>> AdvanceCourrierAsync(int id, CourrierStatusActionDto dto, int cabinetId, string userId, string userName);
    Task<ApiResponse<CourrierDomiciliationDto>> AttachScanAsync(int id, int documentId, int cabinetId, string userId, string userName);
    Task<ApiResponse<CourrierDomiciliationDto>> RemovePieceAsync(int id, int pieceId, int cabinetId, string userId, string userName);
    Task<ApiResponse<CourrierDomiciliationDto>> SetPrimaryPieceAsync(int id, int pieceId, int cabinetId, string userId, string userName);

    Task<ApiResponse<DemandeActionCourrierDto>> CreateClientActionAsync(int courrierId, DemandeActionCreateDto dto, int cabinetId, string clientUserId, int clientId);
    Task<ApiResponse<DemandeActionCourrierDto>> AdvanceClientActionAsync(int actionId, DemandeActionAdvanceDto dto, int cabinetId, string userId, string userName);

    Task<ApiResponse<DemandeDocumentDomiciliationDto>> CreateDocumentRequestAsync(DemandeDocumentCreateDto dto, int cabinetId, string userId, string userName);
    Task<ApiResponse<DemandeDocumentDomiciliationDto>> ReviewDocumentRequestAsync(int id, DemandeDocumentReviewDto dto, int cabinetId, string userId, string userName);
    Task<ApiResponse<DemandeDocumentDomiciliationDto>> AttachDocumentVersionAsync(int demandeId, int documentId, int cabinetId, string userId, string userName, bool fromClient);

    Task<ApiResponse<List<CourrierDomiciliationDto>>> GetClientNotifiedCourriersAsync(int clientId, int cabinetId);
    Task<ApiResponse<List<DemandeDocumentDomiciliationDto>>> GetClientDocumentRequestsAsync(int clientId, int cabinetId);
}
