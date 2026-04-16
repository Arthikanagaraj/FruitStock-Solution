public class DashboardFruitVM
{
    public string FruitName { get; set; }

    public int Inlet { get; set; }
    public int Outlet { get; set; }

    public int Remaining => Inlet - Outlet;
}
