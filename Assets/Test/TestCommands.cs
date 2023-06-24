using System.Threading.Tasks;
using Ametrin.Console;
using Ametrin.Console.Command;
using UnityEngine;

public sealed class TestCommands : MonoBehaviour{

    static TestCommands(){
        ConsoleManager.RegisterHandler('/', new ConsoleCommandHandler());
        CommandManager.RegisterCommands<TestCommands>();
    }

    [Command("add")]
    public static void Add(int left, int right){
        ConsoleManager.AddMessage($"{left} + {right} is {left+right}");
    }
    
    [Command("wait")]
    public static async Task Test(float seconds){
        await Task.Delay((int)(seconds*1000));
        ConsoleManager.AddMessage($"Waited for {seconds}s");
    }
}