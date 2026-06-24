namespace InfiniteJourney.Global.Shared.Enums;

public enum TenantStatus
{
    Provisioning,
    Pending,
    Active,
    Suspended
}

public enum MembershipStatus
{
    Pending,
    Active,
    Suspended
}

public enum CampaignStatus
{
    Draft,
    Active,
    Suspended,
    Ended
}

public enum DonationStatus
{
    Pending,
    Completed,
    Failed,
    Refunded
}
