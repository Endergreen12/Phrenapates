using Microsoft.AspNetCore.Mvc;

namespace Phrenapates.Controllers
{
    [ApiController]
    [Route("/app")]
    public class AppConfigController : ControllerBase
    {
        [Route("client_info")]
        public IResult ClientInfo()
        {
            return Results.Json(new
            {
                result = 0
            });
        }

        [Route("getSettings")]
        public IResult GetSettings()
        {
            Response.ContentType = "application/json";

            return Results.Text(@"{
    ""settings"": {
        ""APP_ANDROIDKEY"": ""MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAhV/4Nvy/YuMRlBncn7Qac70zomF74qWAKIDiTKGiuMrNjIAkD3ijNuhNVYhklV8gVEP5XkAZIGF8SXlIS0SYDwFhE7LwsHtKr42rDj1m9XG9y3CEnOilmfJZ4bFKIBI46a+2EsjDB1fxDdh2PqNvq5oRVU9/KDJKTmBXDSjeHob3UKbQRgSiKdwv4/QQLv1bm3qbQJhPhgGjWZm8/aQIw0hy561dcY82u/ioESeDZjRgnOtL62ufmt9Xq2hecXMmSasqX2dojxdJrL19YXFuCXV+YSe6ElovwCo+Qy9a4bPstjS4BG0VEcL7iw72kJGsQZq48drgz/vZGeH9DbhXmQIDAQAB"",
        ""ADJUST_ENABLED"": 0,
        ""ADJUST_ISDEBUG"": 0,
        ""ADJUST_APPID"": ""lkm6uvfjb75s"",
        ""ADJUST_CHARGEEVENTTOKEN"": ""k6igj0"",
        ""app_gl"": ""ja"",
        ""USER_AGREEMENT"": {
            ""LATEST"": {
                ""version"": ""1.0.0""
            }
        },
        ""app_fire"": 0,
        ""app_debug"": 1,
        ""UserDestroyDays"": 0,
        ""LOG_ENABLE"": 1,
        ""MERGE_TO_4X"": {
            ""WEB_URL"": ""https://sdk-association.yostarplat.com/#/index"",
            ""ONE_KEY_ENABLE"": 1
        },
        ""ADJUST_EVENTTOKENS"": { },
        ""TWITTER_CLIENT_ID"": ""dnlSQzBBZGdQbDZ0czNabF9WQms6MTpjaQ"",
        ""GOOGLE_CLIENT_ID"": ""411167048846-ri57ab445m2gl5nrpoabqvkjirhn6ehg.apps.googleusercontent.com"",
        ""TWITTER_KEY"": ""iywFqxZVJUnHeQe9VuX6LvCii"",
        ""TWITTER_SECRET"": ""iQi4fg2S74iMDlB1LpXebT8Cv7Dn9xrZsKZoaYitx1d7yJSPsa"",
        ""FACEBOOK_APPID"": ""FACEBOOK_APPID.todo"",
        ""FACEBOOK_CLIENTTOKEN"": ""null"",
        ""APP_BIRTHSET_ENABLED"": 0,
        ""GEETEST_ENABLE"": 0,
        ""GEETEST_ID"": ""00b06e0a4ed58bd1c2ad59f1b054ade0"",
        ""GEETEST_FORCECHECK"": 0
    },
    ""EuropeUnion"": false
}");
        }

