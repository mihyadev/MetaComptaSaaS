namespace MetaCompta.Application.DTOs;

public class UnitTypeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? EntityTable { get; set; }
}

public class UnitTakeDto
{
    public int Id { get; set; }
    public int UnitTypeId { get; set; }
    public string UnitTypeCode { get; set; } = string.Empty;
    public string UnitTypeName { get; set; } = string.Empty;
    public int UnitEntityId { get; set; }
    public string TakenByUserId { get; set; } = string.Empty;
    public string? TakenByUserName { get; set; }
    public DateTime TakenAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public int? TaskItemId { get; set; }
    public int? WorkflowStepInstanceId { get; set; }
    public int? DeclarationId { get; set; }
    public bool IsOpen => ClosedAt == null && Status == "Open";
}

public class OpenUnitTakeDto
{
    public string UnitTypeCode { get; set; } = string.Empty;
    public int UnitEntityId { get; set; }
    public string? Comment { get; set; }
    public int? TaskItemId { get; set; }
    public int? WorkflowStepInstanceId { get; set; }
    public int? DeclarationId { get; set; }
    /// <summary>Manager force: close existing open take then open a new one.</summary>
    public bool ForceRelease { get; set; }
    public string? ForceReason { get; set; }
}

public class CloseUnitTakeDto
{
    public string UnitTypeCode { get; set; } = string.Empty;
    public int UnitEntityId { get; set; }
    public string? Comment { get; set; }
    public bool ForceClose { get; set; }
}
