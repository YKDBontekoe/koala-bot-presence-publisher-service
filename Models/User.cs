﻿namespace Koala.ActivityGameHandlerService.Models;

public class User
{
    public ulong Id { get; set; }
    public string Username { get; set; }
    
    public IEnumerable<Guild> Guilds { get; set; }
    public string? NickName { get; set; }
}