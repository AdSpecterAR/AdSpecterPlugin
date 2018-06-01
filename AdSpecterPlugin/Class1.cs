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
        public string developer_key;
        public Device device;

        public AppSetup(string developerKey)
        {
            developer_key = developerKey;
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
        public bool startUpdate;
        // private Renderer[] renderers;
        private VideoPlayer video;

        void Start()
        {
            startUpdate = false;
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

                switch(format)
                {
                    case "image":
                    {
                        StartCoroutine(GetImageTexture(adUnitWrapper.ad_unit.ad_unit_url));
                        break;
                    }

                    case "video":
                    {
                            //  StartCoroutine(GetMovieTexture("https://unity3d.com/files/docs/sample.ogg"));
                            //StartCoroutine(GetMovieTexture(adUnitWrapper.ad_unit.ad_unit_url));
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

           // Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Debug.Log("Received ad texture");

            //ASRUAdUnit.GetComponent<Renderer>().material.mainTexture = myTexture;
            //RenderOff();
                


            var impression = new Impression(adUnitWrapper.ad_unit.id,
                    AdSpecterConfigPlugIn.appSessionWrapper.app_session.developer_app_id,
                    AdSpecterConfigPlugIn.appSessionWrapper.app_session.id
                );
            Debug.Log("impression: " + impression);

            impressionWrapper = new ImpressionWrapper();
            impressionWrapper.impression = impression;

            var json = impressionWrapper.SaveToString();

            Debug.Log("Ad was seen");

            StartCoroutine(PostImpression(json, "https://adspecter-sandbox.herokuapp.com/impressions"));

            startUpdate = true;
            
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
                 //Debug.Log("Received ad texture");

                ASRUAdUnit.GetComponent<Renderer>().material.mainTexture = myTexture;
                //RenderOff();
                
                var impression = new Impression(adUnitWrapper.ad_unit.id,
                     AdSpecterConfigPlugIn.appSessionWrapper.app_session.developer_app_id,
                     AdSpecterConfigPlugIn.appSessionWrapper.app_session.id
                 );
                //Debug.Log("impression: " + impression);

                 impressionWrapper = new ImpressionWrapper();
                 impressionWrapper.impression = impression;

                 var json = impressionWrapper.SaveToString();

                 //Debug.Log("Ad was seen");

                StartCoroutine(PostImpression(json, "https://adspecter-sandbox.herokuapp.com/impressions"));

                startUpdate = true;
                //RenderOn();
            }
         } 

        /*
        IEnumerator GetMovieTexture(string url)
        {
            UnityWebRequest www = UnityWebRequestMultimedia.GetMovieTexture(url);
            
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("Error while getting ad texture:" + www.error);
            }
            else
            {
                MovieTexture myTexture = (DownloadHandlerMovieTexture.GetContent(www));
                //Debug.Log("Received ad texture");

                ASRUAdUnit.GetComponent<Renderer>().material.mainTexture = myTexture;
               // RenderOff();


                //ASRUAdUnit.SetActive(true);



                var impression = new Impression(adUnitWrapper.ad_unit.id,
                    AdSpecterConfigPlugIn.appSessionWrapper.app_session.developer_app_id,
                    AdSpecterConfigPlugIn.appSessionWrapper.app_session.id
                );

                impressionWrapper = new ImpressionWrapper();
                impressionWrapper.impression = impression;

                var json = impressionWrapper.SaveToString();

                //Debug.Log("Ad was seen");
                //            StartCoroutine(PostImpression(json, "http://localhost:3000/impressions"));
                StartCoroutine(PostImpression(json, "https://adspecter-sandbox.herokuapp.com/impressions"));

                startUpdate = true;
              //  RenderOn();
            }
        }*/

      /*  public void RenderOff()
        {
            renderers = ASRUAdUnit.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Debug.Log(renderer);
                renderer.enabled = false;
            }
        }

        public void RenderOn()
        {
            renderers = ASRUAdUnit.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                Debug.Log(renderer);
                renderer.enabled = true;
            }
        }*/


        //only call if ad format is video, returns length of movie
        public void PlayVideo()
        {
            //MovieTexture movie = ASRUAdUnit.GetComponent<Renderer>().material.mainTexture as MovieTexture;

            /* AudioSource audio = ASRUAdUnit.GetComponent<AudioSource>();
             audio.clip = movie.audioClip;
             Debug.Log(movie.audioClip);
             audio.Play();*/
            if (!video.isPlaying && video.isPrepared)
            //if(!movie.isPlaying)
            {
                video.Play();
            }
        }

        public void PauseVideo()
        {
           // MovieTexture movie = ASRUAdUnit.GetComponent<Renderer>().material.mainTexture as MovieTexture;
            if (video.isPlaying)
            {
                video.Pause();
            }
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
               // Debug.Log("Received response");

                impressionWrapper = ImpressionWrapper.CreateFromJSON(uwr.downloadHandler.text);
            }
        }

        public void DetectClickThrough()
        {
            for (int i = 0; i < Input.touchCount; ++ i)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        //Debug.Log("hit.transform.name" + hit.transform.name);

                        if (hit.transform.parent == ASRUAdUnit.transform && hit.transform.name == "ASRUCTA")
                        {
                            //  Debug.Log("Clicked");

                            string click_url;

                            if (Application.platform == RuntimePlatform.Android)
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

                            Application.OpenURL(click_url);
                            var json = impressionWrapper.SaveToString();
                            var impressionId = impressionWrapper.impression.id;

                            StartCoroutine(PostImpression("", string.Format("https://adspecter-sandbox.herokuapp.com/impressions/{0}/clicked", impressionId)));
                        }
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
            else
            {
                return null;
            }
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
            //Debug.Log("appSetup: " + appSetup);
 //           Debug.Log("postData: " + postData);

            var url = "https://adspecter-sandbox.herokuapp.com/developer_app/authenticate";

            StartCoroutine(ASRUSetDeveloperKey(postData, url));
           // Debug.Log("done authentication");
        }


        IEnumerator ASRUSetDeveloperKey(string json, string url)
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            AdLoaderPlugIn adLoader = gameObject.AddComponent<AdLoaderPlugIn>();

            UnityWebRequest uwr = UnityWebRequest.Put(url, bodyRaw);
            uwr.method = "POST";
            uwr.SetRequestHeader("Content-Type", "application/json");
       
            yield return uwr.SendWebRequest();
            
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error while setting developer key: " + uwr.error);
            }
            else
            {
                //Debug.Log("Developer key set successfully");

                appSessionWrapper = AppSessionWrapper.CreateFromJSON(uwr.downloadHandler.text);

                loadAds = true;
            }
        }
    }
}
