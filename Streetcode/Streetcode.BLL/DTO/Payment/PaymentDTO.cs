﻿using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.Dto.Payment
{
    public class PaymentDto
    {
        [Required]
        public long Amount { get; set; }

        public string? RedirectUrl { get; set; }
    }
}
