using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using DataModels;

namespace AdSpecter
{
    [System.Serializable]
    public class DeveloperApp
    {
        public int id;
        public string name;
        public string developer_key;
        public User user;

        public DeveloperApp(string developerKey)
        {
            developer_key = developerKey;
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
        public string click_url;
        public string ad_unit_url;
        public bool active;
        public User user;

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

    [System.Serializable]
    public class Impression
    {
        public int id;
        public int ad_unit_id;
        public int developer_key;
        public int app_session_id;
        public bool served;
        public bool clicked;
        public bool shown;

        public Impression(int adUnitId, int developerKey, int appSessionId)
        {
            id = 0;
            served = true;
            clicked = false;
            shown = false;
            ad_unit_id = adUnitId;
            developer_key = developerKey;
            app_session_id = appSessionId;
        }

        public static Impression CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<Impression>(jsonString);
        }
    }

    [System.Serializable]
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




    [System.Serializable]
    public class AppSession
    {
        public int id;
        public int developer_key;

        public static AppSession CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AppSession>(jsonString);
        }
    }

    [System.Serializable]
    public class AppSessionWrapper
    {
        public AppSession app_session;

        public static AppSessionWrapper CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<AppSessionWrapper>(jsonString);
        }
    }

    [System.Serializable]
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

    [System.Serializable]
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

    [System.Serializable]
    public class AppSetupWrapper
    {
        public AppSetup developer_app;
        //
        //	public AppSetupWrapper(string clientAPIKey){
        //		developer_app = new AppSetup (clientAPIKey);
        //	}

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }


    public class AdLoaderPlugIn : MonoBehaviour
    {
        private GameObject ASRUAdPlane;

        private AdUnitWrapper adUnitWrapper;
        private ImpressionWrapper impressionWrapper;
        public bool startUpdate;

        void Start()
        {
            startUpdate = false;
        }

        public IEnumerator GetAdUnit(GameObject Plane)
        {
            ASRUAdPlane = Plane;
            //JSON in here
            UnityWebRequest uwr = UnityWebRequest.Get("https://adspecter-sandbox.herokuapp.com/ad_units/default");

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error while retrieving ad: " + uwr.error);
            }
            else
            {
                Debug.Log("Received ad unit");

                adUnitWrapper = AdUnitWrapper.CreateFromJSON(uwr.downloadHandler.text);
                //Debug.Log("Created From JSON");
                //StartCoroutine(GetTexture(adUnitWrapper.ad_unit.ad_unit_url));
                StartCoroutine(GetTexture("https://s3-us-west-1.amazonaws.com/adspecter-demo-video/Castle_Demo+(1).mov"));
                //Debug.Log("Got the texture");

            }
        }

        //called by getAdUnit
        /* IEnumerator GetTexture(string url)
         {
             //movie texture
             UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);

             //Debug.Log("got web request");
             yield return www.SendWebRequest();
             //Debug.Log("send web request");

             if (www.isNetworkError || www.isHttpError)
             {
                 Debug.Log("Error while getting ad texture:" + www.error);
             }
             else
             {
                 //Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                 Debug.Log("Received ad texture");

                 ASRUAdPlane.GetComponent<Renderer>().material.mainTexture = myTexture;
                 //(ASRUAdPlane.GetComponent<Renderer>().material.mainTexture).Play();

                 ASRUAdPlane.SetActive(true);

                 var impression = new Impression(adUnitWrapper.ad_unit.id,
                     AdSpecterConfigPlugIn.appSessionWrapper.app_session.developer_key,
                     AdSpecterConfigPlugIn.appSessionWrapper.app_session.id
                 );

                 impressionWrapper = new ImpressionWrapper();
                 impressionWrapper.impression = impression;

                 var json = impressionWrapper.SaveToString();

                 Debug.Log("line before post impression");
                 StartCoroutine(PostImpression(json, "https://adspecter-sandbox.herokuapp.com/impressions"));

                 Debug.Log("Ad was seen");
                 startUpdate = true;
             }
         } */

        IEnumerator GetTexture(string url)
        {
            //movie texture
            WWW www = new WWW(url);

            //Debug.Log("got web request");
            //yield return www.SendWebRequest();
            yield return www;
            //Debug.Log("send web request");

            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log("Error while getting ad texture:" + www.error);
            }
            else
            {
                MovieTexture myTexture = www.GetMovieTexture();
                Debug.Log("Received ad texture");

                ASRUAdPlane.GetComponent<Renderer>().material.mainTexture = myTexture;
                MovieTexture movie = ASRUAdPlane.GetComponent<Renderer>().material.mainTexture as MovieTexture;
                movie.Play();

                ASRUAdPlane.SetActive(true);

                var impression = new Impression(adUnitWrapper.ad_unit.id,
                    AdSpecterConfigPlugIn.appSessionWrapper.app_session.developer_key,
                    AdSpecterConfigPlugIn.appSessionWrapper.app_session.id
                );

                impressionWrapper = new ImpressionWrapper();
                impressionWrapper.impression = impression;

                var json = impressionWrapper.SaveToString();

                Debug.Log("line before post impression");
                StartCoroutine(PostImpression(json, "https://adspecter-sandbox.herokuapp.com/impressions"));

                Debug.Log("Ad was seen");
                startUpdate = true;
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
                Debug.Log("Received response");

                impressionWrapper = ImpressionWrapper.CreateFromJSON(uwr.downloadHandler.text);
            }
        }

        public void DetectClickThrough()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("clicked!");
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log("hit.transform.name" + hit.transform.name);

                    if (hit.transform.name == "ASRUAdPlane")
                    {
                        Debug.Log("Clicked");

                        Application.OpenURL(adUnitWrapper.ad_unit.click_url);
                        var json = impressionWrapper.SaveToString();
                        var impressionId = impressionWrapper.impression.id;

                        StartCoroutine(PostImpression("", string.Format("https://adspecter-sandbox.herokuapp.com/impressions/{0}/clicked", impressionId)));
                    }
                }
            }
        }
    }


    public class AdSpecterConfigPlugIn : MonoBehaviour
    {
        //private GameObject ASRUAdUnit;
        //public string developerKey;

        public static string appSessionId;
        public static AppSessionWrapper appSessionWrapper;

        public bool loadAds = false;

        public void AuthenticateUser(string developerKey)
        {
            var appSetup = new AppSetup(developerKey);
            var postData = appSetup.SaveToString();
            Debug.Log("appSetup: " + appSetup);
            Debug.Log("postData: " + postData);

            var url = "https://adspecter-sandbox.herokuapp.com/developer_app/authenticate";

            StartCoroutine(ASRUSetDeveloperKey(postData, url));
            //ASRUSetDeveloperKey(postData, url);
            Debug.Log("done authentication");
        }


        IEnumerator ASRUSetDeveloperKey(string json, string url)
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            AdLoaderPlugIn adLoader = gameObject.AddComponent<AdLoaderPlugIn>();

            UnityWebRequest uwr = UnityWebRequest.Put(url, bodyRaw);
            uwr.method = "POST";
            uwr.SetRequestHeader("Content-Type", "application/json");

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error while set request header " + uwr.error);
            }
            else
            {
                Debug.Log("sendwebrequest problem");
            }
            yield return uwr.SendWebRequest();
            //Debug.Log(uwr);

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log("Error while setting developer key: " + uwr.error);
            }
            else
            {
                Debug.Log("Developer key set successfully");

                appSessionWrapper = AppSessionWrapper.CreateFromJSON(uwr.downloadHandler.text);

                //GameObject.Find("ASRUScripts").GetComponent<AdLoaderPlugIn>().enabled = true;
                loadAds = true;

            }
        }
    }


}
