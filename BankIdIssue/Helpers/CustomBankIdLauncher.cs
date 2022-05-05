using ActiveLogin.Authentication.BankId.AspNetCore.Launcher;
using ActiveLogin.Authentication.BankId.AspNetCore.SupportedDevice;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text;

namespace BankIdIssue.Helpers;
public class CustomBankIdLauncher : IBankIdLauncher
{
    private const string UserAgentHeaderName = "User-Agent";

    private const string BankIdSchemePrefix = "bankid:///";
    private const string BankIdAppLinkPrefix = "https://app.bankid.com/";

    private const string NullRedirectUrl = "null";

    private const string IosChromeScheme = "googlechromes://";
    private const string IosFirefoxScheme = "firefox://";

    private readonly IBankIdSupportedDeviceDetector _bankIdSupportedDeviceDetector;

    public CustomBankIdLauncher(IBankIdSupportedDeviceDetector bankIdSupportedDeviceDetector)
    {
        _bankIdSupportedDeviceDetector = bankIdSupportedDeviceDetector;
    }

    public BankIdLaunchInfo GetLaunchInfo(LaunchUrlRequest request, HttpContext httpContext)
    {
        var detectedDevice = _bankIdSupportedDeviceDetector.Detect(httpContext.Request.Headers[UserAgentHeaderName]);
        var deviceMightRequireUserInteractionToLaunch = GetDeviceMightRequireUserInteractionToLaunchBankIdApp(detectedDevice);
        var deviceWillReloadPageOnReturn = GetDeviceWillReloadPageOnReturnFromBankIdApp(detectedDevice);

        var launchUrl = GetLaunchUrl(detectedDevice, request);
        return new BankIdLaunchInfo(launchUrl, deviceMightRequireUserInteractionToLaunch, deviceWillReloadPageOnReturn);
    }

    private bool GetDeviceMightRequireUserInteractionToLaunchBankIdApp(BankIdSupportedDevice detectedDevice)
    {
        return
            detectedDevice.DeviceBrowser == BankIdSupportedDeviceBrowser.Safari ||
             (detectedDevice.DeviceOs == BankIdSupportedDeviceOs.Android
                   && detectedDevice.DeviceBrowser != BankIdSupportedDeviceBrowser.Firefox
                   && detectedDevice.DeviceBrowser != BankIdSupportedDeviceBrowser.Opera
             );
    }

    private bool GetDeviceWillReloadPageOnReturnFromBankIdApp(BankIdSupportedDevice detectedDevice)
    {
        //When returned from the BankID app Safari on iOS will refresh the page / tab.
        return detectedDevice.DeviceOs == BankIdSupportedDeviceOs.Ios
               && detectedDevice.DeviceBrowser == BankIdSupportedDeviceBrowser.Safari;
    }

    private string GetLaunchUrl(BankIdSupportedDevice device, LaunchUrlRequest request)
    {
        var prefix = GetPrefixPart(device);
        var queryString = GetQueryStringPart(device, request);

        return $"{prefix}{queryString}";
    }

    private string GetPrefixPart(BankIdSupportedDevice device)
    {
        return CanUseAppLink(device)
            ? BankIdAppLinkPrefix
            : BankIdSchemePrefix;
    }

    private static bool CanUseAppLink(BankIdSupportedDevice device)
    {
        // Only Safari on IOS and Chrome or Edge on Android version >= 6 seems to support
        //  the https://app.bankid.com/ launch url

        return IsSafariOnIos(device)
               || IsChromeOrEdgeOnAndroid6OrGreater(device);
    }

    private static bool IsSafariOnIos(BankIdSupportedDevice device)
    {
        return device.DeviceOs == BankIdSupportedDeviceOs.Ios
               && device.DeviceBrowser == BankIdSupportedDeviceBrowser.Safari;
    }

    private static bool IsChromeOrEdgeOnAndroid6OrGreater(BankIdSupportedDevice device)
    {
        return device.DeviceOs == BankIdSupportedDeviceOs.Android
               && device.DeviceOsVersion.MajorVersion >= 6
               && (
                   device.DeviceBrowser == BankIdSupportedDeviceBrowser.Chrome
                   || device.DeviceBrowser == BankIdSupportedDeviceBrowser.Edge
                );
    }

    private string GetQueryStringPart(BankIdSupportedDevice device, LaunchUrlRequest request)
    {
        var queryStringParams = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(request.AutoStartToken))
        {
            queryStringParams.Add("autostarttoken", request.AutoStartToken);
        }

        if (!string.IsNullOrWhiteSpace(request.RelyingPartyReference))
        {
            queryStringParams.Add("rpref", Base64Encode(request.RelyingPartyReference));
        }

        queryStringParams.Add("redirect", GetRedirectUrl(device, request));

        return GetQueryString(queryStringParams);
    }

    private static string GetRedirectUrl(BankIdSupportedDevice device, LaunchUrlRequest request) => NullRedirectUrl;

    private static string Base64Encode(string value)
    {
        var encodedBytes = Encoding.Unicode.GetBytes(value);
        return Convert.ToBase64String(encodedBytes);
    }

    private static string GetQueryString(Dictionary<string, string> queryStringParams)
    {
        if (!queryStringParams.Any())
        {
            return string.Empty;
        }

        var queryBuilder = new QueryBuilder(queryStringParams);
        return queryBuilder.ToQueryString().ToString();
    }
}

