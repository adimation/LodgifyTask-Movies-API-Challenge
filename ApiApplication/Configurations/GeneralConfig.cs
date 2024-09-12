namespace ApiApplication.Configurations
{
    public class GeneralConfig
    {
        private string ReservationExpiryInMinutes { get; set; }

        public int GetReservationExpiryInMinutes()
        {
            int.TryParse(ReservationExpiryInMinutes, out int reservationExpiryInMinutes);
            if (reservationExpiryInMinutes == 0) return 10;
            return reservationExpiryInMinutes;
        }
    }
}
