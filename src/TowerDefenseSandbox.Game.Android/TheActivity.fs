namespace TowerDefenseSandbox.Game.Android

open Android.App
open Android.Content.PM
open Android.Views

[<Activity(
        Label = "MonogameAndroidSandbox", 
        MainLauncher = true, 
        Icon = "@drawable/icon", 
        Theme = "@style/Theme.Splash", 
        AlwaysRetainTaskState = true, 
        LaunchMode = LaunchMode.SingleInstance, 
        ScreenOrientation = ScreenOrientation.Landscape,
        ConfigurationChanges = (ConfigChanges.Orientation ||| ConfigChanges.Keyboard ||| ConfigChanges.KeyboardHidden ||| ConfigChanges.ScreenSize ||| ConfigChanges.ScreenLayout))>]
type TheActivity() =
    inherit  Microsoft.Xna.Framework.AndroidGameActivity ()

    override this.OnCreate (bundle) =
        base.OnCreate(bundle);
        let game = new TheGame()

        this.SetContentView(game.Services.GetService(typeof<View>) :?> View);

        game.Run();
