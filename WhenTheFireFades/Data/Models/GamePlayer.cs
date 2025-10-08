using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WhenTheFireFades.Data.Models;

public partial class GamePlayer
{
    [Key]
    public int GamePlayerId { get; set; }

    public int GameId { get; set; }

    public int UserId { get; set; }

    public int Seat { get; set; }

    public bool IsReady { get; set; }

    public bool IsConnected { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [ForeignKey("GameId")]
    [InverseProperty("GamePlayers")]
    public virtual Game Game { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("GamePlayers")]
    public virtual User User { get; set; } = null!;
}
