using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.Authentication;

namespace MonitorSaude;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Android.Content.Intent.ActionView },
     Categories = new[] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryBrowsable },
      DataScheme = "com.companyname.monitorsaude")]
public class WebAuthenticatorActivity : WebAuthenticatorCallbackActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
    }
}