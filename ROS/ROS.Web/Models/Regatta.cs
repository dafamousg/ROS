﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ROS.Web.Models
{
    public class Regatta : BaseEntity, IValidatableObject
    {
        public Regatta()
        {

        }

        [Required]
        [MaxLength(50)]
        [DisplayName("Titel")]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        [DisplayName("Beskrivning")]
        public string Description { get; set; }

        [Required]
        [DisplayName("Starttid")]
        public DateTime StartTime { get; set; }

        [Required]
        [DisplayName("Sluttid")]
        public DateTime EndTime { get; set; }

        [Required]
        [DisplayName("Adress")]
        public string Address { get; set; } // This might get changed depending on Google maps API

        public ApplicationUser CreatedBy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime < StartTime)
            {
                yield return new ValidationResult("Sluttiden måste vara efter starttiden.", new[] { "EndTime" });
            }
        }
    }
}