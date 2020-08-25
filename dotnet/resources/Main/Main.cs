using System;
using GTANetworkAPI;
//using Microsoft.EntityFrameworkCore;
using Npgsql;
//using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace RAGE_Cargo
{
    public class Main : Script
    {
        public Main()
        {
            Console.WriteLine("RAGE Cargo!");
            var connString = "Host=localhost;Username=postgres;Password=database;Database=RAGE:Cargo";
            var con = new NpgsqlConnection(connString);
            con.Open();
            var sql = "SELECT version()";
            using var cmd = new NpgsqlCommand(sql, con);
            var version = cmd.ExecuteScalar().ToString();
            Console.WriteLine($"PostgreSQL version: {version}");
            con.Close();
        }
        class Database : Script
        {
            public static string connString = "Host=localhost;Username=postgres;Password=database;Database=RAGE:Cargo";

        }
        // public void OnResourceStart(){
        //     var connString = "Host=localhost;Username=postgres;Password=database;Database=RAGE:Cargo";
        //     using var con = new NpgsqlConnection(connString);
        //     con.Open();
        //     var sql = "SELECT version()";
        //     using var cmd = new NpgsqlCommand(sql, con);
        //     var version = cmd.ExecuteScalar().ToString();
        //     Console.WriteLine($"PostgreSQL version: {version}");
        // }
         [Command("hi")]
            public void SayHi (Player player)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Hello World!");
            }
        [ServerEvent(Event.PlayerConnected)]
            public void OnPlayerConnected(Player player)
            {
                player.SendChatMessage($"Welcome to RAGE:CARGO, {player.SocialClubName}. If this is your first time here, use /register [pin] [confirmPin] to create your account. If not, you can use /login.");
                //player.Name = player.SocialClubName;
                player.Name = player.SocialClubName + "[U]";
            }
        [Command("register", "USAGE: /register [pin] [confirmPin]")]
            public void sqltest (Player player, int arg1, int arg2)
            {
                if (arg1 == arg2)
                {
                    using (var con = new NpgsqlConnection(Database.connString))
                    {                                                                           // was just @p, did this @p, @d, still column p does not exist
                        using (var search = new NpgsqlCommand("SELECT * FROM accounts WHERE accname = (@a)", con))
                        {
                            //bool hasAccount = true;
                            con.Open();
                            search.Parameters.AddWithValue("a", $"{player.SocialClubName}");
                            using NpgsqlDataReader rdr = search.ExecuteReader();
                            while (rdr.Read())
                            {
                                if (rdr.GetInt32(0) > 0)
                                {
                                    //hasAccount = true;
                                    player.SendChatMessage("You already have an account!");
                                    con.Close();
                                }
                                else
                                {
                                    //con.Close();
                                    //hasAccount = false;
                                    using (var cmd = new NpgsqlCommand("INSERT INTO accounts(accname, pin) VALUES (@p, @d)", con))
                                    {
                                        //con.Open();
                                        cmd.Parameters.AddWithValue("p", $"{player.SocialClubName}");
                                        cmd.Parameters.AddWithValue("d", arg1);
                                        int a = cmd.ExecuteNonQuery();
                                        con.Close();
                                        if (a==0)
                                        {
                                            player.SendChatMessage("Error!");
                                        }
                                        else
                                        {
                                            player.SendChatMessage("You have successfully created an account!");
                                        }
                                    }
                                }
                            }
                            //con.Close();
                        }
                        // using (var cmd = new NpgsqlCommand("INSERT INTO accounts(accname, pin) VALUES (@p, @d)", con))
                        // {
                        //     con.Open();
                        //     cmd.Parameters.AddWithValue("p", $"{player.SocialClubName}");
                        //     cmd.Parameters.AddWithValue("d", arg1);
                        //     int a = cmd.ExecuteNonQuery();
                        //     con.Close();
                        //     if (a==0)
                        //     {
                        //         player.SendChatMessage("Error!");
                        //     }
                        //     else
                        //     {
                        //         player.SendChatMessage("You have successfully created an account!");
                        //     }
                        // }
                    }
                    
                }
                else
                {
                    player.SendChatMessage("Those pins are not the same!");
                }
            }

            [Command("login", "USAGE: /login [pin]")]
            public void login (Player player, int arg1)
            {
                using (var con = new NpgsqlConnection(Database.connString))
                {
                    using (var search = new NpgsqlCommand("SELECT * FROM accounts WHERE accname = (@a) AND pin = (@b)", con))
                    {
                        con.Open();
                        search.Parameters.AddWithValue("a", $"{player.SocialClubName}");
                        search.Parameters.AddWithValue("b", arg1);
                        using NpgsqlDataReader rdr = search.ExecuteReader();
                        while (rdr.Read())
                        {
                            if (rdr.GetInt32(2) == arg1)
                            {
                                player.Name = player.SocialClubName;
                                player.SendChatMessage("You have logged into your account!");
                            }
                            else //if (rdr.GetInt32(2) != arg1)
                            {
                                player.SendChatMessage("That is not the correct pin!");
                            }
                        }
                    }
                }
            }
    }
}
