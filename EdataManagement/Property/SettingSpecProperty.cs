namespace EdataManagement.Property
{
    /// <summary>
    /// Data carrier for the Spec Setting page (ucSpecSetting): one product + customer.
    /// </summary>
    public class SettingSpecProperty
    {
        public string productName { get; set; }   // 1480 / 980 / 980Dual / 1480VE -> key table suffix
        public string dbProduct { get; set; }     // ddb / 980ldm / ... -> info_cus_condition lives here
        public string customerName { get; set; }
    }
}
