using Android;
using Android.App;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace MonitorSaude.Platforms.Android
{
    public class PermissionService
    {
        private readonly Activity _activity;

        private static readonly string[] RequiredPermissions =
        {
            Manifest.Permission.ActivityRecognition,
            Manifest.Permission.AccessFineLocation,
            Manifest.Permission.BodySensors
        };

        private const int RequestCode = 1001;

        public PermissionService(Activity activity)
        {
            _activity = activity;
        }

        public bool HasAllPermissions()
        {
            foreach (var permission in RequiredPermissions)
            {
                if (ContextCompat.CheckSelfPermission(_activity, permission) != Permission.Granted)
                    return false;
            }
            return true;
        }

        public void RequestPermissions()
        {
            ActivityCompat.RequestPermissions(_activity, RequiredPermissions, RequestCode);
        }
    }
}
