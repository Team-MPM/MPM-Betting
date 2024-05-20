﻿using System.ComponentModel.DataAnnotations;

namespace MPM_Betting.DataModel.User;

public class Notification(MpmUser target, string message)
{
    [Key]
    public int Id { get; set; }

    public MpmUser Target { get; set; } = target;

    [StringLength(200)]
    public string Message { get; set; } = message;

    public bool IsRead { get; set; } = false;
    
    public DateTime Date { get; set; } = DateTime.Now;
    
    private Notification() : this(null!, null!) {}
}