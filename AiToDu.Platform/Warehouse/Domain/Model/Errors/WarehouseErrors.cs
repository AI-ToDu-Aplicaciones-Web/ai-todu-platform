using AiToDu.Platform.Shared.Domain.Model.Errors;
namespace AiToDu.Platform.Warehouse.Domain.Model.Errors;
public static class WarehouseErrors
{
    private static readonly IReadOnlyDictionary<WarehouseErrorCode,string> Messages = new Dictionary<WarehouseErrorCode,string>
    {
        [WarehouseErrorCode.ItemNotFound] = "The inventory item was not found.",
        [WarehouseErrorCode.LotNotFound] = "The stock lot was not found.",
        [WarehouseErrorCode.DuplicateSku] = "This SKU already exists in your organization.",
        [WarehouseErrorCode.InvalidSku] = "A SKU of up to 64 characters is required.",
        [WarehouseErrorCode.InvalidName] = "A product name of up to 160 characters is required.",
        [WarehouseErrorCode.InvalidCategory] = "A category is required.",
        [WarehouseErrorCode.InvalidQuantity] = "Use a nonnegative quantity with at most three decimal places.",
        [WarehouseErrorCode.PositiveQuantityRequired] = "The quantity must be greater than zero.",
        [WarehouseErrorCode.InsufficientStock] = "There is not enough stock for this operation.",
        [WarehouseErrorCode.LotUnavailable] = "This lot is quarantined or expired.",
        [WarehouseErrorCode.VersionConflict] = "The inventory changed. Refresh it before trying again.",
        [WarehouseErrorCode.CountOutdated] = "The stock changed after the count was started.",
        [WarehouseErrorCode.OperationReused] = "This operation reference was already used with different data.",
        [WarehouseErrorCode.OperationIdRequired] = "An operation reference is required.",
        [WarehouseErrorCode.InvalidMovementReason] = "A reason of up to 500 characters is required.",
        [WarehouseErrorCode.InvalidLotCode] = "A lot code is required.",
        [WarehouseErrorCode.InvalidStorageLocation] = "A location is required.",
        [WarehouseErrorCode.ExpiredReceipt] = "The received lot has already expired.",
        [WarehouseErrorCode.LotExpiryConflict] = "This lot and location already have a different expiration date.",
        [WarehouseErrorCode.LotLocationConflict] = "This lot already exists at the selected location.",
        [WarehouseErrorCode.InvalidPrice] = "Use a nonnegative price with at most two decimal places.",
        [WarehouseErrorCode.ZeroAdjustment] = "The adjustment must change the stock.",
        [WarehouseErrorCode.ReturnReferenceRequired] = "Specify a return reference.",
        [WarehouseErrorCode.Forbidden] = "You do not have permission for this operation.",
        [WarehouseErrorCode.PersistenceConflict] = "Another operation conflicts with this request. Refresh and try again.",
        [WarehouseErrorCode.InvalidPagination] = "The page must be positive and the page size must be between 1 and 100.",
        [WarehouseErrorCode.InvalidIdentifier] = "A valid identifier is required.",
        [WarehouseErrorCode.InvalidMovementType] = "Select a supported movement type.",
        [WarehouseErrorCode.InvalidDisposition] = "Select a supported return classification.",
    };
    public static Error From(WarehouseErrorCode code)
    {
        var name = "Warehouse." + code;
        var message = Messages.GetValueOrDefault(code,"The inventory operation could not be completed.");
        return code switch
        {
            WarehouseErrorCode.ItemNotFound or WarehouseErrorCode.LotNotFound => Error.NotFound(name,message),
            WarehouseErrorCode.Forbidden => Error.Forbidden(name,message),
            WarehouseErrorCode.DuplicateSku or WarehouseErrorCode.VersionConflict or WarehouseErrorCode.InsufficientStock or
            WarehouseErrorCode.LotUnavailable or WarehouseErrorCode.CountOutdated or WarehouseErrorCode.OperationReused or
            WarehouseErrorCode.LotExpiryConflict or WarehouseErrorCode.LotLocationConflict or WarehouseErrorCode.PersistenceConflict => Error.Conflict(name,message),
            WarehouseErrorCode.DatabaseError or WarehouseErrorCode.InternalServerError => Error.Unexpected(name,message),
            WarehouseErrorCode.OperationCancelled => Error.Conflict(name,message),
            _ => Error.Validation(name,message)
        };
    }
}
