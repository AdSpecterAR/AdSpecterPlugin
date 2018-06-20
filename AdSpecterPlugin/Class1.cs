using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.Video;

namespace AdSpecter
{
    [Serializable]
    public class DeveloperApp
    {
        public int id;
        public string name;
        public int developer_app_id;
        public User user;

        public DeveloperApp(int developerAppId)
        {
            developer_app_id = developerAppId;
        }

        public static DeveloperApp CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<DeveloperApp>(jsonString);
        }

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [Serializable]
    public class User
    {
        public int id;
        public string first_name;
        public string last_name;
        public string full_name;
        public string account_type;
        public string username;
        public string email;
        public string authentication_token;

        public static User CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<User>(jsonString);
        }
    }

    [Serializable]
    public class AdUnit
    {
        public int id;
        public string title;
        public string description;
        public string click_url_default;
        public string click_url_ios;
        public string click_url_android;
        public string ad_unit_url;
        public bool active;
        public User user;
        public int aspect_ratio_width;
        public int aspect_ratio_height;
        public string ad_format;
        public bool rewarded;
        public bool interstitial;
        public string call_to_action;
        public string impression_url_ios;
        public string impression_url_android;
        public string attribution_partner;

        public static AdUnit CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AdUnit>(jsonString);
        }
    }

    [Serializable]
    public class AdUnitWrapper
    {
        public AdUnit ad_unit;
        public int impression_id;

        public static AdUnitWrapper CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AdUnitWrapper>(jsonString);
        }
    }

    [Serializable]
    public class AppSession
    {
        public int id;
        public int developer_app_id;

        public static AppSession CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AppSession>(jsonString);
        }
    }

    [Serializable]
    public class AppSessionWrapper
    {
        public AppSession app_session;

        public static AppSessionWrapper CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AppSessionWrapper>(jsonString);
        }
    }

    [Serializable]
    public class Device
    {
        public string device_model;

        public Device()
        {
            device_model = SystemInfo.deviceModel;
        }

        public static Device CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<Device>(jsonString);
        }

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [Serializable]
    public class AppSetup
    {
        public Device device;
        public string developer_key;

        public AppSetup(string developerKey)
        {
            developer_key = developerKey;
            device = new Device();
        }

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [Serializable]
    public class AppSetupWrapper
    {
        public AppSetup developer_app;

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }


    public class AdLoaderPlugIn : MonoBehaviour
    {
        public bool hasAdLoaded;

        private GameObject ASRUAdUnit;
        private AdUnitWrapper adUnitWrapper;
        private int impressionId;
        private VideoPlayer video;

        private bool firstImpressionPosted = false;

        void Start()
        {
            hasAdLoaded = false;
        }

        public IEnumerator GetAdUnit(GameObject adUnit, string format, int width, int height)
        {
            ASRUAdUnit = adUnit;

            var aspect_ratio_height = height;
            var aspect_ratio_width = width;

            if (width == height)
            {
                aspect_ratio_height = 1;
                aspect_ratio_width = 1;
            }

            var baseUrl = "https://adspecter-sandbox.herokuapp.com/ad_units/fetch";
           
            var appSession = AdSpecterConfigPlugIn.appSessionWrapper.app_session;

            var url = baseUrl +
                      "?ad_format=" + format +
                      "&aspect_ratio_width=" + aspect_ratio_width +
                      "&aspect_ratio_height=" + aspect_ratio_height +
                      "&app_session_id=" + appSession.id +
                      "&developer_app_id=" + appSession.developer_app_id;

          //  url = "https://adspecter-sandbox.herokuapp.com/ad_units/fetch_portal?ad_format=image_360";
            UnityWebRequest uwr = UnityWebRequest.Get(url);

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error while retrieving ad: " + uwr.error);
            }
            else
            {
                adUnitWrapper = AdUnitWrapper.CreateFromJSON(uwr.downloadHandler.text);
                impressionId = adUnitWrapper.impression_id;

                Debug.Log("IMPRESSION ID: " + impressionId);

                switch (format)
                {
                    case "image":
                        {
                            StartCoroutine(GetImageTexture(adUnitWrapper.ad_unit.ad_unit_url));
                            break;
                        }

                    case "video":
                        {
                            GetVideo(adUnitWrapper.ad_unit.ad_unit_url);
                            //GetVideo("https://www.quirksmode.org/html5/videos/big_buck_bunny.mp4");
                            break;
                        }
                }
            }
        }

        public IEnumerator Get360AdUnit(GameObject adUnit, string format)
        {
            ASRUAdUnit = adUnit;
            
            var baseUrl = "https://adspecter-sandbox.herokuapp.com/ad_units/fetch_portal";

            // var appSession = AdSpecterConfigPlugIn.appSessionWrapper.app_session;
            //Debug.Log("Appsession: " + appSession);
            var url = baseUrl +
                    "?ad_format=" + format;
            //            "&app_session_id=" + appSession.id +
            //          "&developer_app_id=" + appSession.developer_app_id;
           
            UnityWebRequest uwr = UnityWebRequest.Get(url);
           
            yield return uwr.SendWebRequest();
           
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error while retrieving ad: " + uwr.error);
            }
            else
            {
                adUnitWrapper = AdUnitWrapper.CreateFromJSON(uwr.downloadHandler.text);
               
                impressionId = adUnitWrapper.impression_id;
                
                Debug.Log("IMPRESSION ID: " + impressionId);

                switch (format)
                {
                    case "image_360":
                        {
                            StartCoroutine(GetImageTexture(adUnitWrapper.ad_unit.ad_unit_url));
                            break;
                        }

                    case "video_360":
                        {
                            GetVideo(adUnitWrapper.ad_unit.ad_unit_url);
                            //GetVideo("https://www.quirksmode.org/html5/videos/big_buck_bunny.mp4");
                            break;
                        }
                }
            }
        }

        void GetVideo(string url)
        {
            video = ASRUAdUnit.AddComponent<UnityEngine.Video.VideoPlayer>();

            video.url = url;
            video.isLooping = true;
            video.playOnAwake = false;

            hasAdLoaded = true;
        }

        IEnumerator GetImageTexture(string url)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
           
            yield return www.SendWebRequest();
           
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("Error while getting ad texture:" + www.error);
            }
            else
            {
                Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
             
                ASRUAdUnit.GetComponent<Renderer>().material.mainTexture = myTexture;

                hasAdLoaded = true;
            }

        }

        public void PlayVideo()
        {
            /* AudioSource audio = ASRUAdUnit.GetComponent<AudioSource>();
             audio.clip = movie.audioClip;
             Debug.Log(movie.audioClip);
             audio.Play();*/
            if (!video.isPlaying)
            {
                video.Play();
            }
        }

        public void PauseVideo()
        {
            if (video.isPlaying)
            {
                video.Pause();
            }
        }

        public bool IsVideoPlaying()
        {
            return video.isPlaying;
        }


        IEnumerator PostImpression(string json, string url)
        {
            var uwr = new UnityWebRequest(url, "PUT");

            if (json != "")
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            }

            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error While Sending Impression: " + uwr.error);
            }
            else
            {
                Debug.Log("Impression posted successfully!");
            }
        }

        public IEnumerator LogImpression()
        {
            //            StartCoroutine(PostImpression("", string.Format("http://localhost:3000/impressions/{0}/shown", impressionId)));

            // var impressionUrl = string.Format("https://app.adjust.com/cbtest" +
            //                                 "?impression_callback=https%3A%2F%2Fadspecter-sandbox.herokuapp.com%2Fpostback%2Fadjust%2Fimpression%3Fimpression_id%3D3{0}", impressionId);
            var impressionUrl = string.Format(whichImpressionURL(), impressionId);
            var uwr = new UnityWebRequest(impressionUrl, "GET");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error While Sending Impression viewed to adjust: " + uwr.error);
            }
            else
            {
                Debug.Log("Impression successfully seen!");
            }
        }

        private string whichImpressionURL()
        {
            // TODO: IMPLEMENT
            var impressionURL = "";
            var siteID = Application.productName + Application.platform + "AdSpecter";
            var productionParameters = string.Format("?impression_callback=https%3A%2F%2Fsanchez-production.herokuapp.com%2Fpostback%2Fadjust%2Fimpression%3Fimpression_id%3D{0}", impressionId);
            var debugParameters = string.Format("?impression_callback=https%3A%2F%2Fadspecter-sandbox.herokuapp.com%2Fpostback%2Fadjust%2Fimpression%3Fimpression_id%3D{0}", impressionId);

            switch (adUnitWrapper.ad_unit.attribution_partner)
            {
                case "appsflyer":
                {
                    if (Debug.isDebugBuild)
                    {
                        impressionURL = "https://adspecter-sandbox.herokuapp.com/postback/appsflyer/impression?impression_id={0}";
                    }
                    else
                    {
                        if (Application.platform == RuntimePlatform.Android)
                        {
                            impressionURL = string.Format("https://impression.appsflyer.com/app_id?c=test&pid=adspecter_int&click_id={0}&af_siteid={1}", impressionId, siteID);
                                //&advertising_id={GAID}
                            }
                            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                        {
                            impressionURL = string.Format("https://impression.appsflyer.com/app_id?c=test&pid=adspecter_int&click_id={0}&af_siteid={1}", impressionId, siteID);
                              //  &idfa ={ IDFA}
                            }
                    }
                    break;
                }
                case "adjust":
                {
                    var baseURL = "https://app.adjust.com/cbtest";
                    
                    if (Debug.isDebugBuild)
                    {
                        impressionURL = baseURL + debugParameters;
                    }
                    else
                    {
                        if (Application.platform == RuntimePlatform.Android)
                        {
                            baseURL = adUnitWrapper.ad_unit.impression_url_android;
                        }
                        else if (Application.platform == RuntimePlatform.IPhonePlayer)
                        {
                            baseURL = adUnitWrapper.ad_unit.impression_url_ios;
                        }
                        
                        impressionURL = baseURL + productionParameters;
                    }
                    
                    break;
                }
            }
           

            return impressionURL;
        }

        private string whichClickThroughURL()
        {
            // TODO: IMPLEMENT
            string clickThroughURL = "";
            var siteID = Application.productName + Application.platform + "AdSpecter";
            string productionParameters = string.Format("?install_callback=https%3A%2F%2Fsanchez-production.herokuapp.com%2Fpostback%2Fadjust%2Finstall%3Fimpression_id%3D{0}" +
                                        "&click_callback=https%3A%2F%2Fsanchez-production.herokuapp.com%2Fpostback%2Fadjust%2Fclick%3Fimpression_id%3D{0}", impressionId);
            string debugParameters = string.Format("?install_callback=https%3A%2F%2Fadspecter-sandbox.herokuapp.com%2Fpostback%2Fadjust%2Finstall%3Fimpression_id%3D{0}" +
                                        "&click_callback=https%3A%2F%2Fadspecter-sandbox.herokuapp.com%2Fpostback%2Fadjust%2Fclick%3Fimpression_id%3D{0}", impressionId);

            //put in new generated URL?
            switch (adUnitWrapper.ad_unit.attribution_partner)
            {
                case "appsflyer":
                {
                    if (Debug.isDebugBuild)
                    {
                        clickThroughURL = string.Format("https://adspecter-sandbox.herokuapp.com/postback/appsflyer/click?impression_id={0}", impressionId);
                    }
                    else
                    {
                        if (Application.platform == RuntimePlatform.Android)
                        {
                            clickThroughURL = string.Format("https://app.appsflyer.com/app_id?pid=adspecter_int&click_id={0}&af_siteid={1}", impressionId, siteID);
                        }
                        else if (Application.platform == RuntimePlatform.IPhonePlayer)
                        {
                            clickThroughURL = string.Format("https://app.appsflyer.com/app_id?pid=adspecter_int&click_id={0}&af_siteid{1}", impressionId, siteID);
                        }
                    }
                    
                    break;
                }
                case "adjust":
                {
                    if (Debug.isDebugBuild)
                    {
                        clickThroughURL = "https://app.adjust.com/cbtest" + debugParameters;
                    }
                    else
                    {
                        string baseURL = adUnitWrapper.ad_unit.click_url_default;

                        if (Application.platform == RuntimePlatform.Android)
                        {
                            baseURL = adUnitWrapper.ad_unit.click_url_android;
                        }
                        else if (Application.platform == RuntimePlatform.IPhonePlayer)
                        {
                            baseURL = adUnitWrapper.ad_unit.click_url_ios;
                        }
                        
                        clickThroughURL = baseURL + productionParameters;
                    }
                    
                    break;
                }
            }

            return clickThroughURL;
        }

        public void DetectClickThrough()
        {
            RaycastHit hit;
            var touches = Input.touches;

            foreach (Touch touch in touches)
            {
                var ray = Camera.main.ScreenPointToRay(new Vector3(touch.position.x, touch.position.y, 0));
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (hit.transform.parent == ASRUAdUnit.transform && hit.transform.name == "ASRUCTA")
                    {
                        Application.OpenURL(whichClickThroughURL());

                        //                        StartCoroutine(PostImpression("", string.Format("https://adspecter-sandbox.herokuapp.com/impressions/{0}/clicked", impressionId)));
                        //                        StartCoroutine(PostImpression("", string.Format("http://localhost:3000/impressions/{0}/clicked", impressionId)));
                    }
                }
            }
        }

        public string GetCallToAction()
        {
            if (adUnitWrapper != null && adUnitWrapper.ad_unit != null)
            {
                return adUnitWrapper.ad_unit.call_to_action;
            }

            return null;
        }
    }


    public class AdSpecterConfigPlugIn : MonoBehaviour
    {
        public static string appSessionId;
        public static AppSessionWrapper appSessionWrapper;
        
        private GeoData geoData;

        public bool loadAds = false;
        public bool inUSA = false;
        public bool inCanada = false;

        public void AuthenticateUser(string developerKey)
        {
            var appSetup = new AppSetup(developerKey);
            var postData = appSetup.SaveToString();

            //            var url = "http://localhost:3000/developer_app/authenticate";
            var url = "https://adspecter-sandbox.herokuapp.com/developer_app/authenticate";

            //   Debug.Log("Authentication post data: " + postData);

            StartCoroutine(ASRUSetDeveloperKey(postData, url));
        }


        IEnumerator ASRUSetDeveloperKey(string json, string url)
        {
            var uwr = new UnityWebRequest(url, "PUT");

            if (json != "")
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            }

            uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                //                Debug.Log("Is network error? " + uwr.isNetworkError);
                //                Debug.Log("Is HTTP error? " + uwr.isHttpError);
                Debug.Log("Error while setting developer key: " + uwr.error);
            }
            else
            {
                //                Debug.Log("Developer key set successfully");

                appSessionWrapper = AppSessionWrapper.CreateFromJSON(uwr.downloadHandler.text);

                loadAds = true;
            }
        }

        public IEnumerator GetGeoData()
        {
            UnityWebRequest www = UnityWebRequest.Get("https://api.ipdata.co/?api-key=1ad68e416ab80c9eaa32d01f42de56210053fff2bb3c0d838af50e34");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("error getting geodata" + www.error);
            }
            else
            {
                try
                {
                    geoData = JsonUtility.FromJson<GeoData>(www.downloadHandler.text);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Could not get geo data: " + ex.ToString());
                    //  return;
                }


                if (geoData.country_code == null)
                {
                    Debug.LogError("Unsuccessful geo data request: " + www.downloadHandler.text);
                }

                if (geoData.country_code == "US")
                {
                    inUSA = true;
                }

                if (geoData.country_code == "CA")
                {
                    inCanada = true;
                }

            }
        }

        public bool IsValid()
        {
            return inCanada;
        }
    }

    [Serializable]
    public class GeoData
    {
        /// <summary>
        /// The status that is returned if the response was successful.
        /// </summary>
        //public const string SuccessResult = "success";
       // public string status;
        public string country_code;
        //public string query;
    }
}