        [Route("getCode")]
        public IResult GetCode()
        {
            Response.ContentType = "application/json";

            return Results.Text(@"{
    ""result"": 0,
    ""data"": [
        {
            ""codestr"": ""-1"",
            ""codemessage"": ""Unknown Error""
        },
        {
            ""codestr"": ""0"",
            ""codemessage"": ""Success""
        },
        {
            ""codestr"": ""100100"",
            ""codemessage"": ""Device ID is banned""
        },
        {
            ""codestr"": ""100110"",
            ""codemessage"": ""Verification failed""
        },
        {
            ""codestr"": ""100111"",
            ""codemessage"": ""Account creation failed""
        },
        {
            ""codestr"": ""100112"",
            ""codemessage"": ""Account creation success; Account binding failed""
        },
        {
            ""codestr"": ""100113"",
            ""codemessage"": ""Account binding success; Verification failed""
        },
        {
            ""codestr"": ""100114"",
            ""codemessage"": ""IP is restricted during login creation""
        },
        {
            ""codestr"": ""100115"",
            ""codemessage"": ""Device ID is banned during login creation""
        },
        {
            ""codestr"": ""100116"",
            ""codemessage"": ""UID is banned during login creation""
        },
        {
            ""codestr"": ""100117"",
            ""codemessage"": ""Missing parameters""
        },
        {
            ""codestr"": ""100120"",
            ""codemessage"": ""Login failed, IP is restricted""
        },
        {
            ""codestr"": ""100130"",
            ""codemessage"": ""Login failed, UID is banned""
        },
        {
            ""codestr"": ""100140"",
            ""codemessage"": ""Access Token verification failed""
        },
        {
            ""codestr"": ""100150"",
            ""codemessage"": ""UID does not match with Transcode""
        },
        {
            ""codestr"": ""100160"",
            ""codemessage"": ""User birthday has already been added""
        },
        {
            ""codestr"": ""100170"",
            ""codemessage"": ""Invalid birthday format""
        },
        {
            ""codestr"": ""100180"",
            ""codemessage"": ""The third party account is not bound with any game account""
        },
        {
            ""codestr"": ""100190"",
            ""codemessage"": ""Failed to verify the third party parameter""
        },
        {
            ""codestr"": ""100200"",
            ""codemessage"": ""The third party account is already bound with another UID""
        },
        {
            ""codestr"": ""100210"",
            ""codemessage"": ""The third party account does not match with the one bound to this account""
        },
        {
            ""codestr"": ""100211"",
            ""codemessage"": ""Platform binding error""
        },
        {
            ""codestr"": ""100212"",
            ""codemessage"": ""Platform unbinding error""
        },
        {
            ""codestr"": ""100213"",
            ""codemessage"": ""Unable to unbind the account""
        },
        {
            ""codestr"": ""100220"",
            ""codemessage"": ""Authorization canceled""
        },
        {
            ""codestr"": ""100221"",
            ""codemessage"": ""Authorization failed""
        },
        {
            ""codestr"": ""100222"",
            ""codemessage"": ""Authorization failed""
        },
        {
            ""codestr"": ""100223"",
            ""codemessage"": ""Authorization failed""
        },
        {
            ""codestr"": ""100224"",
            ""codemessage"": ""Unable to use Google service""
        },
        {
            ""codestr"": ""100225"",
            ""codemessage"": ""Google authorization was canceled by user""
        },
        {
            ""codestr"": ""100226"",
            ""codemessage"": ""Unable to login during another login request""
        },
        {
            ""codestr"": ""100227"",
            ""codemessage"": ""Failed to login with the current account""
        },
        {
            ""codestr"": ""100228"",
            ""codemessage"": ""The account has been deleted. Log in failed.""
        },
        {
            ""codestr"": ""100229"",
            ""codemessage"": ""Account deletion cannot be performed repeatedly""
        },
        {
            ""codestr"": ""100231"",
            ""codemessage"": ""Failed to restore, the account was deleted completely""
        },
        {
            ""codestr"": ""100232"",
            ""codemessage"": ""Unable to restore, no deletion history of the account""
        },
        {
            ""codestr"": ""100233"",
            ""codemessage"": ""The account has been in the cooling-off period for account  deletion now.""
        },
        {
            ""codestr"": ""100234"",
            ""codemessage"": ""The account is not authorized to login""
        },
        {
            ""codestr"": ""100230"",
            ""codemessage"": ""Initialization failed""
        },
        {
            ""codestr"": ""100240"",
            ""codemessage"": ""Apple authorization information does not match""
        },
        {
            ""codestr"": ""100241"",
            ""codemessage"": ""User cancelled Apple authorization request""
        },
        {
            ""codestr"": ""100242"",
            ""codemessage"": ""Apple authorization request failed""
        },
        {
            ""codestr"": ""100243"",
            ""codemessage"": ""Invalid response from Apple authorization request""
        },
        {
            ""codestr"": ""100244"",
            ""codemessage"": ""Failed to process Apple authorization request""
        },
        {
            ""codestr"": ""100245"",
            ""codemessage"": ""Apple authorization request failed for unknown reason""
        },
        {
            ""codestr"": ""100246"",
            ""codemessage"": ""System version does not support Apple authorization""
        },
        {
            ""codestr"": ""100300"",
            ""codemessage"": ""Invalid email address format""
        },
        {
            ""codestr"": ""100301"",
            ""codemessage"": ""Email addresses do not match""
        },
        {
            ""codestr"": ""100302"",
            ""codemessage"": ""Verification code request is too frequent""
        },
        {
            ""codestr"": ""100303"",
            ""codemessage"": ""Verification failed""
        },
        {
            ""codestr"": ""100304"",
            ""codemessage"": ""Verification failed too many times""
        },
        {
            ""codestr"": ""100305"",
            ""codemessage"": ""The account is banned""
        },
        {
            ""codestr"": ""100306"",
            ""codemessage"": ""Verification code cannot be empty""
        },
        {
            ""codestr"": ""100404"",
            ""codemessage"": ""Network error""
        },
        {
            ""codestr"": ""200100"",
            ""codemessage"": ""User birthday is required""
        },
        {
            ""codestr"": ""200110"",
            ""codemessage"": ""Monthly purchase limit exceeded""
        },
        {
            ""codestr"": ""200120"",
            ""codemessage"": ""Order creation failed: Item is not configured in SDK management platform""
        },
        {
            ""codestr"": ""200130"",
            ""codemessage"": ""Payment method does not exist""
        },
        {
            ""codestr"": ""200140"",
            ""codemessage"": ""serverTag does not exist""
        },
        {
            ""codestr"": ""200150"",
            ""codemessage"": ""Payment receipt verification failed""
        },
        {
            ""codestr"": ""200160"",
            ""codemessage"": ""Invalid purchase request""
        },
        {
            ""codestr"": ""200170"",
            ""codemessage"": ""Purchase request failed on game server""
        },
        {
            ""codestr"": ""200180"",
            ""codemessage"": ""It takes a long time for searching the purchase result""
        },
        {
            ""codestr"": ""200190"",
            ""codemessage"": ""Order ID does not exist""
        },
        {
            ""codestr"": ""200200"",
            ""codemessage"": ""Order status tracking timed out""
        },
        {
            ""codestr"": ""200210"",
            ""codemessage"": ""productid does not exist on payment backend""
        },
        {
            ""codestr"": ""200220"",
            ""codemessage"": ""Payment backend response - payment failed""
        },
        {
            ""codestr"": ""200230"",
            ""codemessage"": ""Payment backend response - payment canceled""
        },
        {
            ""codestr"": ""200240"",
            ""codemessage"": ""Current API version does not support the request""
        },
        {
            ""codestr"": ""200250"",
            ""codemessage"": ""Invalid parameters provided to API""
        },
        {
            ""codestr"": ""200260"",
            ""codemessage"": ""Fatal error during API operation""
        },
        {
            ""codestr"": ""200270"",
            ""codemessage"": ""The request is not supported by the Play store on current device""
        },
        {
            ""codestr"": ""200280"",
            ""codemessage"": ""Item has already been purchased, not consumed yet""
        },
        {
            ""codestr"": ""200290"",
            ""codemessage"": ""Item has already been purchased, failed to consume""
        },
        {
            ""codestr"": ""200300"",
            ""codemessage"": ""Unable to purchase the requested item""
        },
        {
            ""codestr"": ""200310"",
            ""codemessage"": ""Unable to connect to Google Play service""
        },
        {
            ""codestr"": ""200320"",
            ""codemessage"": ""Request reached maximum timeout before receiving any response from Google Play""
        },
        {
            ""codestr"": ""200330"",
            ""codemessage"": ""Network connection is turned off""
        },
        {
            ""codestr"": ""200340"",
            ""codemessage"": ""Payment canceled by user""
        },
        {
            ""codestr"": ""200350"",
            ""codemessage"": ""Order creation failed: Item doesn't exist at store""
        },
        {
            ""codestr"": ""200360"",
            ""codemessage"": ""Connection to play services failed""
        },
        {
            ""codestr"": ""300100"",
            ""codemessage"": ""System sharing failed""
        },
        {
            ""codestr"": ""200390"",
            ""codemessage"": ""Payment backend response - payment failed""
        },
        {
            ""codestr"": ""100205"",
            ""codemessage"": ""Unbind failed. Your game account hasn't been linked to a Google Play Games account.""
        },
        {
            ""codestr"": ""100214"",
            ""codemessage"": ""The game account now is linked to a Google Play Games account. If you unlink it from other accounts (except the Google Play Games account), it will also unlink from the Google Play Games account automatically.""
        },
        {
            ""codestr"": ""100206"",
            ""codemessage"": ""The non-guest account has been linked to a Google Play Games account automatically. You can log on another device with the Google Play Games account.""
        },
        {
            ""codestr"": ""100201"",
            ""codemessage"": ""The Google Play Games account has been linked to another game account. Confirm to unlink it from the old game account and re-link to the current one?""
        },
        {
            ""codestr"": ""100202"",
            ""codemessage"": ""The game account has been linked to another Google Play Games account. Confirm to unlink it from the old Google Play Games account and re-link to the current one?""
        },
        {
            ""codestr"": ""100203"",
            ""codemessage"": ""The current game account and Google Play Games account have each been linked to other accounts. Confirm to unlink the both from the old accounts and re-link them together?""
        },
        {
            ""codestr"": ""100204"",
            ""codemessage"": ""The guest account can't be linked to a Google Play Games account so you can't log on another device with the guest account. If you want to link the guest account to a Google Play Games account, please link it to a Yostar account/third-party account first.""
        },
        {
            ""codestr"": ""100803"",
            ""codemessage"": ""Callback URL exceeds its maximum characters""
        },
        {
            ""codestr"": ""200232"",
            ""codemessage"": ""Apple Pay is not available on this device""
        },
        {
            ""codestr"": ""200141"",
            ""codemessage"": ""Order creation failed: Purchase currency is not supported""
        },
        {
            ""codestr"": ""100215"",
            ""codemessage"": ""Unable to bound Twitter account in current App""
        },
        {
            ""codestr"": ""100216"",
            ""codemessage"": ""Unable to switch Twitter account in current App""
        },
        {
            ""codestr"": ""100217"",
            ""codemessage"": ""Unable to register Twitter account in current App""
        },
        {
            ""codestr"": ""100218"",
            ""codemessage"": ""Unable to login with Twitter account in current App""
        },
        {
            ""codestr"": ""100600"",
            ""codemessage"": ""Interface parameter error. Invoke failed""
        }
    ]
}");
        }
    }
}
