﻿namespace Prober.Options;

public class MySqlSettings
{
    public const string SectionName = nameof(MySqlSettings);

    public string User { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string Host { get; set; } = default!;
    public int Port { get; set; } = default!;
    public string Database { get; set; } = default!;
    public string Charset { get; set; } = "utf8";

    public string ConnectionString => $"Server={Host};Database={Database};User={User};Password={Password};Charset={Charset};Port={Port}";
}