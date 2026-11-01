namespace ProFootball.Core;

/// <summary>Idempotent additive migrations for old saves and new franchises.</summary>
public static class ProFootballSetup
{
    public static ProFranchise Ensure(ProFranchise state) =>
        ProMedicalStaffDepartment.Ensure(
            ProScoutDepartment.Ensure(
                ProTeamStrategyCatalog.Ensure(state)));
}
