using ApiApplication.Database.Entities;
using ApiApplication.DTOs.MovieDTOs;
using System;

namespace ApiApplication.DTOs.ShowtimeDTOs
{
    public class ShowtimeDTO
    {
        public int Id { get; set; }
        public MovieDTO Movie { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }
}
