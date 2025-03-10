using Android.App;
using Android.Content.PM;
using Android.OS;
using MonitorSaude.Platforms.Android;

namespace MonitorSaude;


[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private PermissionService _permissionService;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        _permissionService = new PermissionService(this);

        if (!_permissionService.HasAllPermissions())
        {
            _permissionService.RequestPermissions();
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == 1001)
        {
            bool allGranted = grantResults.All(result => result == Permission.Granted);

            if (allGranted)
            {
                Console.WriteLine("✅ Todas as permissões foram concedidas.");
            }
            else
            {
                Console.WriteLine("⚠ Algumas permissões foram negadas. O app pode não funcionar corretamente.");
            }
        }
    }
}
