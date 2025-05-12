using DomashneeZadanie;
using Otus.ToDoList.ConsoleBot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomashneZadanie
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var t = new UserService();
            var v = new ToDoService();
            var botClient = new ConsoleBotClient();
            var handler = new UpdateHandler(t, v);

            botClient.StartReceiving(handler);


        }

    }
}