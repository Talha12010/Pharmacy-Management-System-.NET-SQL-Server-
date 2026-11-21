using PharmacyManagement.Domain.Common;
using PharmacyManagement.Domain.Enums;

namespace PharmacyManagement.Domain.Entities;

public class MedicineBatch : BaseEntity
{
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int InitialQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}

public class InventoryTransaction
{
    public long Id { get; set; }
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int? BatchId { get; set; }
    public MedicineBatch? Batch { get; set; }
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    public InventoryTransactionType Type { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public decimal? UnitCost { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class StockTransfer : BaseEntity
{
    public string TransferNumber { get; set; } = string.Empty;
    public int FromBranchId { get; set; }
    public Branch FromBranch { get; set; } = null!;
    public int ToBranchId { get; set; }
    public Branch ToBranch { get; set; } = null!;
    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;
    public Guid? RequestedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime TransferDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Notes { get; set; }

    public ICollection<StockTransferDetail> Details { get; set; } = new List<StockTransferDetail>();
}

public class StockTransferDetail : BaseEntity
{
    public int StockTransferId { get; set; }
    public StockTransfer StockTransfer { get; set; } = null!;
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public int BatchId { get; set; }
    public MedicineBatch Batch { get; set; } = null!;
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
}
