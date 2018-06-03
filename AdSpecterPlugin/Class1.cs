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

        public static AdUnit CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AdUnit>(jsonString);
        }
    }

    [Serializable]
    public class AdUnitWrapper
    {
        public AdUnit ad_unit;

        public static AdUnitWrapper CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AdUnitWrapper>(jsonString);
        }
    }

    [Serializable]
    public class Impression
    {
        public int id;
        public int ad_unit_id;
        public int developer_app_id;
        public int app_session_id;
        public bool served;
        public bool clicked;
        public bool shown;
        public int interaction_length;

        public Impression(int adUnitId, int developerAppId, int appSessionId)
        {
            id = 0;
            served = true;
            clicked = false;
            shown = false;
            ad_unit_id = adUnitId;
            developer_app_id = developerAppId;
            app_session_id = appSessionId;
            interaction_length = 0;
        }

        public static Impression CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<Impression>(jsonString);
        }
    }

    [Serializable]
    public class ImpressionWrapper
    {
        public Impression impression;

        public static ImpressionWrapper CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<ImpressionWrapper>(jsonString);
        }

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
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
        private GameObject ASRUAdUnit;

        private AdUnitWrapper adUnitWrapper;
        private ImpressionWrapper impressionWrapper;
        public bool hasAdLoaded;
        // private Renderer[] renderers;
        private VideoPlayer video;
        bool firstImpressionPosted = false;

        void Start()
        {
            hasAdLoaded = false;
        }

        public IEnumerator GetAdUnit(GameObject adUnit, string format, int width, int height)
        {
            //format must be "image" or "video"

            ASRUAdUnit = adUnit;

            var aspect_ratio_height = height;
            var aspect_ratio_width = width;

            if (width == height)
            {
                aspect_ratio_height = 1;
                aspect_ratio_width = 1;
            }

            //var url ="https://adspecter-sandbox.herokuapp.com/ad_units/default";
            var baseUrl = "https://adspecter-sandbox.herokuapp.com/ad_units/fetch";

            var url = baseUrl +
                       "?ad_format=" + format +
                       "&aspect_ratio_width=" + aspect_ratio_width +
                       "&aspect_ratio_height=" + aspect_ratio_height;

            UnityWebRequest uwr = UnityWebRequest.Get(url);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error while retrieving ad: " + uwr.error);
            }
            else
            {
                //Debug.Log("Received ad unit");
                //Debug.Log(uwr.downloadHandler.text);
                adUnitWrapper = AdUnitWrapper.CreateFromJSON(uwr.downloadHandler.text);

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

        void GetVideo(string url)
        {
            video = ASRUAdUnit.AddComponent<UnityEngine.Video.VideoPlayer>();

            video.url = url;
            video.isLooping = true;
            video.playOnAwake = false;

            // TODO: change this so that it is set true only when video has started playing
            hasAdLoaded = true;
        }

        //called by getAdUnit
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

            if (!video.isPlaying && video.isPrepared)
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

        public void NewImpression()
        {
            if (firstImpressionPosted)
            {
                return;
            }

            // Debug.Log("posting first impression!");
            var impression = new Impression(
                adUnitWrapper.ad_unit.id,
                AdSpecterConfigPlugIn.appSessionWrapper.app_session.developer_app_id,
                AdSpecterConfigPlugIn.appSessionWrapper.app_session.id
            );

            impressionWrapper = new ImpressionWrapper();
            impressionWrapper.impression = impression;

            var json = impressionWrapper.SaveToString();

            StartCoroutine(PostImpression(json, "https://adspecter-sandbox.herokuapp.com/impressions"));
            //hasAdLoaded = true;
            firstImpressionPosted = true;
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
                impressionWrapper = ImpressionWrapper.CreateFromJSON(uwr.downloadHandler.text);
            }
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
                        string click_url;

                        /* if (Application.platform == RuntimePlatform.Android)
                         {
                             click_url = adUnitWrapper.ad_unit.click_url_android;
                         }
                         else if (Application.platform == RuntimePlatform.IPhonePlayer)
                         {
                             click_url = adUnitWrapper.ad_unit.click_url_ios;
                         }
                         else
                         {
                             click_url = adUnitWrapper.ad_unit.click_url_default;
                         }
                         */

                        click_url = adUnitWrapper.ad_unit.click_url_default;

                        Application.OpenURL(click_url);
                        var json = impressionWrapper.SaveToString();
                        var impressionId = impressionWrapper.impression.id;

                        StartCoroutine(PostImpression("", string.Format("https://adspecter-sandbox.herokuapp.com/impressions/{0}/clicked", impressionId)));
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

        public bool loadAds = false;

        public void AuthenticateUser(string developerKey)
        {
            var appSetup = new AppSetup(developerKey);
            var postData = appSetup.SaveToString();

            var url = "https://adspecter-sandbox.herokuapp.com/developer_app/authenticate";

            //            Debug.Log("Authentication post data: " + postData);

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
    }
}
