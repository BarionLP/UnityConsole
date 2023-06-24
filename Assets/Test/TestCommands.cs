using System.Threading.Tasks;
using Ametrin.Console;
using Ametrin.Console.Command;
using UnityEngine;

public sealed class TestCommands : MonoBehaviour{

    static TestCommands(){
        ConsoleCommandHandler.RegisterCommands<TestCommands>();
    }

    [Command("add")]
    public static void Add(int a, int b){
        ConsoleManager.AddMessage($"{a} + {b} is {a+b}");
    }
    
    [Command("test")]
    public static async Task Test(float seconds){
        await Task.Delay((int)(seconds*1000));
        ConsoleManager.AddMessage($"Waited for {seconds:2F}s");
    }
}