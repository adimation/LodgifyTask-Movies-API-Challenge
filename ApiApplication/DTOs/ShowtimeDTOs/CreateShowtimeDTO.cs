using System;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.DTOs.ShowtimeDTOs
{
    public class CreateShowtimeDTO
    {
        [Required(ErrorMessage = "ImdbId is required.")]
        public string ImdbId { get; set; }

        [Required(ErrorMessage = "AuditoriumId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "AuditoriumId must be greater than 0.")]
        public int AuditoriumId { get; set; }

        [Required(ErrorMessage = "SessionDate is required.")]
        public DateTime SessionDate { get; set; }
    }
}
