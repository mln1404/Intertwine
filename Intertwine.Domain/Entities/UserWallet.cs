using Intertwine.Domain.Abstractions;
using Intertwine.Domain.Entities;

public class UserWallet : BaseEntity
{
    public int UserWalletId { get; set; }

    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public long CreditBalance { get; set; }

    public byte[] RowVersion { get; set; } = [];
}