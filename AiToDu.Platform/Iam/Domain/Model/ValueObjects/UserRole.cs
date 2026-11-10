using AiToDu.Platform.Iam.Domain.Model.Errors;
namespace AiToDu.Platform.Iam.Domain.Model.ValueObjects;

public static class UserRole
{
    public const string Owner = "BUSINESS_OWNER";
    public const string WarehouseOperator = "WAREHOUSE_OPERATOR";
    public const string SalesClerk = "SALES_CLERK";

    public static void ValidateStaffRole(string role)
    {
        if (role is not (WarehouseOperator or SalesClerk))
            throw new IamDomainException(IamError.InvalidRole);
    }
}
