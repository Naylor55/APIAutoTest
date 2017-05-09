using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace BKS_Sys.App.Models
{
    public class JpushModels
    {
    }

    class Base64
    {
        public static String getBase64Encode(String str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            //
            return Convert.ToBase64String(bytes);
        }
    }

    public abstract class BaseResult
    {
        public const int ERROR_CODE_NONE = -1;
        public const int ERROR_CODE_OK = 0;
        public const String ERROR_MESSAGE_NONE = "None error message.";

        public const int RESPONSE_OK = 200;

        private ResponseWrapper responseResult;

        public ResponseWrapper ResponseResult
        {
            get { return responseResult; }
            set { responseResult = value; }
        }
        public abstract bool isResultOK();



        // public override String getErrorMessage();

        public int getRateLimitQuota()
        {
            if (null != responseResult)
            {
                return responseResult.rateLimitQuota;
            }
            return 0;
        }

        public int getRateLimitRemaining()
        {
            if (null != responseResult)
            {
                return responseResult.rateLimitRemaining;
            }
            return 0;
        }

        public int getRateLimitReset()
        {
            if (null != responseResult)
            {
                return responseResult.rateLimitReset;
            }
            return 0;
        }
    }

    public class MessageResult : BaseResult
    {
        public long msg_id { get; set; }
        public long sendno { get; set; }

        override public bool isResultOK()
        {
            if (Equals(ResponseResult.responseCode, HttpStatusCode.OK))
            {
                return true;
            }
            return false;
        }
        public override string ToString()
        {
            return string.Format("sendno:{0},message_id:{1}", sendno, msg_id);
        }
    }
    class Preconditions
    {
        public static void checkArgument(bool expression)
        {
            if (!expression)
            {
                throw new ArgumentNullException();
            }
        }
        public static void checkArgument(bool expression, object errorMessage)
        {
            if (!expression)
            {
                throw new ArgumentException(errorMessage.ToString());
            }
        }
    }

    public class Options
    {
        private const long NONE_TIME_TO_LIVE = -1;

        public Options()
        {
            this.sendno = 0;
            this.override_msg_id = 0;
            this.time_to_live = NONE_TIME_TO_LIVE;
            this.big_push_duration = 0;
            this.apns_production = false;
        }
        public Options(int sendno,
                       long overrideMsgId,
                       long timeToLive,
                       int bigPushDuration,
                       bool apnsProduction = false)
        {
            this.sendno = sendno;
            this.override_msg_id = overrideMsgId;
            this.time_to_live = timeToLive;
            this.big_push_duration = bigPushDuration;
            this.apns_production = apnsProduction;
        }
        private int _sendno;
        [DefaultValue(0)]
        public int sendno
        {
            get
            {
                return _sendno;
            }
            set
            {
                Preconditions.checkArgument(value >= 0, "sendno should be greater than 0.");
                _sendno = value;
            }
        }
        private long _override_msg_id;
        [DefaultValue(0)]
        public long override_msg_id
        {
            get
            {
                return _override_msg_id;
            }
            set
            {
                Preconditions.checkArgument(value >= 0, "override_msg_id should be greater than 0.");
                _override_msg_id = value;
            }
        }
        private long _time_to_live;
        [DefaultValue(NONE_TIME_TO_LIVE)]
        public long time_to_live
        {
            get
            {
                return _time_to_live;
            }
            set
            {
                Preconditions.checkArgument(value >= NONE_TIME_TO_LIVE, "time_to_live should be greater than 0.");
                _time_to_live = value;
            }
        }
        private long _big_push_duration;
        [DefaultValue(0)]
        public long big_push_duration
        {
            get
            {
                return _big_push_duration;
            }
            set
            {
                Preconditions.checkArgument(value >= 0, "big_push_duration should be greater than 0.");
                _big_push_duration = value;
            }
        }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool apns_production { get; set; }
    }


    public enum DeviceType
    {
        [Description("android")]
        android,
        [Description("ios")]
        ios,
        [Description("winphone")]
        winphone

    }

    public class Platform
    {
        private const String ALL = "all";
        [JsonProperty(PropertyName = "winphone")]
        public string allPlatform { get; set; }
        private HashSet<string> _deviceTypes;
        public HashSet<string> deviceTypes
        {
            get
            {
                return _deviceTypes;
            }
            set
            {
                if (value != null)
                {
                    allPlatform = null;
                }
                _deviceTypes = value;
            }
        }
        private Platform()
        {
            allPlatform = ALL;
            deviceTypes = null;
        }
        private Platform(bool all, HashSet<string> deviceTypes)
        {
            //用来判断all=true时deviceTypes必须为空，反之当all=false时deviceTypes有值，不然json序列化会出错
            Debug.Assert(all && deviceTypes == null || !all && deviceTypes != null);
            if (all)
            {
                allPlatform = ALL;
            }
            this.deviceTypes = deviceTypes;
        }
        public static Platform all()
        {
            return new Platform(true, null).Check();
        }
        public static Platform ios()
        {
            HashSet<string> types = new HashSet<string>();
            types.Add(DeviceType.ios.ToString());
            return new Platform(false, types).Check();
        }
        public static Platform android()
        {
            HashSet<string> types = new HashSet<string>();
            types.Add(DeviceType.android.ToString());
            return new Platform(false, types).Check();
        }
        public static Platform winphone()
        {
            HashSet<string> types = new HashSet<string>();
            types.Add(DeviceType.winphone.ToString());
            return new Platform(false, types).Check();
        }
        public static Platform android_ios()
        {
            HashSet<string> types = new HashSet<string>();
            types.Add(DeviceType.android.ToString());
            types.Add(DeviceType.ios.ToString());
            return new Platform(false, types).Check();
        }
        public static Platform android_winphone()
        {
            HashSet<string> types = new HashSet<string>();
            types.Add(DeviceType.android.ToString());
            types.Add(DeviceType.winphone.ToString());
            return new Platform(false, types).Check();
        }
        public static Platform ios_winphone()
        {
            HashSet<string> types = new HashSet<string>();
            types.Add(DeviceType.ios.ToString());
            types.Add(DeviceType.winphone.ToString());

            return new Platform(false, types).Check();
        }
        public bool isAll()
        {
            return allPlatform != null;
        }
        public void setAll(bool all)
        {
            if (all)
            {
                allPlatform = ALL;
            }
            else
            {
                allPlatform = null;
            }
        }
        public Platform Check()
        {
            Preconditions.checkArgument(!(isAll() && null != deviceTypes), "Since all is enabled, any platform should not be set.");
            Preconditions.checkArgument(!(!isAll() && null == deviceTypes), "No any deviceType is set.");
            return this;
        }


    }

    public enum AudienceType
    {
        tag,
        tag_and,
        alias,
        segment,
        registration_id
    }


    public class Message
    {
        public String title { get; set; }
        public String msg_content { get; set; }
        public String content_type { get; set; }
        [JsonProperty]
        private Dictionary<string, object> extras { get; set; }

        private Message()
        {

        }
        private Message(String msgContent)
        {
            Preconditions.checkArgument(!(msgContent == null), "msgContent should be set");

            this.title = null;
            this.msg_content = msgContent;
            this.content_type = null;
            this.extras = null;
        }
        private Message(String msgContent, String title, String contentType)
        {
            Preconditions.checkArgument(!(msgContent == null), "msgContent should be set");

            this.title = title;
            this.msg_content = msgContent;
            this.content_type = contentType;
        }
        public static Message content(string msgContent)
        {
            return new Message(msgContent).Check();
        }
        public Message setTitle(String title)
        {
            this.title = title;
            return this;
        }
        public Message setContentType(String ContentType)
        {
            this.content_type = ContentType;
            return this;
        }
        public Message AddExtras(string key, string value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            if (value != null)
            {
                extras.Add(key, value);
            }
            return this;
        }
        public Message AddExtras(string key, int value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            extras.Add(key, value);
            return this;
        }
        public Message AddExtras(string key, bool value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            extras.Add(key, value);
            return this;

        }
        public Message Check()
        {
            Preconditions.checkArgument(!(msg_content == null), "msgContent should be set");
            return this;
        }
    }

    public class AudienceTarget
    {
        public AudienceType audienceType { get; private set; }
        public HashSet<string> valueBuilder { get; private set; }

        private AudienceTarget(AudienceType audienceType, HashSet<string> values)
        {
            this.audienceType = audienceType;
            this.valueBuilder = values;
        }
        public static AudienceTarget tag(HashSet<string> values)
        {
            return new AudienceTarget(AudienceType.tag, values).Check();

        }
        public static AudienceTarget tag_and(HashSet<string> values)
        {
            return new AudienceTarget(AudienceType.tag_and, values).Check();
        }
        public static AudienceTarget alias(HashSet<string> values)
        {
            return new AudienceTarget(AudienceType.alias, values).Check();

        }
        public static AudienceTarget segment(HashSet<string> values)
        {
            return new AudienceTarget(AudienceType.segment, values).Check();

        }
        public static AudienceTarget registrationId(HashSet<string> values)
        {
            return new AudienceTarget(AudienceType.registration_id, values).Check();
        }
        public AudienceTarget Check()
        {
            Preconditions.checkArgument(null != valueBuilder, "Target values should be set one at least.");
            return this;
        }
    }

    public class Audience
    {
        private const String ALL = "all";
        public string allAudience;

        private void AddWithAudienceTarget(AudienceTarget target)
        {
            Debug.Assert(target != null && target.valueBuilder != null);
            if (target != null && target.valueBuilder != null)
            {
                this.allAudience = null;
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, HashSet<string>>();
                }
                if (dictionary.ContainsKey(target.audienceType.ToString()))
                {
                    HashSet<string> origin = dictionary[target.audienceType.ToString()];
                    foreach (var item in target.valueBuilder)
                    {
                        origin.Add(item);
                    }
                }
                else
                {
                    dictionary.Add(target.audienceType.ToString(), target.valueBuilder);
                }
            }
        }


        public Dictionary<string, HashSet<string>> dictionary;
        private Audience()
        {
            allAudience = ALL;
            dictionary = null;
        }

        public static Audience all()
        {
            return new Audience() { allAudience = ALL, dictionary = null }.Check();
        }
        public static Audience s_tag(HashSet<string> values)
        {
            return new Audience().tag(values);
        }
        public static Audience s_tag(params string[] values)
        {
            return new Audience().tag(values);
        }
        public static Audience s_tag_and(HashSet<string> values)
        {
            return new Audience().tag_and(values);
        }
        public static Audience s_tag_and(params string[] values)
        {
            return new Audience().tag_and(values);
        }
        public static Audience s_alias(HashSet<string> values)
        {
            return new Audience().alias(values);
        }
        public static Audience s_alias(params string[] values)
        {
            return new Audience().alias(values);
        }
        public static Audience s_segment(HashSet<string> values)
        {
            return new Audience().segment(values);
        }
        public static Audience s_segment(params string[] values)
        {
            return new Audience().segment(values);
        }
        public static Audience s_registrationId(HashSet<string> values)
        {
            return new Audience().registrationId(values);
        }
        public static Audience s_registrationId(params string[] values)
        {
            return new Audience().registrationId(values);
        }
        public Audience tag(HashSet<string> values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            AudienceTarget target = AudienceTarget.tag(values);
            AddWithAudienceTarget(target);
            return this.Check();
        }
        public Audience tag(params string[] values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            var valueList = new HashSet<string>(values);
            return tag(valueList);

        }
        public Audience tag_and(HashSet<string> values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            AudienceTarget target = AudienceTarget.tag_and(values);
            this.allAudience = null;
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, HashSet<string>>();
            }
            if (dictionary.ContainsKey(target.audienceType.ToString()))
            {
                HashSet<string> origin = dictionary[target.audienceType.ToString()];
                foreach (var item in values)
                {
                    origin.Add(item);
                }
            }
            else
            {
                dictionary.Add(target.audienceType.ToString(), values);
            }
            return this.Check();
        }
        public Audience tag_and(params string[] values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            HashSet<string> list = new HashSet<string>(values);
            return tag_and(list);

        }
        public Audience alias(HashSet<string> values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            AddWithAudienceTarget(AudienceTarget.alias(values));
            return this.Check();
        }
        public Audience alias(params string[] values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            return alias(new HashSet<string>(values));

        }
        public Audience segment(HashSet<string> values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            AddWithAudienceTarget(AudienceTarget.segment(values));
            return this.Check();
        }
        public Audience segment(params string[] values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            return segment(new HashSet<string>(values));

        }
        public Audience registrationId(HashSet<string> values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            AddWithAudienceTarget(AudienceTarget.registrationId(values));
            return this.Check();
        }
        public Audience registrationId(params string[] values)
        {
            if (allAudience != null)
            {
                allAudience = null;
            }
            return registrationId(new HashSet<string>(values));

        }
        public bool isAll()
        {

            return allAudience != null;
        }
        public Audience Check()
        {
            Preconditions.checkArgument(!(isAll() && null != dictionary), "Since all is enabled, any platform should not be set.");
            Preconditions.checkArgument(!(!isAll() && null == dictionary), "No any deviceType is set.");
            return this;
        }
    }


    public class IosNotification : PlatformNotification
    {
        public const String NOTIFICATION_IOS = "ios";

        private const String DEFAULT_SOUND = "";
        private const String DEFAULT_BADGE = "+1";

        private const String BADGE = "badge";
        private const String SOUND = "sound";
        private const String CONTENT_AVAILABLE = "content-available";
        private const String CATEGORY = "category";


        private const String ALERT_VALID_BADGE = "Badge number should be 0~99999, "
                + "and badgeDisabled property must be false";
        private const String SOUNd_VALID_BADGE = "Sound  should not be null or empty, "
                + "and disableSound property must be false";


        private bool soundDisabled;
        private bool badgeDisabled;

        [JsonProperty]
        public String sound { get; private set; }
        [JsonProperty]
        public String badge { get; private set; }

        [JsonProperty(PropertyName = "content-available")]
        public bool contentAvailable { get; private set; }
        [JsonProperty]
        public String category { get; private set; }

        public IosNotification()
        {
            base.alert = null;
            base.extras = null;
            this.soundDisabled = false;
            this.badgeDisabled = false;
            this.contentAvailable = false;
            this.category = null;
            this.badge = DEFAULT_BADGE;
            this.sound = DEFAULT_SOUND;
        }

        public IosNotification disableSound()
        {
            this.soundDisabled = true;
            this.sound = null;
            return this;
        }
        public IosNotification disableBadge()
        {
            this.badgeDisabled = true;
            this.badge = null;
            return this;
        }
        public IosNotification setSound(String sound)
        {

            if ((sound == null) || soundDisabled)
            {
                Console.WriteLine(SOUNd_VALID_BADGE);
                return this;
            }
            this.sound = sound;
            return this;
        }
        public IosNotification setBadge(int badge)
        {
            if (!ServiceHelper.isValidIntBadge(Math.Abs(badge)) || badgeDisabled)
            {
                Console.WriteLine(ALERT_VALID_BADGE);
                return this;
            }
            this.badge = badge.ToString();
            return this;
        }
        public IosNotification autoBadge()
        {
            return incrBadge(1);

        }
        public IosNotification incrBadge(int badge)
        {
            if (!ServiceHelper.isValidIntBadge(Math.Abs(badge)) || badgeDisabled)
            {
                Console.WriteLine(ALERT_VALID_BADGE);
                return this;
            }
            if (badge >= 0)
            {
                this.badge = "+" + badge;
            }
            else
            {
                this.badge = "" + badge;
            }
            return this;
        }
        public IosNotification setAlert(String alert)
        {
            this.alert = alert;
            return this;
        }
        public IosNotification setContentAvailable(bool contentAvailable)
        {
            this.contentAvailable = contentAvailable;
            return this;
        }
        public IosNotification setCategory(String category)
        {
            this.category = category;
            return this;
        }
        public IosNotification AddExtra(string key, string value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            if (value != null)
            {
                extras.Add(key, value);
            }
            return this;
        }
        public IosNotification AddExtra(string key, int value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            extras.Add(key, value);
            return this;
        }
        public IosNotification AddExtra(string key, bool value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            extras.Add(key, value);
            return this;
        }

    }
    public abstract class PlatformNotification
    {

        public const String ALERT = "alert";
        private const String EXTRAS = "extras";
        [JsonProperty]
        public String alert { get; protected set; }
        [JsonProperty]
        public Dictionary<String, object> extras { get; protected set; }

        public PlatformNotification()
        {
            this.alert = null;
            this.extras = null;
        }


    }

    public class ServiceHelper
    {
        private const int MAX_BADGE_NUMBER = 99999;
        private const int MIN = 100000;
        private const int MAX = int.MaxValue;

        public static int generateSendno()
        {
            Random random = new Random();
            return random.Next((MAX - MIN) + 1) + MIN;
        }
        public static bool isValidIntBadge(int intBadge)
        {
            if (intBadge >= 0 && intBadge <= MAX_BADGE_NUMBER)
            {
                return true;
            }
            return false;
        }

    }


    public class AndroidNotification : PlatformNotification
    {
        public const String NOTIFICATION_ANDROID = "android";

        private const String TITLE = "title";
        private const String BUILDER_ID = "builder_id";

        [JsonProperty]
        public String title { get; private set; }
        [JsonProperty]
        public int builder_id { get; private set; }
        public AndroidNotification()
            : base()
        {
            this.title = null;
            this.builder_id = 0;
        }
        public AndroidNotification setTitle(string title)
        {
            this.title = title;
            return this;
        }
        public AndroidNotification setBuilderID(int builder_id)
        {
            this.builder_id = builder_id;
            return this;
        }
        public AndroidNotification setAlert(String alert)
        {
            this.alert = alert;
            return this;
        }
        public AndroidNotification AddExtra(string key, string value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            if (value != null)
            {
                extras.Add(key, value);
            }
            return this;
        }
        public AndroidNotification AddExtra(string key, int value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            extras.Add(key, value);
            return this;
        }
        public AndroidNotification AddExtra(string key, bool value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            extras.Add(key, value);
            return this;

        }


    }


    public class WinphoneNotification : PlatformNotification
    {
        [JsonProperty]
        private String title;
        [JsonProperty(PropertyName = "_open_page")]
        public String openPage;

        public WinphoneNotification()
            : base()
        {
            this.title = null;
            this.openPage = null;
        }

        public WinphoneNotification setAlert(String alert)
        {
            this.alert = alert;
            return this;
        }
        public WinphoneNotification setOpenPage(String openPage)
        {
            this.openPage = openPage;
            return this;
        }
        public WinphoneNotification setTitle(String title)
        {
            this.title = title;
            return this;
        }
        public WinphoneNotification AddExtra(string key, string value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            if (value != null)
            {
                extras.Add(key, value);
            }

            return this;
        }
        public WinphoneNotification AddExtra(string key, int value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            extras.Add(key, value);
            return this;
        }
        public WinphoneNotification AddExtra(string key, bool value)
        {
            if (extras == null)
            {
                extras = new Dictionary<string, object>();
            }
            extras.Add(key, value);
            return this;
        }

    }
    public class Notification
    {
        public String alert { get; set; }
        [JsonProperty(PropertyName = "ios")]
        public IosNotification IosNotification { get; set; }
        [JsonProperty(PropertyName = "android")]
        public AndroidNotification AndroidNotification { get; set; }
        [JsonProperty(PropertyName = "winphone")]
        public WinphoneNotification WinphoneNotification { get; set; }

        public Notification()
        {
            this.alert = null;
            this.IosNotification = null;
            this.AndroidNotification = null;
            this.WinphoneNotification = null;
        }

        public Notification setAlert(string alert)
        {
            this.alert = alert;
            return this;
        }
        public Notification setAndroid(AndroidNotification android)
        {
            this.AndroidNotification = android;
            return this;
        }
        public Notification setIos(IosNotification ios)
        {
            this.IosNotification = ios;
            return this;
        }
        public Notification setWinphone(WinphoneNotification winphone)
        {
            this.WinphoneNotification = winphone;
            return this;
        }

        public static Notification android(String alert, String title)
        {
            var platformNotification = new AndroidNotification().setAlert(alert).setTitle(title);
            var notificaiton = new Notification().setAlert(alert);
            notificaiton.AndroidNotification = platformNotification;
            return notificaiton;
        }
        public static Notification ios(String alert)
        {
            var iosNotification = new IosNotification().setAlert(alert);
            var notification = new Notification().setAlert(alert);
            notification.IosNotification = iosNotification;
            return notification;
        }
        public static Notification ios_auto_badge()
        {
            var platformNotification = new IosNotification();
            platformNotification.autoBadge();
            var notificaiton = new Notification().setAlert(""); ;
            notificaiton.IosNotification = platformNotification;
            return notificaiton;
        }
        public static Notification ios_set_badge(int badge)
        {
            var platformNotification = new IosNotification();
            platformNotification.setBadge(badge);

            var notificaiton = new Notification();
            notificaiton.IosNotification = platformNotification;
            return notificaiton;
        }
        public static Notification ios_incr_badge(int badge)
        {
            var platformNotification = new IosNotification();
            platformNotification.incrBadge(badge);
            var notificaiton = new Notification();
            notificaiton.IosNotification = platformNotification;
            return notificaiton;
        }
        public static Notification winphone(String alert)
        {
            var platformNotification = new WinphoneNotification().setAlert(alert);

            var notificaiton = new Notification().setAlert(alert);
            notificaiton.WinphoneNotification = platformNotification;
            return notificaiton;
        }

        public Notification Check()
        {
            Preconditions.checkArgument(!(isPlatformEmpty() && null == alert), "No notification payload is set.");
            if (IosNotification != null)
            {
                Preconditions.checkArgument(!(null == IosNotification.alert && null == alert), "No notification payload is set.");
            }
            if (AndroidNotification != null)
            {
                Preconditions.checkArgument(!(null == AndroidNotification.alert && null == alert), "No notification payload is set.");

            }
            if (WinphoneNotification != null)
            {
                Preconditions.checkArgument(!(null == WinphoneNotification.alert && null == alert), "No notification payload is set.");

            }
            return this;
        }
        private bool isPlatformEmpty()
        {
            return (IosNotification == null && AndroidNotification == null && WinphoneNotification == null);
        }
    }

    public class PushPayload
    {

        private JsonSerializerSettings jSetting;
        private const String PLATFORM = "platform";
        private const String AUDIENCE = "audience";
        private const String NOTIFICATION = "notification";
        private const String MESSAGE = "message";
        private const String OPTIONS = "options";

        private const int MAX_GLOBAL_ENTITY_LENGTH = 1200;  // Definition acording to JPush Docs
        private const int MAX_IOS_PAYLOAD_LENGTH = 220;  // Definition acording to JPush Docs

        //serializaiton property
        [JsonConverter(typeof(PlatformConverter))]
        public Platform platform { get; set; }
        [JsonConverter(typeof(AudienceConverter))]
        public Audience audience { get; set; }
        public Notification notification { get; set; }
        public Message message { get; set; }
        public Options options { get; set; }
        //construct
        public PushPayload()
        {
            platform = null;
            audience = null;
            notification = null;
            message = null;
            options = new Options();
            jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            jSetting.DefaultValueHandling = DefaultValueHandling.Ignore;
        }
        public PushPayload(Platform platform, Audience audience, Notification notification, Message message = null, Options options = null)
        {
            Debug.Assert(platform != null);
            Debug.Assert(audience != null);
            Debug.Assert(notification != null || message != null);

            this.platform = platform;
            this.audience = audience;
            this.notification = notification;
            this.message = message;
            this.options = options;

            jSetting = new JsonSerializerSettings();
            jSetting.NullValueHandling = NullValueHandling.Ignore;
            jSetting.DefaultValueHandling = DefaultValueHandling.Ignore;
        }
        /**
         * The shortcut of building a simple alert notification object to all platforms and all audiences
        */
        public static PushPayload AlertAll(String alert)
        {
            return new PushPayload(Platform.all(),
                                   Audience.all(),
                                   new Notification().setAlert(alert),
                                   null,
                                   new Options());
        }
        //* The shortcut of building a simple message object to all platforms and all audiences
        //*/
        public static PushPayload MessageAll(String msgContent)
        {
            return new PushPayload(Platform.all(),
                                   Audience.all(),
                                   null,
                                   Message.content(msgContent),
                                   new Options());
        }
        public static PushPayload FromJSON(String payloadString)
        {
            try
            {
                var jSetting = new JsonSerializerSettings();
                jSetting.NullValueHandling = NullValueHandling.Ignore;
                jSetting.DefaultValueHandling = DefaultValueHandling.Ignore;

                var jsonObject = JsonConvert.DeserializeObject<PushPayload>(payloadString, jSetting);
                return jsonObject.Check();
            }
            catch (Exception e)
            {
                Console.WriteLine("JSON to PushPayLoad occur error:" + e.Message);
                return null;
            }
        }
        public void ResetOptionsApnsProduction(bool apnsProduction)
        {
            if (this.options == null)
            {
                this.options = new Options();
            }
            this.options.apns_production = apnsProduction;
        }
        public void ResetOptionsTimeToLive(long timeToLive)
        {
            if (this.options == null)
            {
                this.options = new Options();
            }
            this.options.time_to_live = timeToLive;
        }
        public int GetSendno()
        {
            if (this.options != null)
                return this.options.sendno;
            return 0;
        }
        public bool IsGlobalExceedLength()
        {
            int messageLength = 0;
            if (message != null)
            {
                var messageJson = JsonConvert.SerializeObject(this.message, jSetting);
                messageLength += UTF8Encoding.UTF8.GetBytes(messageJson).Length;
            }
            if (this.notification == null)
            {
                return messageLength > MAX_GLOBAL_ENTITY_LENGTH;
            }
            else
            {
                var notificationJson = JsonConvert.SerializeObject(this.notification);
                if (notificationJson != null)
                {
                    messageLength += UTF8Encoding.UTF8.GetBytes(notificationJson).Length;
                }
                return messageLength > MAX_GLOBAL_ENTITY_LENGTH;
            }

        }
        public bool IsIosExceedLength()
        {
            if (this.notification != null)
            {
                if (this.notification.IosNotification != null)
                {
                    var iosJson = JsonConvert.SerializeObject(this.notification.IosNotification, jSetting);
                    if (iosJson != null)
                    {
                        return UTF8Encoding.UTF8.GetBytes(iosJson).Length > MAX_IOS_PAYLOAD_LENGTH;
                    }
                }
                else
                {
                    if (!(this.notification.alert == null))
                    {
                        string jsonText;
                        using (StringWriter sw = new StringWriter())
                        {
                            JsonWriter writer = new JsonTextWriter(sw);
                            writer.WriteValue(this.notification.alert);
                            writer.Flush();
                            jsonText = sw.GetStringBuilder().ToString();
                        }
                        return UTF8Encoding.UTF8.GetBytes(jsonText).Length > MAX_IOS_PAYLOAD_LENGTH;
                    }
                    else
                    {
                        // No iOS Payload
                    }


                }

            }
            return false;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, jSetting);
        }
        public PushPayload Check()
        {
            Preconditions.checkArgument(!(null == audience || null == platform), "audience and platform both should be set.");
            Preconditions.checkArgument(!(null == notification && null == message), "notification or message should be set at least one.");
            if (audience != null)
            {
                audience.Check();
            }
            if (platform != null)
            {
                platform.Check();
            }
            if (message != null)
            {
                message.Check();
            }
            if (notification != null)
            {
                notification.Check();
            }
            return this;
        }

    }

    public class AudienceConverter : JsonConverter
    {
        /// <summary>
        /// Platform whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Audience))
                return true;
            return false;
        }
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Audience audience = value as Audience;
            if (audience == null)
            {
                return;
            }
            audience.Check();
            if (audience.isAll())
            {
                writer.WriteValue(audience.allAudience);
                //writer.WriteValue("alll");
            }
            else
            {
                var json = JsonConvert.SerializeObject(audience.dictionary);
                writer.WriteRawValue(json);
            }
        }
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Audience audience = Audience.all();
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                audience.allAudience = reader.Value.ToString();
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                audience.allAudience = null;
                Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
                string key = "key";
                HashSet<string> value = null;
                while (reader.Read())
                {
                    Debug.WriteLine("Type:{0},Path:{1}", reader.TokenType, reader.Path);
                    switch (reader.TokenType)
                    {
                        case JsonToken.StartObject:
                            break;
                        case JsonToken.PropertyName:
                            key = reader.Value.ToString();
                            break;
                        case JsonToken.StartArray:
                            value = new HashSet<string>();
                            break;
                        case JsonToken.String:
                            value.Add(reader.Value.ToString());
                            break;
                        case JsonToken.EndArray:
                            {
                                dictionary.Add(key, value);
                            }
                            break;
                        case JsonToken.EndObject:
                            return audience;
                    }
                }
                audience.dictionary = dictionary;
            }
            return audience;
        }
    }

    public class PlatformConverter : JsonConverter
    {
        /// <summary>
        /// Platform whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Platform))
                return true;

            return false;
        }
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Platform platform = value as Platform;
            if (platform == null)
            {
                return;
            }
            platform.Check();
            if (platform.isAll())
            {
                writer.WriteValue(platform.allPlatform);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var item in platform.deviceTypes)
                {
                    writer.WriteValue(item);
                }
                writer.WriteEndArray();
            }
        }
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Platform platform = Platform.all();
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                platform.allPlatform = null;
                platform.deviceTypes = ReadArray(reader);
            }
            else if (reader.TokenType == JsonToken.String)
            {
                platform.allPlatform = reader.Value.ToString();
            }
            else
            {
                return null;
            }
            return platform;
        }

        private HashSet<string> ReadArray(JsonReader reader)
        {
            HashSet<string> list = new HashSet<string>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                        list.Add(Convert.ToString(reader.Value, CultureInfo.InvariantCulture));
                        break;
                    case JsonToken.EndArray:
                        return list;
                    case JsonToken.Comment:
                        // skip
                        break;
                    default:
                        return null;
                }
            }
            return null;
        }

    }
}