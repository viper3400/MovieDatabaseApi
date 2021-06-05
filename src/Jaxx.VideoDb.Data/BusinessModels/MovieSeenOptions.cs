using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Jaxx.VideoDb.Data.BusinessModels
{
    /// <summary>
    /// Model class for handle options when setting a movie seen/unseen
    /// </summary>
    public class MovieSeenOptions
    {
        /// <summary>
        /// The id of the movie to set seen / unseen
        /// </summary>
        [Required]
        public int Id { get; set; }
        [Required]
        public string Date { get; set; }
    }
}
